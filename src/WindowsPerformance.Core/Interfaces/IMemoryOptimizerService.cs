namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IMemoryOptimizerService
{
    Task<long> GetAvailableMemoryMbAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default);
}
