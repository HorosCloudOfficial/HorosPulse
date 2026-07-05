namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IMetricsCollector : IDisposable
{
    Task<double> GetCpuUsagePercentAsync(CancellationToken cancellationToken = default);
    Task<long> GetMemoryUsedMbAsync(CancellationToken cancellationToken = default);
    Task<long> GetMemoryTotalMbAsync(CancellationToken cancellationToken = default);
    Task<double> GetDiskActivePercentAsync(CancellationToken cancellationToken = default);
    Task<PerformanceMetric> GetCurrentMetricAsync(CancellationToken cancellationToken = default);
    event EventHandler<PerformanceMetric>? MetricUpdated;
    void StartPolling();
    void StopPolling();
}
