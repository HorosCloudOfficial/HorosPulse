namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IStartupManagerService
{
    Task<IReadOnlyList<StartupEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);

    Task<OptimizationResult> SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken cancellationToken = default);

    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
