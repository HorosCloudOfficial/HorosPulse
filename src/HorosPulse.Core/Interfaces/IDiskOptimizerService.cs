namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Verwaltet Festplatten-Optimierungseinstellungen (Prefetch, TRIM, Write-Caching).
/// </summary>
public interface IDiskOptimizerService
{
    /// <summary>Aktuellen Festplatten-Status abrufen.</summary>
    Task<DiskOptimizerState> GetCurrentStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Dev-optimierte Festplatten-Einstellungen anwenden.</summary>
    Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Einstellungen auf Snapshot-Stand zurücksetzen.</summary>
    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
