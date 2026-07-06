namespace HorosPulse.Services.Startup;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class StartupManagerService : IStartupManagerService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string DisabledSuffix = "_HorosPulse_Disabled";

    private readonly ILogger<StartupManagerService> _logger;
    private readonly string _snapshotPath;

    public StartupManagerService(ILogger<StartupManagerService> logger)
    {
        _logger = logger;
        _snapshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "startup-entries-snapshot.json");
    }

    public Task<IReadOnlyList<StartupEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entries = new List<StartupEntry>();

        entries.AddRange(ReadRegistryRun(Registry.CurrentUser, "HKCU"));
        entries.AddRange(ReadRegistryRun(Registry.LocalMachine, "HKLM"));
        entries.AddRange(ReadStartupFolder());

        return Task.FromResult<IReadOnlyList<StartupEntry>>(entries.OrderBy(e => e.Name).ToList());
    }

    public Task<OptimizationResult> SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveSnapshotIfNeeded();

        try
        {
            if (entry.Source == "StartupFolder")
            {
                var path = entry.Command;
                if (!File.Exists(path) && !Directory.Exists(path))
                    return Task.FromResult(OptimizationResult.Fail($"Datei nicht gefunden: {path}"));

                var disabledPath = path + DisabledSuffix;
                if (enabled)
                {
                    if (File.Exists(disabledPath))
                        File.Move(disabledPath, path);
                }
                else if (File.Exists(path))
                {
                    File.Move(path, disabledPath);
                }

                return Task.FromResult(OptimizationResult.Ok($"{entry.Name}: {(enabled ? "aktiviert" : "deaktiviert")}"));
            }

            if (entry.RegistryKey is null || entry.RegistryValueName is null)
                return Task.FromResult(OptimizationResult.Fail("Registry-Eintrag unvollständig."));

            var hive = entry.Source == "HKLM" ? Registry.LocalMachine : Registry.CurrentUser;
            using var key = hive.OpenSubKey(entry.RegistryKey, writable: true)
                ?? throw new InvalidOperationException($"Registry-Schlüssel nicht gefunden: {entry.RegistryKey}");

            if (enabled)
            {
                var disabledName = entry.RegistryValueName + DisabledSuffix;
                var disabledValue = key.GetValue(disabledName) as string;
                if (disabledValue is not null)
                {
                    key.SetValue(entry.RegistryValueName, disabledValue);
                    key.DeleteValue(disabledName, throwOnMissingValue: false);
                }
            }
            else
            {
                var value = key.GetValue(entry.RegistryValueName) as string;
                if (value is not null)
                {
                    key.SetValue(entry.RegistryValueName + DisabledSuffix, value);
                    key.DeleteValue(entry.RegistryValueName, throwOnMissingValue: false);
                }
            }

            _logger.LogInformation("Startup {Name} → {Enabled}", entry.Name, enabled);
            return Task.FromResult(OptimizationResult.Ok($"{entry.Name}: {(enabled ? "aktiviert" : "deaktiviert")}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OptimizationResult.Fail(ex.Message));
        }
    }

    public async Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_snapshotPath))
            return OptimizationResult.Fail("Kein Startup-Snapshot vorhanden.");

        var json = await File.ReadAllTextAsync(_snapshotPath, cancellationToken);
        var snapshot = JsonSerializer.Deserialize<List<StartupEntry>>(json) ?? [];
        var restored = new List<string>();

        foreach (var entry in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await SetEnabledAsync(entry, entry.IsEnabled, cancellationToken);
            if (result.Success)
                restored.Add(entry.Name);
        }

        File.Delete(_snapshotPath);
        return OptimizationResult.Ok(restored.Select(n => $"Wiederhergestellt: {n}").ToArray());
    }

    private static IEnumerable<StartupEntry> ReadRegistryRun(RegistryKey hive, string source)
    {
        using var key = hive.OpenSubKey(RunKeyPath);
        if (key is null)
            yield break;

        foreach (var valueName in key.GetValueNames())
        {
            if (valueName.EndsWith(DisabledSuffix, StringComparison.Ordinal))
                continue;

            var disabledName = valueName + DisabledSuffix;
            var isEnabled = key.GetValue(disabledName) is null;
            var command = (isEnabled ? key.GetValue(valueName) : key.GetValue(disabledName)) as string ?? string.Empty;

            yield return new StartupEntry
            {
                Name = valueName,
                Command = command,
                Source = source,
                RegistryKey = RunKeyPath,
                RegistryValueName = valueName,
                IsEnabled = isEnabled,
            };
        }
    }

    private static IEnumerable<StartupEntry> ReadStartupFolder()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (!Directory.Exists(folder))
            yield break;

        foreach (var file in Directory.GetFiles(folder))
        {
            var disabled = file.EndsWith(DisabledSuffix, StringComparison.Ordinal);
            var displayName = Path.GetFileName(disabled ? file[..^DisabledSuffix.Length] : file);
            yield return new StartupEntry
            {
                Name = displayName,
                Command = disabled ? file[..^DisabledSuffix.Length] : file,
                Source = "StartupFolder",
                IsEnabled = !disabled,
            };
        }
    }

    private void SaveSnapshotIfNeeded()
    {
        if (File.Exists(_snapshotPath))
            return;

        var entries = ReadRegistryRun(Registry.CurrentUser, "HKCU")
            .Concat(ReadRegistryRun(Registry.LocalMachine, "HKLM"))
            .Concat(ReadStartupFolder())
            .ToList();

        Directory.CreateDirectory(Path.GetDirectoryName(_snapshotPath)!);
        File.WriteAllText(_snapshotPath, JsonSerializer.Serialize(entries));
    }
}
