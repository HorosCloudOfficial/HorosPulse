namespace WindowsPerformance.Services.Trends;

using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class TrendAnalysisService : ITrendAnalysisService
{
    private readonly IPerformanceMetricRepository _metricRepository;
    private readonly IAuditRepository _auditRepository;

    public TrendAnalysisService(
        IPerformanceMetricRepository metricRepository,
        IAuditRepository auditRepository)
    {
        _metricRepository = metricRepository;
        _auditRepository = auditRepository;
    }

    public async Task<IReadOnlyList<TrendBucket>> GetTrendBucketsAsync(
        TrendTimeRange range,
        CancellationToken cancellationToken = default)
    {
        await _metricRepository.InitializeAsync(cancellationToken);
        var (since, bucketSize) = GetRangeSettings(range);
        var metrics = await _metricRepository.GetSinceAsync(since, cancellationToken);

        if (metrics.Count == 0)
            return Array.Empty<TrendBucket>();

        return AggregateBuckets(metrics, bucketSize);
    }

    public async Task<IReadOnlyList<TrendAnnotation>> GetAnnotationsAsync(
        TrendTimeRange range,
        CancellationToken cancellationToken = default)
    {
        await _auditRepository.InitializeAsync(cancellationToken);
        var (since, _) = GetRangeSettings(range);
        var entries = await _auditRepository.GetSinceAsync(since, cancellationToken);

        return entries
            .Where(e => e.Operation is "Apply" or "PresetApply")
            .Select(e => new TrendAnnotation
            {
                Timestamp = e.Timestamp,
                Label = e.Details ?? e.Module,
                Module = e.Module,
            })
            .ToList();
    }

    private static (DateTimeOffset Since, TimeSpan BucketSize) GetRangeSettings(TrendTimeRange range) =>
        range switch
        {
            TrendTimeRange.FiveMinutes => (DateTimeOffset.UtcNow.AddMinutes(-5), TimeSpan.FromSeconds(30)),
            TrendTimeRange.OneHour => (DateTimeOffset.UtcNow.AddHours(-1), TimeSpan.FromMinutes(5)),
            TrendTimeRange.TwentyFourHours => (DateTimeOffset.UtcNow.AddHours(-24), TimeSpan.FromHours(1)),
            _ => (DateTimeOffset.UtcNow.AddHours(-1), TimeSpan.FromMinutes(5)),
        };

    internal static IReadOnlyList<TrendBucket> AggregateBuckets(
        IReadOnlyList<PerformanceMetric> metrics,
        TimeSpan bucketSize)
    {
        if (metrics.Count == 0)
            return Array.Empty<TrendBucket>();

        var buckets = new List<TrendBucket>();
        var bucketStart = metrics[0].Timestamp;
        var bucketMetrics = new List<PerformanceMetric>();

        foreach (var metric in metrics)
        {
            if (metric.Timestamp - bucketStart >= bucketSize && bucketMetrics.Count > 0)
            {
                buckets.Add(CreateBucket(bucketStart, bucketMetrics));
                bucketMetrics.Clear();
                bucketStart = metric.Timestamp;
            }

            bucketMetrics.Add(metric);
        }

        if (bucketMetrics.Count > 0)
            buckets.Add(CreateBucket(bucketStart, bucketMetrics));

        return buckets;
    }

    private static TrendBucket CreateBucket(DateTimeOffset timestamp, List<PerformanceMetric> metrics)
    {
        var cpu = metrics.Average(m => m.CpuPercent);
        var ram = metrics
            .Where(m => m.RamTotalMb > 0)
            .Select(m => m.RamUsedMb * 100.0 / m.RamTotalMb)
            .DefaultIfEmpty(0)
            .Average();
        var disk = metrics.Average(m => m.DiskActivePercent);

        return new TrendBucket
        {
            Timestamp = timestamp,
            CpuPercent = Math.Round(cpu, 1),
            RamPercent = Math.Round(ram, 1),
            DiskPercent = Math.Round(disk, 1),
        };
    }
}
