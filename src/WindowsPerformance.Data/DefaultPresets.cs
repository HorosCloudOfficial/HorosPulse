namespace WindowsPerformance.Data;

using WindowsPerformance.Core;
using WindowsPerformance.Core.Models;

public static class DefaultPresets
{
    public static IReadOnlyList<ProfileDefinition> All { get; } =
    [
        new ProfileDefinition
        {
            Id = PresetIds.CursorDevMode,
            Name = "Cursor Dev Mode",
            Description = "Optimiert Windows und Cursor für maximale Entwicklungsleistung. Erstellt automatisch einen Snapshot vor der Anwendung.",
            IsBuiltIn = true,
            Steps =
            [
                "Snapshot erstellen",
                "Energieplan: Höchstleistung",
                "Visuelle Effekte minimieren",
                "Windows Defender Ausnahmen (opt-in)",
                "Suchindexer-Ausschlüsse",
                "Prozessprioritäten",
                "Cursor-Einstellungen optimieren",
                "Audit-Protokoll",
            ],
        },
        new ProfileDefinition
        {
            Id = PresetIds.Balanced,
            Name = "Balanced",
            Description = "Setzt alle Optimierungen zurück und stellt den ausgewogenen Energieplan wieder her.",
            IsBuiltIn = true,
            Steps =
            [
                "Snapshot erstellen",
                "Alle Module zurücksetzen",
                "Energieplan: Ausgewogen",
            ],
        },
        new ProfileDefinition
        {
            Id = PresetIds.Gaming,
            Name = "Gaming",
            Description = "Höchstleistungs-Energieplan ohne Cursor-Anpassungen.",
            IsBuiltIn = true,
            Steps =
            [
                "Snapshot erstellen",
                "Energieplan: Höchstleistung",
            ],
        },
    ];
}
