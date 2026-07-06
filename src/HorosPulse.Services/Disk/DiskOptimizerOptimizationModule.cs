namespace HorosPulse.Services.Disk;

using HorosPulse.Core.Interfaces;

public sealed class DiskOptimizerOptimizationModule : IOptimizationModule
{
    private readonly IDiskOptimizerService _diskOptimizerService;

    public DiskOptimizerOptimizationModule(IDiskOptimizerService diskOptimizerService) =>
        _diskOptimizerService = diskOptimizerService;

    public string ModuleName => "DiskOptimizer";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _diskOptimizerService.ApplyOptimizationsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _diskOptimizerService.RollbackAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
