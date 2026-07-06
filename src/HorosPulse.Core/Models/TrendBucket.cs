namespace HorosPulse.Core.Models;

public sealed class TrendBucket
{
    public DateTimeOffset Timestamp { get; init; }
    public double CpuPercent { get; init; }
    public double RamPercent { get; init; }
    public double DiskPercent { get; init; }
}
