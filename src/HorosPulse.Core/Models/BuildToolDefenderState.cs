namespace HorosPulse.Core.Models;

/// <summary>
/// Status der Defender-Prozess-Ausschlüsse für Build-Tools.
/// </summary>
public sealed class BuildToolDefenderState
{
    public required IReadOnlyList<BuildToolProcessEntry> Entries { get; init; }

    public required IReadOnlyList<string> AddedByApp { get; init; }

    public int AppliedCount { get; init; }

    public int RecommendedCount { get; init; }
}
