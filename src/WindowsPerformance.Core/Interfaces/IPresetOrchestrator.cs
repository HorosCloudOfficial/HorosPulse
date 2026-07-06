namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// Orchestriert Preset-Anwendung über mehrere Optimierungsmodule.
/// </summary>
public interface IPresetOrchestrator
{
    /// <summary>Alle verfügbaren Presets (Built-in + User) laden.</summary>
    Task<IReadOnlyList<ProfileDefinition>> GetPresetsAsync(CancellationToken cancellationToken = default);

    /// <summary>Preset anwenden: Snapshot, Module in Reihenfolge, Audit.</summary>
    Task<PresetApplyResult> ApplyPresetAsync(string presetId, CancellationToken cancellationToken = default);
}
