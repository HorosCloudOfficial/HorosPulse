namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Game Mode, HAGS und Fenster-Optimierung für Dev-/GPU-lastige Workflows.
/// </summary>
public interface ICodingBoostService
{
    /// <summary>Aktuellen Status und Empfehlungen abrufen.</summary>
    Task<CodingBoostState> GetStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Empfohlenes Dev-Setup anwenden (nur nach User-Bestätigung).</summary>
    Task<OptimizationResult> ApplyDevSetupAsync(bool userConfirmed, CancellationToken cancellationToken = default);

    /// <summary>Nur von HorosPulse gesetzte Werte zurücksetzen.</summary>
    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
