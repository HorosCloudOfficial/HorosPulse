namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IPowerPlanService
{
    Task<IReadOnlyList<PowerPlanInfo>> GetAvailablePlansAsync(CancellationToken cancellationToken = default);
    Task<PowerPlanInfo?> GetActivePlanAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> SetActivePlanAsync(Guid planGuid, CancellationToken cancellationToken = default);
    Task<OptimizationResult> EnsureHighPerformancePlanAsync(CancellationToken cancellationToken = default);
}
