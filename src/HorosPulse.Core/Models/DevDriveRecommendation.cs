namespace HorosPulse.Core.Models;

/// <summary>Konkrete Empfehlung zur Dev-Drive-Nutzung (nur Hinweise, keine Auto-Migration).</summary>
public sealed class DevDriveRecommendation
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public DevDriveRecommendationPriority Priority { get; init; }

    public string? RelatedPath { get; init; }

    public string PriorityLabel => Priority switch
    {
        DevDriveRecommendationPriority.High => "Hoch",
        DevDriveRecommendationPriority.Medium => "Mittel",
        DevDriveRecommendationPriority.Low => "Niedrig",
        _ => "Info",
    };
}

public enum DevDriveRecommendationPriority
{
    High,
    Medium,
    Low,
    Info,
}
