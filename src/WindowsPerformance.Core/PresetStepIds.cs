namespace WindowsPerformance.Core;

public static class PresetStepIds
{
    public const string Snapshot = "snapshot";
    public const string PowerPlanHighPerformance = "power-plan:high-performance";
    public const string PowerPlanBalanced = "power-plan:balanced";
    public const string ProcessPriorityCursor = "process-priority:cursor";
    public const string ProcessPriorityNodeNormal = "process-priority:node-normal";
    public const string IndexerExclusions = "indexer-exclusions";
    public const string DefenderExclusions = "defender-exclusions";
    public const string CursorOptimize = "cursor-optimize";
    public const string CursorRevert = "cursor-revert";

    public static IReadOnlyList<(string Id, string Label)> AllSelectable { get; } =
    [
        (Snapshot, "Snapshot erstellen"),
        (PowerPlanHighPerformance, "Energieplan: Höchstleistung"),
        (PowerPlanBalanced, "Energieplan: Ausgewogen"),
        (ProcessPriorityCursor, "Prozessprioritäten (Cursor)"),
        (ProcessPriorityNodeNormal, "node.exe Priorität: Normal"),
        (IndexerExclusions, "Suchindexer-Ausschlüsse"),
        (DefenderExclusions, "Windows Defender Ausnahmen (opt-in)"),
        (CursorOptimize, "Cursor-Einstellungen optimieren"),
        (CursorRevert, "Cursor zurücksetzen"),
    ];
}
