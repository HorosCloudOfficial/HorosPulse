namespace WindowsPerformance.Core.Models;

public sealed class HealthScoreResult
{
    public int Score { get; init; }
    public string ColorHex { get; init; } = "#9ECE6A";
    public IReadOnlyList<HealthScoreFactor> Factors { get; init; } = Array.Empty<HealthScoreFactor>();
}
