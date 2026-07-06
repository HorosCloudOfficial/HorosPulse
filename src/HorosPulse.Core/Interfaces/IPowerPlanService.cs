namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Verwaltet Windows-Energiepläne via powercfg.
/// </summary>
public interface IPowerPlanService
{
    /// <summary>Alle verfügbaren Energiepläne auflisten.</summary>
    Task<IReadOnlyList<PowerPlanInfo>> GetAvailablePlansAsync(CancellationToken cancellationToken = default);

    /// <summary>Aktiven Energieplan abrufen.</summary>
    Task<PowerPlanInfo?> GetActivePlanAsync(CancellationToken cancellationToken = default);

    /// <summary>Energieplan per GUID aktivieren.</summary>
    Task<OptimizationResult> SetActivePlanAsync(Guid planGuid, CancellationToken cancellationToken = default);

    /// <summary>High-Performance-Plan aktivieren oder duplizieren.</summary>
    Task<OptimizationResult> EnsureHighPerformancePlanAsync(CancellationToken cancellationToken = default);

    /// <summary>Ultimate-Performance-Plan bei Bedarf duplizieren und aktivieren.</summary>
    Task<OptimizationResult> EnsureUltimatePerformancePlanAsync(CancellationToken cancellationToken = default);
}
