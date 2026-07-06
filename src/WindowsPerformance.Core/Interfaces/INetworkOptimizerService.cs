namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface INetworkOptimizerService
{
    Task<NetworkSettingsState> GetCurrentSettingsAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
