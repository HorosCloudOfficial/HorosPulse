namespace HorosPulse.Services.BuildToolDefender;

using HorosPulse.Core.Interfaces;

public sealed class BuildToolDefenderOptimizationModule : IOptimizationModule
{
    private readonly IBuildToolDefenderService _buildToolDefenderService;

    public BuildToolDefenderOptimizationModule(IBuildToolDefenderService buildToolDefenderService) =>
        _buildToolDefenderService = buildToolDefenderService;

    public string ModuleName => "BuildToolDefender";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _buildToolDefenderService.ApplyExclusionsAsync(userConfirmed: true, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _buildToolDefenderService.RollbackExclusionsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
