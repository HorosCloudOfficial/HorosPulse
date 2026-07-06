namespace HorosPulse.Core.Models;

public sealed class MetricsAnomalyResult
{
    public bool HasAnomaly { get; init; }
    public IReadOnlyList<AnomalyPoint> Anomalies { get; init; } = Array.Empty<AnomalyPoint>();
}

public sealed class AnomalyPoint
{
    public DateTimeOffset Timestamp { get; init; }
    public double CpuPercent { get; init; }
    public double ExpectedCpuPercent { get; init; }
}
