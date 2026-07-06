namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Models;

public interface ITrendAnalysisService
{
    Task<IReadOnlyList<TrendBucket>> GetTrendBucketsAsync(
        TrendTimeRange range,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrendAnnotation>> GetAnnotationsAsync(
        TrendTimeRange range,
        CancellationToken cancellationToken = default);
}
