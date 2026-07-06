namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

public interface INetworkOptimizerService
{
    Task<NetworkSettingsState> GetCurrentSettingsAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
