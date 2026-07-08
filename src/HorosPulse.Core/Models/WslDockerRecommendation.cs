namespace HorosPulse.Core.Models;

/// <summary>
/// Einzelne Empfehlung für WSL2/Docker-Tuning.
/// </summary>
public sealed class WslDockerRecommendation
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string CurrentDisplay { get; init; }

    public required string RecommendedDisplay { get; init; }

    public bool IsOptimal { get; init; }

    public WslDockerRecommendationSeverity Severity { get; init; } = WslDockerRecommendationSeverity.Info;
}

public enum WslDockerRecommendationSeverity
{
    Info,
    Suggestion,
    Warning,
}
