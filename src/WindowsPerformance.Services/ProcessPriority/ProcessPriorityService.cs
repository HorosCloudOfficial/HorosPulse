namespace WindowsPerformance.Services.ProcessPriority;

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class ProcessPriorityService : IProcessPriorityService
{
    private readonly ILogger<ProcessPriorityService> _logger;
    private readonly string _statePath;
    private Dictionary<int, ProcessPriorityLevel>? _savedPriorities;

    public ProcessPriorityService(ILogger<ProcessPriorityService> logger)
    {
        _logger = logger;
        _statePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsPerformance", "process-priority-state.json");
    }

    public Task<IReadOnlyList<ProcessPriorityRule>> GetDefaultRulesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProcessPriorityRule>>(
        [
            new ProcessPriorityRule { ProcessName = "Cursor", Priority = ProcessPriorityLevel.High, ApplyOnStartup = false },
            new ProcessPriorityRule { ProcessName = "Cursor Helper", Priority = ProcessPriorityLevel.BelowNormal, ApplyOnStartup = false },
        ]);

    public Task<OptimizationResult> ApplyCursorPrioritiesAsync(CancellationToken cancellationToken = default)
    {
        var changes = new List<string>();
        _savedPriorities ??= new Dictionary<int, ProcessPriorityLevel>();

        foreach (var process in Process.GetProcessesByName("Cursor"))
        {
            SaveAndSet(process, ProcessPriorityLevel.High, changes);
        }

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (process.ProcessName.StartsWith("Cursor", StringComparison.OrdinalIgnoreCase) &&
                    !process.ProcessName.Equals("Cursor", StringComparison.OrdinalIgnoreCase))
                {
                    SaveAndSet(process, ProcessPriorityLevel.BelowNormal, changes);
                }
            }
            catch
            {
                // Process may exit during enumeration.
            }
        }

        if (changes.Count == 0)
            return Task.FromResult(OptimizationResult.Fail("Keine Cursor-Prozesse gefunden. Bitte Cursor starten."));

        PersistState();
        _logger.LogInformation("Prozessprioritäten angewendet: {Count}", changes.Count);
        return Task.FromResult(OptimizationResult.Ok(changes.ToArray()));
    }

    public Task<OptimizationResult> RollbackCursorPrioritiesAsync(CancellationToken cancellationToken = default)
    {
        if (_savedPriorities is null || _savedPriorities.Count == 0)
            LoadState();

        if (_savedPriorities is null || _savedPriorities.Count == 0)
            return Task.FromResult(OptimizationResult.Fail("Kein Prioritäts-Snapshot vorhanden."));

        var changes = new List<string>();
        foreach (var (pid, priority) in _savedPriorities.ToList())
        {
            try
            {
                var process = Process.GetProcessById(pid);
                process.PriorityClass = ToProcessPriorityClass(priority);
                changes.Add($"PID {pid} → {priority}");
            }
            catch
            {
                // Process no longer running.
            }
        }

        _savedPriorities.Clear();
        PersistState();
        return Task.FromResult(OptimizationResult.Ok(changes.ToArray()));
    }

    public Task<OptimizationResult> SetPriorityAsync(int processId, ProcessPriorityLevel priority, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.PriorityClass = ToProcessPriorityClass(priority);
            return Task.FromResult(OptimizationResult.Ok($"PID {processId} → {priority}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OptimizationResult.Fail(ex.Message));
        }
    }

    public string? GetCursorProcessStatus()
    {
        var cursorProcesses = Process.GetProcessesByName("Cursor");
        if (cursorProcesses.Length == 0)
            return null;

        return string.Join(", ", cursorProcesses.Select(p => $"Cursor (PID {p.Id}, {p.PriorityClass})"));
    }

    private void SaveAndSet(Process process, ProcessPriorityLevel priority, List<string> changes)
    {
        try
        {
            _savedPriorities ??= new Dictionary<int, ProcessPriorityLevel>();
            if (!_savedPriorities.ContainsKey(process.Id))
                _savedPriorities[process.Id] = FromProcessPriorityClass(process.PriorityClass);

            process.PriorityClass = ToProcessPriorityClass(priority);
            changes.Add($"{process.ProcessName} (PID {process.Id}) → {priority}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Priorität für PID {Pid} konnte nicht gesetzt werden", process.Id);
        }
    }

    private void PersistState()
    {
        var dir = Path.GetDirectoryName(_statePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_savedPriorities ?? new Dictionary<int, ProcessPriorityLevel>());
        File.WriteAllText(_statePath, json);
    }

    private void LoadState()
    {
        if (!File.Exists(_statePath))
            return;

        _savedPriorities = JsonSerializer.Deserialize<Dictionary<int, ProcessPriorityLevel>>(File.ReadAllText(_statePath));
    }

    internal static ProcessPriorityClass ToProcessPriorityClass(ProcessPriorityLevel priority) =>
        priority switch
        {
            ProcessPriorityLevel.Idle => ProcessPriorityClass.Idle,
            ProcessPriorityLevel.BelowNormal => ProcessPriorityClass.BelowNormal,
            ProcessPriorityLevel.AboveNormal => ProcessPriorityClass.AboveNormal,
            ProcessPriorityLevel.High => ProcessPriorityClass.High,
            ProcessPriorityLevel.RealTime => ProcessPriorityClass.RealTime,
            _ => ProcessPriorityClass.Normal,
        };

    internal static ProcessPriorityLevel FromProcessPriorityClass(ProcessPriorityClass priority) =>
        priority switch
        {
            ProcessPriorityClass.Idle => ProcessPriorityLevel.Idle,
            ProcessPriorityClass.BelowNormal => ProcessPriorityLevel.BelowNormal,
            ProcessPriorityClass.AboveNormal => ProcessPriorityLevel.AboveNormal,
            ProcessPriorityClass.High => ProcessPriorityLevel.High,
            ProcessPriorityClass.RealTime => ProcessPriorityLevel.RealTime,
            _ => ProcessPriorityLevel.Normal,
        };
}
