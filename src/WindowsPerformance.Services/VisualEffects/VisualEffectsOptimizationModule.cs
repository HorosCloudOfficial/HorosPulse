namespace WindowsPerformance.Services.VisualEffects;

using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Interfaces;

public sealed class VisualEffectsOptimizationModule : IOptimizationModule
{
    private readonly IVisualEffectsService _visualEffectsService;
    private VisualEffectsPreset _preset = VisualEffectsPreset.Performance;

    public VisualEffectsOptimizationModule(IVisualEffectsService visualEffectsService) =>
        _visualEffectsService = visualEffectsService;

    public string ModuleName => "VisualEffects";

    public bool CanApply => true;

    public void SetPreset(VisualEffectsPreset preset) => _preset = preset;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _visualEffectsService.ApplyPresetAsync(_preset, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _visualEffectsService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
