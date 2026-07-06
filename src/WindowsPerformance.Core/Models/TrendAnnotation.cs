namespace WindowsPerformance.Core.Models;

public sealed class TrendAnnotation
{
    public DateTimeOffset Timestamp { get; init; }
    public required string Label { get; init; }
    public required string Module { get; init; }
}
