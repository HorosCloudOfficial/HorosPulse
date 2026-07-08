namespace HorosPulse.Services.DevTempCleanup;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class DevTempCleanupService : IDevTempCleanupService
{
    private readonly IDirectorySizeCalculator _sizeCalculator;
    private readonly ILogger<DevTempCleanupService> _logger;

    public DevTempCleanupService(
        IDirectorySizeCalculator sizeCalculator,
        ILogger<DevTempCleanupService> logger)
    {
        _sizeCalculator = sizeCalculator;
        _logger = logger;
    }

    public async Task<DevTempCleanupScanResult> ScanAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entries = new List<DevTempCacheEntry>();

        foreach (var definition in DevTempCleanupPathCatalog.Definitions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = await BuildEntryAsync(definition, cancellationToken);
            entries.Add(entry);
        }

        var totalSafe = entries.Where(e => e.IsDeletable).Sum(e => e.SizeBytes);

        _logger.LogDebug(
            "Dev-Cache-Scan: {Count} Einträge, {SafeBytes} Bytes sicher löschbar",
            entries.Count,
            totalSafe);

        return new DevTempCleanupScanResult
        {
            Entries = entries,
            TotalSafeDeletableBytes = totalSafe,
        };
    }

    public async Task<DevTempCleanupResult> CleanupAsync(
        IReadOnlyList<string> entryIds,
        bool allowGlobalPackages = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (entryIds.Count == 0)
            return DevTempCleanupResult.Fail("Keine Cache-Einträge ausgewählt.");

        var scan = await ScanAsync(cancellationToken);
        var selected = scan.Entries.Where(e => entryIds.Contains(e.Id, StringComparer.Ordinal)).ToList();

        if (selected.Count == 0)
            return DevTempCleanupResult.Fail("Keine gültigen Einträge für die Bereinigung gefunden.");

        long bytesBefore = 0;
        var messages = new List<string>();

        foreach (var entry in selected)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.Safety == DevTempCacheSafety.InfoOnly)
            {
                return DevTempCleanupResult.Fail(
                    $"'{entry.DisplayName}' ist nur zur Information — Bereinigung nicht erlaubt.");
            }

            if (entry.RequiresExtraConfirmation && !allowGlobalPackages)
            {
                return DevTempCleanupResult.Fail(
                    $"'{entry.DisplayName}' erfordert eine explizite Extra-Bestätigung.");
            }

            if (!entry.IsDeletable && entry.CleanupMethod == DevTempCacheCleanupMethod.None)
            {
                return DevTempCleanupResult.Fail(
                    $"'{entry.DisplayName}' kann nicht bereinigt werden.");
            }

            bytesBefore += entry.SizeBytes;

            var message = entry.CleanupMethod switch
            {
                DevTempCacheCleanupMethod.DirectoryContents =>
                    CleanupDirectoryContents(entry),
                DevTempCacheCleanupMethod.DotNetNugetHttpCache =>
                    RunDotNetNugetClear("http-cache", entry.DisplayName),
                DevTempCacheCleanupMethod.DotNetNugetGlobalPackages =>
                    RunDotNetNugetClear("global-packages", entry.DisplayName),
                DevTempCacheCleanupMethod.PnpmStorePrune =>
                    RunPnpmStorePrune(entry),
                _ => $"'{entry.DisplayName}': keine Bereinigungsmethode.",
            };

            messages.Add(message);
        }

        // Re-scan to estimate freed bytes
        var afterScan = await ScanAsync(cancellationToken);
        var bytesAfter = afterScan.Entries
            .Where(e => entryIds.Contains(e.Id, StringComparer.Ordinal))
            .Sum(e => e.SizeBytes);

        var bytesFreed = Math.Max(0, bytesBefore - bytesAfter);

        _logger.LogInformation(
            "Dev-Cache-Bereinigung abgeschlossen: {Freed} Bytes freigegeben",
            bytesFreed);

        return DevTempCleanupResult.Ok(bytesFreed, messages.ToArray());
    }

    internal async Task<DevTempCacheEntry> BuildEntryAsync(
        DevTempCleanupPathCatalog.PathDefinition definition,
        CancellationToken cancellationToken)
    {
        var path = ResolvePath(definition);
        var exists = definition.CleanupMethod is DevTempCacheCleanupMethod.DotNetNugetHttpCache
            or DevTempCacheCleanupMethod.DotNetNugetGlobalPackages
            ? IsDotNetAvailable()
            : _sizeCalculator.PathExists(path);

        long sizeBytes = 0;
        if (exists)
        {
            sizeBytes = definition.CleanupMethod switch
            {
                DevTempCacheCleanupMethod.DotNetNugetHttpCache =>
                    await GetDotNetNugetLocalSizeAsync("http-cache", cancellationToken),
                DevTempCacheCleanupMethod.DotNetNugetGlobalPackages =>
                    await GetDotNetNugetLocalSizeAsync("global-packages", cancellationToken),
                _ when !string.IsNullOrWhiteSpace(path) =>
                    await Task.Run(() => _sizeCalculator.GetDirectorySizeBytes(path, cancellationToken), cancellationToken),
                _ => 0,
            };
        }

        return new DevTempCacheEntry
        {
            Id = definition.Id,
            DisplayName = definition.DisplayName,
            Path = string.IsNullOrWhiteSpace(path) ? GetDotNetLocalsHint(definition.CleanupMethod) : path,
            SizeBytes = sizeBytes,
            PathExists = exists,
            Safety = definition.Safety,
            CleanupMethod = definition.CleanupMethod,
            SafetyReason = definition.SafetyReason,
        };
    }

    internal static string ResolvePath(DevTempCleanupPathCatalog.PathDefinition definition) =>
        string.IsNullOrWhiteSpace(definition.PathTemplate)
            ? string.Empty
            : DevTempCleanupPathCatalog.ExpandPath(definition.PathTemplate);

    internal static string CleanupDirectoryContents(DevTempCacheEntry entry)
    {
        if (!DevTempCleanupPathCatalog.IsPathSafeForDeletion(entry.Path, entry.Path))
            throw new InvalidOperationException($"Pfad nicht für Löschung freigegeben: {entry.Path}");

        if (!Directory.Exists(entry.Path))
            return $"'{entry.DisplayName}': nicht vorhanden — übersprungen.";

        var deleted = DeleteDirectoryContents(entry.Path);
        return $"'{entry.DisplayName}': {deleted} Element(e) entfernt.";
    }

    internal static int DeleteDirectoryContents(string directoryPath)
    {
        var count = 0;

        foreach (var file in Directory.EnumerateFiles(directoryPath))
        {
            try
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
                count++;
            }
            catch
            {
                // Skip locked files
            }
        }

        foreach (var dir in Directory.EnumerateDirectories(directoryPath))
        {
            try
            {
                Directory.Delete(dir, recursive: true);
                count++;
            }
            catch
            {
                try
                {
                    count += DeleteDirectoryContents(dir);
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir, recursive: false);
                        count++;
                    }
                }
                catch
                {
                    // Skip locked directories
                }
            }
        }

        return count;
    }

    internal static string RunDotNetNugetClear(string localName, string displayName)
    {
        if (!IsDotNetAvailable())
            throw new InvalidOperationException("dotnet SDK nicht gefunden — NuGet-Cache kann nicht geleert werden.");

        var (exitCode, output, error) = RunProcess("dotnet", $"nuget locals {localName} --clear");
        if (exitCode != 0)
            throw new InvalidOperationException(
                $"dotnet nuget locals {localName} --clear fehlgeschlagen: {error ?? output}");

        return $"'{displayName}': NuGet-{localName} geleert.";
    }

    internal static string RunPnpmStorePrune(DevTempCacheEntry entry)
    {
        if (!IsCommandAvailable("pnpm"))
        {
            if (!string.IsNullOrWhiteSpace(entry.Path) &&
                DevTempCleanupPathCatalog.IsPathSafeForDeletion(entry.Path, entry.Path) &&
                Directory.Exists(entry.Path))
            {
                var deleted = DeleteDirectoryContents(entry.Path);
                return $"'{entry.DisplayName}': pnpm nicht gefunden — {deleted} Element(e) im Store-Ordner entfernt.";
            }

            return $"'{entry.DisplayName}': pnpm nicht installiert — übersprungen.";
        }

        var (exitCode, output, error) = RunProcess("pnpm", "store prune");
        if (exitCode != 0)
            throw new InvalidOperationException($"pnpm store prune fehlgeschlagen: {error ?? output}");

        return $"'{entry.DisplayName}': pnpm store prune ausgeführt.";
    }

    internal static async Task<long> GetDotNetNugetLocalSizeAsync(string localName, CancellationToken cancellationToken)
    {
        if (!IsDotNetAvailable())
            return 0;

        cancellationToken.ThrowIfCancellationRequested();

        var (exitCode, output, _) = await Task.Run(
            () => RunProcess("dotnet", $"nuget locals {localName} -l"),
            cancellationToken);

        if (exitCode != 0 || string.IsNullOrWhiteSpace(output))
            return 0;

        // Output format: "http-cache: C:\path" — size via directory
        var pathLine = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(l => l.Contains(localName, StringComparison.OrdinalIgnoreCase));

        if (pathLine is null)
            return 0;

        var colonIndex = pathLine.IndexOf(':');
        if (colonIndex < 0 || colonIndex >= pathLine.Length - 1)
            return 0;

        var path = pathLine[(colonIndex + 1)..].Trim();
        if (!Directory.Exists(path))
            return 0;

        return new DirectorySizeCalculator().GetDirectorySizeBytes(path, cancellationToken);
    }

    internal static bool IsDotNetAvailable() => IsCommandAvailable("dotnet");

    internal static bool IsCommandAvailable(string command)
    {
        try
        {
            var (exitCode, _, _) = RunProcess(command, "--version");
            return exitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    internal static (int ExitCode, string Output, string? Error) RunProcess(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Prozess konnte nicht gestartet werden: {fileName}");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, string.IsNullOrWhiteSpace(error) ? null : error);
    }

    private static string GetDotNetLocalsHint(DevTempCacheCleanupMethod method) => method switch
    {
        DevTempCacheCleanupMethod.DotNetNugetHttpCache => "(dotnet nuget locals http-cache)",
        DevTempCacheCleanupMethod.DotNetNugetGlobalPackages => "(dotnet nuget locals global-packages)",
        _ => "—",
    };
}
