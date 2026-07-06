namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

public interface IMemoryOptimizerService
{
    Task<long> GetAvailableMemoryMbAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default);
}
