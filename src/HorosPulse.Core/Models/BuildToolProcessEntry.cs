namespace HorosPulse.Core.Models;

/// <summary>
/// Ein Build-/Dev-Tool-Prozess für Defender-Prozess-Ausschlüsse.
/// </summary>
public sealed class BuildToolProcessEntry
{
    public required string ProcessName { get; init; }

    public required string DisplayName { get; init; }

    public required string Category { get; init; }

    public bool IsApplied { get; init; }

    public bool IsRecommended { get; init; } = true;
}
