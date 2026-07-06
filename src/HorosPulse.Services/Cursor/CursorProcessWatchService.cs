namespace HorosPulse.Services.Cursor;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;

public sealed class CursorProcessWatchService : ICursorProcessWatchService
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IProcessPriorityService _processPriorityService;
    private readonly ILogger<CursorProcessWatchService> _logger;
    private readonly HashSet<int> _knownCursorPids = new();
    private Timer? _timer;
    private bool _disposed;

    public CursorProcessWatchService(
        IAppSettingsService appSettingsService,
        IProcessPriorityService processPriorityService,
        ILogger<CursorProcessWatchService> logger)
    {
        _appSettingsService = appSettingsService;
        _processPriorityService = processPriorityService;
        _logger = logger;
    }

    public void Start()
    {
        if (_timer is not null)
            return;

        SeedKnownProcesses();
        _timer = new Timer(
            async _ => await CheckForNewCursorProcessesAsync(),
            null,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(3));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
    }

    private void SeedKnownProcesses()
    {
        foreach (var process in Process.GetProcessesByName("Cursor"))
        {
            using (process)
                _knownCursorPids.Add(process.Id);
        }
    }

    private async Task CheckForNewCursorProcessesAsync()
    {
        if (!_appSettingsService.Current.ReapplyCursorPrioritiesOnRestart)
            return;

        var newProcesses = false;
        foreach (var process in Process.GetProcessesByName("Cursor"))
        {
            using (process)
            {
                if (_knownCursorPids.Add(process.Id))
                    newProcesses = true;
            }
        }

        PruneStalePids();

        if (!newProcesses)
            return;

        try
        {
            var result = await _processPriorityService.ApplyCursorPrioritiesAsync();
            if (result.Success)
                _logger.LogInformation("Cursor-Prioritäten nach Neustart erneut angewendet.");
            else
                _logger.LogDebug("Cursor-Prioritäten Re-Apply: {Message}", result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Cursor-Prioritäten Re-Apply fehlgeschlagen");
        }
    }

    private void PruneStalePids()
    {
        foreach (var pid in _knownCursorPids.ToList())
        {
            try
            {
                using var process = Process.GetProcessById(pid);
            }
            catch
            {
                _knownCursorPids.Remove(pid);
            }
        }
    }
}
