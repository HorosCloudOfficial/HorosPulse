namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IRollbackEngine
{
    Task<OptimizationResult> RollbackSnapshotAsync(SnapshotEntry snapshot, CancellationToken cancellationToken = default);
    Task<OptimizationResult> RollbackModuleAsync(string moduleName, CancellationToken cancellationToken = default);
    Task<OptimizationResult> RollbackLatestAsync(CancellationToken cancellationToken = default);
}
