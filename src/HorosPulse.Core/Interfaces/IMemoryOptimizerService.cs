namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

public interface IMemoryOptimizerService
{
    Task<long> GetAvailableMemoryMbAsync(CancellationToken cancellationToken = default);

    Task<MemoryStatusSnapshot> GetMemoryStatusAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> PurgeMemoryAsync(
        MemoryPurgeOptions options,
        CancellationToken cancellationToken = default);

    Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default);
}
