namespace WindowsPerformance.Services.Defender;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class DefenderExclusionService : IDefenderExclusionService
{
    private readonly IPowerShellBridge _powerShellBridge;
    private readonly ILogger<DefenderExclusionService> _logger;
    private readonly string _trackingPath;

    public DefenderExclusionService(IPowerShellBridge powerShellBridge, ILogger<DefenderExclusionService> logger)
    {
        _powerShellBridge = powerShellBridge;
        _logger = logger;
        _trackingPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsPerformance", "defender-exclusions-added.json");
    }

    public IReadOnlyList<string> GetDefaultPaths()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return
        [
            Path.Combine(appData, "Cursor"),
            Path.Combine(localAppData, "cursor-updater"),
            Path.Combine(localAppData, "Programs", "cursor"),
        ];
    }

    public async Task<DefenderExclusionSet> GetExclusionSetAsync(CancellationToken cancellationToken = default)
    {
        var current = await GetCurrentExclusionsAsync(cancellationToken);
        var addedByApp = LoadAddedByApp();

        return new DefenderExclusionSet
        {
            CurrentExclusions = current,
            AddedByApp = addedByApp,
            DefaultPaths = GetDefaultPaths(),
        };
    }

    public async Task<OptimizationResult> ApplyExclusionsAsync(bool userConfirmed, CancellationToken cancellationToken = default)
    {
        if (!userConfirmed)
            return OptimizationResult.Fail("Defender-Ausschlüsse erfordern ausdrückliche Zustimmung.");

        var paths = GetDefaultPaths();
        var added = new List<string>();
        var errors = new List<string>();

        foreach (var path in paths)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                _logger.LogWarning("Defender-Pfad existiert nicht, übersprungen: {Path}", path);
                continue;
            }

            var escaped = path.Replace("'", "''", StringComparison.Ordinal);
            var script = $"Add-MpPreference -ExclusionPath '{escaped}'";
            var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);

            if (result.Success)
            {
                added.Add(path);
                _logger.LogInformation("Defender-Ausschluss hinzugefügt: {Path}", path);
            }
            else
            {
                errors.Add($"{path}: {result.StdErr}");
            }
        }

        if (added.Count == 0)
            return OptimizationResult.Fail(errors.Count > 0 ? string.Join("; ", errors) : "Keine Ausschlüsse hinzugefügt.");

        SaveAddedByApp(added);
        return OptimizationResult.Ok(added.Select(p => $"Ausschluss: {p}").ToArray());
    }

    public async Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default)
    {
        var added = LoadAddedByApp();
        if (added.Count == 0)
            return OptimizationResult.Fail("Keine vom Tool hinzugefügten Ausschlüsse.");

        var removed = new List<string>();
        foreach (var path in added)
        {
            var escaped = path.Replace("'", "''", StringComparison.Ordinal);
            var script = $"Remove-MpPreference -ExclusionPath '{escaped}'";
            var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);
            if (result.Success)
                removed.Add(path);
        }

        SaveAddedByApp([]);
        return OptimizationResult.Ok(removed.Select(p => $"Entfernt: {p}").ToArray());
    }

    private async Task<IReadOnlyList<string>> GetCurrentExclusionsAsync(CancellationToken cancellationToken)
    {
        var result = await _powerShellBridge.RunAsync(
            "(Get-MpPreference).ExclusionPath | ConvertTo-Json -Compress",
            elevated: true,
            cancellationToken: cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.StdOut))
            return Array.Empty<string>();

        try
        {
            var parsed = JsonSerializer.Deserialize<string[]>(result.StdOut.Trim());
            return parsed ?? Array.Empty<string>();
        }
        catch
        {
            var single = JsonSerializer.Deserialize<string>(result.StdOut.Trim());
            return single is not null ? [single] : Array.Empty<string>();
        }
    }

    private IReadOnlyList<string> LoadAddedByApp()
    {
        if (!File.Exists(_trackingPath))
            return Array.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_trackingPath)) ?? [];
    }

    private void SaveAddedByApp(IReadOnlyList<string> paths)
    {
        var dir = Path.GetDirectoryName(_trackingPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_trackingPath, JsonSerializer.Serialize(paths, new JsonSerializerOptions { WriteIndented = true }));
    }
}
