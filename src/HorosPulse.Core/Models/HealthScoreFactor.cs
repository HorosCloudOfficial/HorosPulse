namespace HorosPulse.Core.Models;

public sealed class HealthScoreFactor
{
    public required string Name { get; init; }
    public int MaxPoints { get; init; }
    public int EarnedPoints { get; init; }
    public string Detail { get; init; } = string.Empty;

    public string PointsDisplay => $"{EarnedPoints}/{MaxPoints}";
}
