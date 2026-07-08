namespace HorosPulse.Services.Indexer;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Core.Scripts;
using HorosPulse.Data;

public sealed class IndexerExclusionService : IIndexerExclusionService
{
    private readonly IPowerShellBridge _powerShellBridge;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger<IndexerExclusionService> _logger;
    private readonly string _statePath;

    public IndexerExclusionService(
        IPowerShellBridge powerShellBridge,
        IAppSettingsService appSettingsService,
        ILogger<IndexerExclusionService> logger)
    {
        _powerShellBridge = powerShellBridge;
        _appSettingsService = appSettingsService;
        _logger = logger;
        _statePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "indexer-exclusions-applied.json");
    }

    public Task<IReadOnlyList<IndexerExcludeEntry>> GetAvailableEntriesAsync(CancellationToken cancellationToken = default)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var applied = LoadAppliedPaths();

        var defaults = new List<(string Path, bool Selected)>
        {
            (Path.Combine(userProfile, "node_modules"), true),
            (Path.Combine(userProfile, "source", "repos"), false),
            (Path.Combine(userProfile, "Documents", "dev"), false),
            (Path.Combine(appData, "Cursor"), true),
            (Path.Combine(appData, "Code"), false),
        };

        var entries = defaults
            .Select(d => new IndexerExcludeEntry
            {
                Path = d.Path,
                IsSelected = d.Selected,
                IsDefault = true,
                IsApplied = applied.Contains(NormalizePath(d.Path)),
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<IndexerExcludeEntry>>(entries);
    }

    public async Task<OptimizationResult> ApplyExclusionsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default)
    {
        if (paths.Count == 0)
            return OptimizationResult.Fail("Keine Ordner ausgewählt.");

        var previouslyApplied = LoadAppliedPaths()
            .Select(NormalizePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var applied = new List<string>(previouslyApplied);
        var indexerTimeout = TimeSpan.FromSeconds(
            Math.Clamp(_appSettingsService.Current.PowerShellTimeoutSeconds * 2, 60, 600));

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Indexer-Pfad existiert nicht, übersprungen: {Path}", path);
                continue;
            }

            var normalized = NormalizePath(path);
            if (previouslyApplied.Contains(normalized))
            {
                _logger.LogInformation("Indexer-Ausschluss bereits angewendet: {Path}", normalized);
                continue;
            }

            var script = PowerShellScriptLibrary.BuildIndexerExclusionScript(normalized);
            var result = await RunIndexerScriptWithRetryAsync(script, indexerTimeout, cancellationToken);
            if (result.Success)
            {
                applied.Add(normalized);
                previouslyApplied.Add(normalized);
                _logger.LogInformation("Indexer-Ausschluss: {Path}", normalized);
            }
            else
            {
                return OptimizationResult.Fail(FormatIndexerFailure(path, result.StdErr));
            }
        }

        SaveAppliedPaths(applied.Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        if (_appSettingsService.Current.RestartSearchServiceAfterIndexerChange && applied.Count > 0)
        {
            var restart = await _powerShellBridge.RunAsync(
                PowerShellScriptLibrary.RestartSearchService,
                elevated: true,
                timeout: indexerTimeout,
                cancellationToken: cancellationToken);
            if (!restart.Success)
            {
                return OptimizationResult.Fail(
                    $"Ausschlüsse angewendet, aber WSearch-Neustart fehlgeschlagen: {restart.StdErr}");
            }

            return OptimizationResult.Ok(
                applied.Select(p => $"Ausgeschlossen: {p}").Append("Windows-Suchdienst neu gestartet").ToArray());
        }

        return OptimizationResult.Ok(applied.Select(p => $"Ausgeschlossen: {p}").ToArray());
    }

    private async Task<PowerShellResult> RunIndexerScriptWithRetryAsync(
        string script,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var result = await _powerShellBridge.RunAsync(script, elevated: true, timeout: timeout, cancellationToken: cancellationToken);
        if (result.Success || !IsTransientElevationFailure(result.StdErr))
            return result;

        _logger.LogWarning("Indexer-Skript Zeitüberschreitung, einmaliger Wiederholungsversuch…");
        await Task.Delay(1500, cancellationToken);
        return await _powerShellBridge.RunAsync(script, elevated: true, timeout: timeout, cancellationToken: cancellationToken);
    }

    private static bool IsTransientElevationFailure(string? stderr) =>
        !string.IsNullOrWhiteSpace(stderr) &&
        (stderr.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
         stderr.Contains("Zeitüberschreitung", StringComparison.OrdinalIgnoreCase) ||
         stderr.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ||
         stderr.Contains("nicht erreichbar", StringComparison.OrdinalIgnoreCase));

    private static string FormatIndexerFailure(string path, string? stderr)
    {
        if (IsTransientElevationFailure(stderr))
        {
            return $"Indexer-Ausschluss für {path} fehlgeschlagen (Elevation-Timeout). " +
                   "Bitte HorosPulse neu starten, UAC bestätigen und erneut versuchen.";
        }

        return $"Indexer-Ausschluss fehlgeschlagen für {path}: {stderr}";
    }

    public async Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default)
    {
        var applied = LoadAppliedPaths();
        if (applied.Count == 0)
            return OptimizationResult.Fail("Keine angewendeten Indexer-Ausschlüsse.");

        var result = await _powerShellBridge.RunAsync(
            PowerShellScriptLibrary.IndexerExclusionRollback,
            elevated: true,
            cancellationToken: cancellationToken);
        if (!result.Success)
            return OptimizationResult.Fail(result.StdErr);

        SaveAppliedPaths([]);
        return OptimizationResult.Ok(applied.Select(p => $"Entfernt: {p}").ToArray());
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private IReadOnlyList<string> LoadAppliedPaths()
    {
        if (!File.Exists(_statePath))
            return Array.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_statePath), JsonDefaults.Options) ?? [];
    }

    private void SaveAppliedPaths(IReadOnlyList<string> paths)
    {
        var dir = Path.GetDirectoryName(_statePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_statePath, JsonSerializer.Serialize(paths, JsonDefaults.Options));
    }
}
