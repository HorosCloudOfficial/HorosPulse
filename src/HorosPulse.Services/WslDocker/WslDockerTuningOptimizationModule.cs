namespace HorosPulse.Services.WslDocker;

using HorosPulse.Core.Interfaces;

public sealed class WslDockerTuningOptimizationModule : IOptimizationModule
{
    private readonly IWslDockerTuningService _wslDockerTuningService;

    public WslDockerTuningOptimizationModule(IWslDockerTuningService wslDockerTuningService) =>
        _wslDockerTuningService = wslDockerTuningService;

    public string ModuleName => "WslDockerTuning";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _wslDockerTuningService.ApplyRecommendedConfigAsync(userConfirmed: true, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _wslDockerTuningService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
