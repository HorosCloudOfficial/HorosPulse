namespace WindowsPerformance.Core.Models;

/// <summary>
/// Captured system state for baseline snapshots.
/// </summary>
public sealed class BaselineState
{
    public string? PowerPlanGuid { get; init; }
    public string? PowerPlanName { get; init; }
    public string? CursorSettingsJson { get; init; }
    public bool CursorHasBackup { get; init; }
    public DefenderExclusionSet? DefenderState { get; init; }
    public IReadOnlyList<IndexerExcludeEntry>? IndexerState { get; init; }
    public string? ProcessPriorityStateJson { get; init; }
}
