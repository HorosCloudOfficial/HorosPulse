namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Stellt Snapshot-basierten und modularen Rollback bereit.
/// </summary>
public interface IRollbackEngine
{
    /// <summary>Alle Module eines Snapshots zurücksetzen.</summary>
    Task<OptimizationResult> RollbackSnapshotAsync(
        SnapshotEntry snapshot,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Einzelnes Modul zurücksetzen.</summary>
    Task<OptimizationResult> RollbackModuleAsync(string moduleName, CancellationToken cancellationToken = default);

    /// <summary>Neuesten Snapshot zurücksetzen.</summary>
    Task<OptimizationResult> RollbackLatestAsync(CancellationToken cancellationToken = default);
}
