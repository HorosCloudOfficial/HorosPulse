namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Enums;
using HorosPulse.Core.Models;

public interface IVisualEffectsService
{
    Task<VisualEffectsState> GetCurrentStateAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> ApplyPresetAsync(VisualEffectsPreset preset, CancellationToken cancellationToken = default);

    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
