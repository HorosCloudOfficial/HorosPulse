namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Führt PowerShell-Skripte mit erhöhten Rechten über HorosPulse.Elevation.exe aus.
/// </summary>
public interface IElevationService
{
    /// <summary>Führt ein Skript elevated aus (Named-Pipe an ElevationHelper).</summary>
    Task<PowerShellResult> RunElevatedScriptAsync(
        string script,
        string scriptHash,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>Ob HorosPulse.Elevation.exe am erwarteten Pfad verfügbar ist.</summary>
    bool IsHelperAvailable { get; }

    /// <summary>Standby-Speicherliste leeren (erfordert Elevation).</summary>
    Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default);

    /// <summary>Mem-Reduct-kompatible Speicherbereiche leeren (erfordert Elevation).</summary>
    Task<OptimizationResult> PurgeMemoryAsync(
        MemoryPurgeOptions options,
        CancellationToken cancellationToken = default);
}
