namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Models;

public interface IVisualEffectsService
{
    Task<VisualEffectsState> GetCurrentStateAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> ApplyPresetAsync(VisualEffectsPreset preset, CancellationToken cancellationToken = default);

    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
