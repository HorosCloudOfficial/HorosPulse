namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IPerformanceMetricRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(PerformanceMetric metric, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PerformanceMetric>> GetRecentAsync(int limit = 60, CancellationToken cancellationToken = default);
    Task PurgeOlderThanAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}
