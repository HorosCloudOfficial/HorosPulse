namespace HorosPulse.Services.Monitoring;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class PerformanceCounterService : IMetricsCollector
{
    private readonly IPerformanceMetricRepository _metricRepository;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger<PerformanceCounterService> _logger;

    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _diskCounter;
    private Timer? _pollingTimer;
    private bool _disposed;
    private PerformanceMetric? _latestMetric;

    public PerformanceCounterService(
        IPerformanceMetricRepository metricRepository,
        IAppSettingsService appSettingsService,
        ILogger<PerformanceCounterService> logger)
    {
        _metricRepository = metricRepository;
        _appSettingsService = appSettingsService;
        _logger = logger;
        InitializeCounters();
    }

    public event EventHandler<PerformanceMetric>? MetricUpdated;

    public async Task<double> GetCpuUsagePercentAsync(CancellationToken cancellationToken = default)
    {
        var metric = await GetCurrentMetricAsync(cancellationToken);
        return metric.CpuPercent;
    }

    public async Task<long> GetMemoryUsedMbAsync(CancellationToken cancellationToken = default)
    {
        var metric = await GetCurrentMetricAsync(cancellationToken);
        return metric.RamUsedMb;
    }

    public async Task<long> GetMemoryTotalMbAsync(CancellationToken cancellationToken = default)
    {
        var metric = await GetCurrentMetricAsync(cancellationToken);
        return metric.RamTotalMb;
    }

    public async Task<double> GetDiskActivePercentAsync(CancellationToken cancellationToken = default)
    {
        var metric = await GetCurrentMetricAsync(cancellationToken);
        return metric.DiskActivePercent;
    }

    public Task<PerformanceMetric> GetCurrentMetricAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (totalMb, usedMb) = GetMemoryStatus();
        var cpu = ReadCpuPercent();
        var disk = ReadDiskPercent();

        var metric = new PerformanceMetric(
            DateTimeOffset.UtcNow,
            Math.Round(JsonDefaults.SanitizeMetric(cpu), 1),
            usedMb,
            totalMb,
            Math.Round(JsonDefaults.SanitizeMetric(disk), 1));

        _latestMetric = metric;
        return Task.FromResult(metric);
    }

    public void StartPolling()
    {
        if (_pollingTimer is not null)
            return;

        var interval = Math.Max(1000, _appSettingsService.Current.MetricsPollingIntervalMs);
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

    public IObservable<PerformanceMetric> ObserveMetrics() => new MetricsObservableAdapter(this);

    public void Dispose()
    {
        if (_disposed)
            return;

        StopPolling();
        _cpuCounter?.Dispose();
        _diskCounter?.Dispose();
        _disposed = true;
    }

    private void InitializeCounters()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CPU PerformanceCounter nicht verfügbar");
            _cpuCounter = null;
        }

        try
        {
            _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            _diskCounter.NextValue();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Disk PerformanceCounter nicht verfügbar");
            _diskCounter = null;
        }
    }

    private async Task PollAsync()
    {
        try
        {
            var metric = await GetCurrentMetricAsync();
            MetricUpdated?.Invoke(this, metric);

            await _metricRepository.InitializeAsync();
            await _metricRepository.InsertAsync(metric);
            await _metricRepository.PurgeOlderThanAsync(TimeSpan.FromDays(7));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Metrik-Polling fehlgeschlagen");
        }
    }

    private double ReadCpuPercent()
    {
        if (_cpuCounter is null)
            return 0;

        try
        {
            return Math.Clamp(_cpuCounter.NextValue(), 0, 100);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "CPU-Auslesen fehlgeschlagen");
            return _latestMetric?.CpuPercent ?? 0;
        }
    }

    private double ReadDiskPercent()
    {
        if (_diskCounter is null)
            return 0;

        try
        {
            return Math.Clamp(_diskCounter.NextValue(), 0, 100);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Disk-Auslesen fehlgeschlagen");
            return _latestMetric?.DiskActivePercent ?? 0;
        }
    }

    private static (long TotalMb, long UsedMb) GetMemoryStatus()
    {
        var status = new MemoryStatusEx { DwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status))
            return (0, 0);

        var totalMb = (long)(status.UllTotalPhys / 1024 / 1024);
        var usedMb = (long)((status.UllTotalPhys - status.UllAvailPhys) / 1024 / 1024);
        return (totalMb, usedMb);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint DwLength;
        public uint DwMemoryLoad;
        public ulong UllTotalPhys;
        public ulong UllAvailPhys;
        public ulong UllTotalPageFile;
        public ulong UllAvailPageFile;
        public ulong UllTotalVirtual;
        public ulong UllAvailVirtual;
        public ulong UllAvailExtendedVirtual;
    }
}
