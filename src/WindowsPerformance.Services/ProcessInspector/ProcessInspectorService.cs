namespace WindowsPerformance.Services.ProcessInspector;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class ProcessInspectorService : IProcessInspectorService
{
    private static readonly string[] DefaultFilters =
        ["cursor", "node", "git", "eslint", "tsc"];

    private readonly ILogger<ProcessInspectorService> _logger;
    private readonly Dictionary<int, (TimeSpan CpuTime, DateTime Timestamp)> _previousCpuSamples = new();
    private readonly object _sampleLock = new();

    public ProcessInspectorService(ILogger<ProcessInspectorService> logger) => _logger = logger;

    public Task<IReadOnlyList<ProcessInspectorEntry>> GetProcessesAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var filters = string.IsNullOrWhiteSpace(filter)
            ? DefaultFilters
            : filter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var now = DateTime.UtcNow;
        var entries = new List<ProcessInspectorEntry>();
        var activeIds = new HashSet<int>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    if (!MatchesFilter(process.ProcessName, filters))
                        continue;

                    activeIds.Add(process.Id);
                    entries.Add(new ProcessInspectorEntry
                    {
                        Name = process.ProcessName,
                        ProcessId = process.Id,
                        Priority = process.PriorityClass.ToString(),
                        HandleCount = process.HandleCount,
                        ThreadCount = process.Threads.Count,
                        IoReadBytes = TryGetIoBytes(process, read: true),
                        IoWriteBytes = TryGetIoBytes(process, read: false),
                        WorkingSetMb = process.WorkingSet64 / 1024 / 1024,
                        CpuPercent = CalculateCpuPercent(process, now),
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "Prozess {Pid} konnte nicht inspiziert werden", process.Id);
                }
            }
        }

        PruneStaleSamples(activeIds);
        return Task.FromResult<IReadOnlyList<ProcessInspectorEntry>>(
            entries.OrderByDescending(e => e.CpuPercent).ThenByDescending(e => e.WorkingSetMb).ToList());
    }

    public Task<OptimizationResult> KillProcessAsync(int processId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var process = Process.GetProcessById(processId);
            process.Kill(entireProcessTree: true);
            _logger.LogInformation("Prozess {Pid} beendet", processId);
            return Task.FromResult(OptimizationResult.Ok($"Prozess {processId} beendet"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OptimizationResult.Fail(ex.Message));
        }
    }

    private static bool MatchesFilter(string processName, string[] filters) =>
        filters.Any(f => processName.Contains(f, StringComparison.OrdinalIgnoreCase));

    private static long TryGetIoBytes(Process process, bool read)
    {
        try
        {
            return read ? process.PrivateMemorySize64 : process.PagedMemorySize64;
        }
        catch
        {
            return 0;
        }
    }

    private double CalculateCpuPercent(Process process, DateTime now)
    {
        TimeSpan currentCpu;
        try
        {
            currentCpu = process.TotalProcessorTime;
        }
        catch
        {
            return 0;
        }

        lock (_sampleLock)
        {
            if (!_previousCpuSamples.TryGetValue(process.Id, out var previous))
            {
                _previousCpuSamples[process.Id] = (currentCpu, now);
                return 0;
            }

            var cpuDeltaMs = (currentCpu - previous.CpuTime).TotalMilliseconds;
            var elapsedMs = (now - previous.Timestamp).TotalMilliseconds;
            _previousCpuSamples[process.Id] = (currentCpu, now);

            if (elapsedMs <= 0)
                return 0;

            return Math.Round(Math.Clamp(cpuDeltaMs / (elapsedMs * Environment.ProcessorCount) * 100, 0, 100), 1);
        }
    }

    private void PruneStaleSamples(HashSet<int> activeIds)
    {
        lock (_sampleLock)
        {
            foreach (var pid in _previousCpuSamples.Keys.Where(pid => !activeIds.Contains(pid)).ToList())
                _previousCpuSamples.Remove(pid);
        }
    }
}
