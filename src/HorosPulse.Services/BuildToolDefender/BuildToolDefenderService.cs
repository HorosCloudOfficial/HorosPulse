namespace HorosPulse.Services.BuildToolDefender;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Core.Scripts;
using HorosPulse.Data;

public sealed class BuildToolDefenderService : IBuildToolDefenderService
{
    private static readonly IReadOnlyList<(string ProcessName, string DisplayName, string Category)> DefaultDefinitions =
    [
        ("dotnet.exe", ".NET SDK / Runtime", ".NET"),
        ("MSBuild.exe", "MSBuild", ".NET"),
        ("csc.exe", "C# Compiler", ".NET"),
        ("VBCSCompiler.exe", "Roslyn Compiler Server", ".NET"),
        ("devenv.exe", "Visual Studio", "IDE"),
        ("node.exe", "Node.js", "JavaScript"),
        ("esbuild.exe", "esbuild", "JavaScript"),
        ("pwsh.exe", "PowerShell 7", "Shell"),
        ("powershell.exe", "Windows PowerShell", "Shell"),
        ("Cursor.exe", "Cursor IDE", "IDE"),
        ("Code.exe", "VS Code", "IDE"),
        ("rustc.exe", "Rust Compiler", "Rust"),
        ("cargo.exe", "Cargo", "Rust"),
        ("go.exe", "Go Toolchain", "Go"),
        ("python.exe", "Python", "Python"),
        ("docker.exe", "Docker Desktop", "Container"),
        ("com.docker.backend.exe", "Docker Backend", "Container"),
        ("wsl.exe", "WSL", "Container"),
        ("vmmemWSL", "WSL VM Memory", "Container"),
    ];

    private readonly IPowerShellBridge _powerShellBridge;
    private readonly ILogger<BuildToolDefenderService> _logger;
    private readonly string _trackingPath;

    public BuildToolDefenderService(IPowerShellBridge powerShellBridge, ILogger<BuildToolDefenderService> logger)
    {
        _powerShellBridge = powerShellBridge;
        _logger = logger;
        _trackingPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "build-tool-defender-exclusions-added.json");
    }

    public IReadOnlyList<BuildToolProcessEntry> GetDefaultProcesses() =>
        DefaultDefinitions.Select(d => new BuildToolProcessEntry
        {
            ProcessName = d.ProcessName,
            DisplayName = d.DisplayName,
            Category = d.Category,
            IsApplied = false,
            IsRecommended = true,
        }).ToList();

    public async Task<BuildToolDefenderState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        var current = await GetCurrentProcessExclusionsAsync(cancellationToken);
        var addedByApp = LoadAddedByApp();
        var currentSet = new HashSet<string>(current, StringComparer.OrdinalIgnoreCase);

        var entries = DefaultDefinitions.Select(d => new BuildToolProcessEntry
        {
            ProcessName = d.ProcessName,
            DisplayName = d.DisplayName,
            Category = d.Category,
            IsApplied = currentSet.Contains(d.ProcessName),
            IsRecommended = true,
        }).ToList();

        return new BuildToolDefenderState
        {
            Entries = entries,
            AddedByApp = addedByApp,
            AppliedCount = entries.Count(e => e.IsApplied),
            RecommendedCount = entries.Count,
        };
    }

    public async Task<OptimizationResult> ApplyExclusionsAsync(bool userConfirmed, CancellationToken cancellationToken = default)
    {
        if (!userConfirmed)
            return OptimizationResult.Fail("Build-Tool-Ausschlüsse erfordern ausdrückliche Zustimmung.");

        var current = await GetCurrentProcessExclusionsAsync(cancellationToken);
        var currentSet = new HashSet<string>(current, StringComparer.OrdinalIgnoreCase);
        var added = new List<string>();
        var errors = new List<string>();

        foreach (var definition in DefaultDefinitions)
        {
            if (currentSet.Contains(definition.ProcessName))
                continue;

            var script = PowerShellScriptLibrary.AddDefenderProcessExclusion(definition.ProcessName);
            var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);

            if (result.Success)
            {
                added.Add(definition.ProcessName);
                _logger.LogInformation("Defender-Prozess-Ausschluss hinzugefügt: {Process}", definition.ProcessName);
            }
            else
            {
                errors.Add($"{definition.ProcessName}: {result.StdErr}");
            }
        }

        if (added.Count == 0)
        {
            return errors.Count > 0
                ? OptimizationResult.Fail(string.Join("; ", errors))
                : OptimizationResult.Ok("Alle empfohlenen Prozess-Ausschlüsse sind bereits aktiv.");
        }

        var tracked = LoadAddedByApp().ToList();
        foreach (var processName in added)
        {
            if (!tracked.Contains(processName, StringComparer.OrdinalIgnoreCase))
                tracked.Add(processName);
        }

        SaveAddedByApp(tracked);
        var changes = added.Select(p => $"Prozess-Ausschluss: {p}").ToList();
        if (errors.Count > 0)
            changes.Add($"Warnungen: {string.Join("; ", errors)}");

        return OptimizationResult.Ok(changes.ToArray());
    }

    public async Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default)
    {
        var added = LoadAddedByApp();
        if (added.Count == 0)
            return OptimizationResult.Fail("Keine vom Tool hinzugefügten Prozess-Ausschlüsse.");

        var removed = new List<string>();
        foreach (var processName in added)
        {
            var script = PowerShellScriptLibrary.RemoveDefenderProcessExclusion(processName);
            var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);
            if (result.Success)
                removed.Add(processName);
        }

        SaveAddedByApp([]);
        return OptimizationResult.Ok(removed.Select(p => $"Entfernt: {p}").ToArray());
    }

    private async Task<IReadOnlyList<string>> GetCurrentProcessExclusionsAsync(CancellationToken cancellationToken)
    {
        var result = await _powerShellBridge.RunAsync(
            PowerShellScriptLibrary.GetDefenderProcessExclusions,
            elevated: true,
            cancellationToken: cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.StdOut))
            return Array.Empty<string>();

        return ParseJsonStringArray(result.StdOut.Trim());
    }

    internal static IReadOnlyList<string> ParseJsonStringArray(string json)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<string[]>(json, JsonDefaults.Options);
            if (parsed is not null)
                return parsed;
        }
        catch
        {
            // PowerShell returns a scalar string when only one exclusion exists.
        }

        try
        {
            var single = JsonSerializer.Deserialize<string>(json, JsonDefaults.Options);
            return single is not null ? [single] : Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private IReadOnlyList<string> LoadAddedByApp()
    {
        if (!File.Exists(_trackingPath))
            return Array.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_trackingPath), JsonDefaults.Options) ?? [];
    }

    private void SaveAddedByApp(IReadOnlyList<string> processes)
    {
        var dir = Path.GetDirectoryName(_trackingPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_trackingPath, JsonSerializer.Serialize(processes, JsonDefaults.Options));
    }
}
