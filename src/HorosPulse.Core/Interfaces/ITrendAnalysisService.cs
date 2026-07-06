namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Enums;
using HorosPulse.Core.Models;

public interface ITrendAnalysisService
{
    Task<IReadOnlyList<TrendBucket>> GetTrendBucketsAsync(
        TrendTimeRange range,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrendAnnotation>> GetAnnotationsAsync(
        TrendTimeRange range,
        CancellationToken cancellationToken = default);
}
