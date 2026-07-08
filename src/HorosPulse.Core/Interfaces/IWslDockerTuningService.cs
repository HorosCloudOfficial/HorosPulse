namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// WSL2-.wslconfig-Tuning und Docker-Status für Dev-Setups.
/// </summary>
public interface IWslDockerTuningService
{
    /// <summary>Aktuellen Status, Limits und Empfehlungen abrufen.</summary>
    Task<WslDockerTuningState> GetStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Empfohlene .wslconfig-Werte anwenden (nur nach User-Bestätigung).</summary>
    Task<OptimizationResult> ApplyRecommendedConfigAsync(bool userConfirmed, CancellationToken cancellationToken = default);

    /// <summary>Von HorosPulse geänderte .wslconfig wiederherstellen.</summary>
    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>WSL-VM herunterfahren (wsl --shutdown) — erforderlich nach .wslconfig-Änderungen.</summary>
    Task<OptimizationResult> ShutdownWslAsync(CancellationToken cancellationToken = default);
}
