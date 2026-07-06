namespace HorosPulse.Services.Defender;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Core.Scripts;

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
            "HorosPulse", "defender-exclusions-added.json");
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
        var defaultPaths = GetDefaultPaths();
        var validations = ValidatePaths(defaultPaths);

        return new DefenderExclusionSet
        {
            CurrentExclusions = current,
            AddedByApp = addedByApp,
            DefaultPaths = defaultPaths,
            PathValidations = validations,
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
            var validation = ValidatePath(path);
            if (!validation.Exists)
            {
                errors.Add($"{path}: {validation.Message}");
                continue;
            }

            if (!validation.DriveValid)
            {
                errors.Add($"{path}: {validation.Message}");
                continue;
            }

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                _logger.LogWarning("Defender-Pfad existiert nicht, übersprungen: {Path}", path);
                continue;
            }

            var script = PowerShellScriptLibrary.AddDefenderExclusion(path);
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
            var script = PowerShellScriptLibrary.RemoveDefenderExclusion(path);
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
            PowerShellScriptLibrary.GetDefenderExclusions,
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

    internal static IReadOnlyList<PathValidationResult> ValidatePaths(IReadOnlyList<string> paths) =>
        paths.Select(ValidatePath).ToList();

    internal static PathValidationResult ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new PathValidationResult
            {
                Path = path,
                Exists = false,
                DriveValid = false,
                Message = "Pfad ist leer.",
            };
        }

        try
        {
            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath);
            var driveValid = !string.IsNullOrEmpty(root) && Directory.Exists(root);
            var exists = Directory.Exists(fullPath) || File.Exists(fullPath);

            return new PathValidationResult
            {
                Path = fullPath,
                Exists = exists,
                DriveValid = driveValid,
                Message = exists
                    ? "OK"
                    : driveValid
                        ? "Pfad existiert nicht (Laufwerk gültig)."
                        : "Laufwerk nicht verfügbar oder Pfad ungültig.",
            };
        }
        catch (Exception ex)
        {
            return new PathValidationResult
            {
                Path = path,
                Exists = false,
                DriveValid = false,
                Message = ex.Message,
            };
        }
    }
}
