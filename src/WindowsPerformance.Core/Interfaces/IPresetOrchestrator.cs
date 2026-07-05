namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IPresetOrchestrator
{
    Task<IReadOnlyList<ProfileDefinition>> GetPresetsAsync(CancellationToken cancellationToken = default);
    Task<PresetApplyResult> ApplyPresetAsync(string presetId, CancellationToken cancellationToken = default);
}
