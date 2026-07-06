namespace HorosPulse.Core.Models;

public sealed record PerformanceMetric(
    DateTimeOffset Timestamp,
    double CpuPercent,
    long RamUsedMb,
    long RamTotalMb,
    double DiskActivePercent);
