namespace WindowsPerformance.Services.Monitoring;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class ProcessMonitorService : IProcessMonitorService
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger<ProcessMonitorService> _logger;
    private readonly Dictionary<int, (TimeSpan CpuTime, DateTime Timestamp)> _previousCpuSamples = new();
    private readonly object _sampleLock = new();
    private Timer? _pollingTimer;
    private bool _disposed;

    public ProcessMonitorService(
        IAppSettingsService appSettingsService,
        ILogger<ProcessMonitorService> logger)
    {
        _appSettingsService = appSettingsService;
        _logger = logger;
    }

    public event EventHandler<IReadOnlyList<ProcessMonitorEntry>>? ProcessesUpdated;

    public Task<IReadOnlyList<ProcessMonitorEntry>> GetProcessesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entries = CollectProcessEntries();
        return Task.FromResult<IReadOnlyList<ProcessMonitorEntry>>(entries);
    }

    public void StartPolling()
    {
        if (_pollingTimer is not null)
            return;

        var interval = Math.Max(1000, _appSettingsService.Current.ProcessMonitorRefreshIntervalMs);
        _pollingTimer = new Timer(
            async _ => await PollAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(interval));
    }

    public void StopPolling()
    {
        _pollingTimer?.Dispose();
        _pollingTimer = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopPolling();
        _disposed = true;
    }

    private async Task PollAsync()
    {
        try
        {
            var entries = await GetProcessesAsync();
            ProcessesUpdated?.Invoke(this, entries);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Prozess-Polling fehlgeschlagen");
        }
    }

    private List<ProcessMonitorEntry> CollectProcessEntries()
    {
        var now = DateTime.UtcNow;
        var entries = new List<ProcessMonitorEntry>();
        var activeIds = new HashSet<int>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    var cpuPercent = CalculateCpuPercent(process, now);
                    activeIds.Add(process.Id);

                    entries.Add(new ProcessMonitorEntry
                    {
                        Name = process.ProcessName,
                        ProcessId = process.Id,
                        Priority = process.PriorityClass.ToString(),
                        CpuPercent = cpuPercent,
                        RamMb = process.WorkingSet64 / 1024 / 1024,
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "Prozess {Pid} konnte nicht gelesen werden", process.Id);
                }
            }
        }

        PruneStaleSamples(activeIds);
        var filtered = _appSettingsService.Current.ProcessMonitorCursorFilterOnly
            ? entries.Where(IsCursorRelevant).ToList()
            : entries;

        return filtered
            .OrderByDescending(entry => entry.CpuPercent)
            .ThenByDescending(entry => entry.RamMb)
            .ToList();
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

            var cpuPercent = cpuDeltaMs / (elapsedMs * Environment.ProcessorCount) * 100;
            return Math.Round(Math.Clamp(cpuPercent, 0, 100), 1);
        }
    }

    private static bool IsCursorRelevant(ProcessMonitorEntry entry)
    {
        var name = entry.Name;
        return name.Contains("cursor", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("node", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("eslint", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("tsc", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("git", StringComparison.OrdinalIgnoreCase);
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
