namespace WindowsPerformance.Core.Models;

public sealed class PerformanceRecommendation
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "Info";
    public DateTimeOffset? RelatedTimestamp { get; init; }
}
