namespace WindowsPerformance.Services.Stubs;

using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class StubPowerPlanService : IPowerPlanService
{
    public Task<IReadOnlyList<PowerPlanInfo>> GetAvailablePlansAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PowerPlanInfo>>(Array.Empty<PowerPlanInfo>());

    public Task<PowerPlanInfo?> GetActivePlanAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<PowerPlanInfo?>(null);

    public Task<OptimizationResult> SetActivePlanAsync(Guid planGuid, CancellationToken cancellationToken = default) =>
        Task.FromResult(OptimizationResult.Ok());

    public Task<OptimizationResult> EnsureHighPerformancePlanAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OptimizationResult.Ok());

    public Task<OptimizationResult> EnsureUltimatePerformancePlanAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OptimizationResult.Ok());
}
