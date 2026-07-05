namespace WindowsPerformance.Services.PowerPlan;

using WindowsPerformance.Core.Interfaces;

public sealed class PowerPlanOptimizationModule : IOptimizationModule
{
    private readonly IPowerPlanService _powerPlanService;

    public PowerPlanOptimizationModule(IPowerPlanService powerPlanService) =>
        _powerPlanService = powerPlanService;

    public string ModuleName => "PowerPlan";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _powerPlanService.EnsureHighPerformancePlanAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_powerPlanService is PowerPlanService concrete)
        {
            var result = await concrete.RollbackAsync(cancellationToken);
            if (!result.Success)
                throw new InvalidOperationException(result.ErrorMessage);
        }
    }
}
