namespace WindowsPerformance.Services.Indexer;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Core.Scripts;

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
            "WindowsPerformance", "indexer-exclusions-applied.json");
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

        var applied = new List<string>();
        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Indexer-Pfad existiert nicht: {Path}", path);
                continue;
            }

            var normalized = NormalizePath(path);
            var script = PowerShellScriptLibrary.BuildIndexerExclusionScript(normalized);
            var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);
            if (result.Success)
            {
                applied.Add(normalized);
                _logger.LogInformation("Indexer-Ausschluss: {Path}", normalized);
            }
            else
            {
                return OptimizationResult.Fail($"Indexer-Ausschluss fehlgeschlagen für {path}: {result.StdErr}");
            }
        }

        SaveAppliedPaths(applied);

        if (_appSettingsService.Current.RestartSearchServiceAfterIndexerChange && applied.Count > 0)
        {
            var restart = await _powerShellBridge.RunAsync(
                PowerShellScriptLibrary.RestartSearchService,
                elevated: true,
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

        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_statePath)) ?? [];
    }

    private void SaveAppliedPaths(IReadOnlyList<string> paths)
    {
        var dir = Path.GetDirectoryName(_statePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_statePath, JsonSerializer.Serialize(paths, new JsonSerializerOptions { WriteIndented = true }));
    }
}
