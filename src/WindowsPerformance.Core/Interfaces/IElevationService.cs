namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// Führt PowerShell-Skripte mit erhöhten Rechten über WindowsPerformance.Elevation.exe aus.
/// </summary>
public interface IElevationService
{
    /// <summary>Führt ein Skript elevated aus (Named-Pipe an ElevationHelper).</summary>
    Task<PowerShellResult> RunElevatedScriptAsync(
        string script,
        string scriptHash,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>Ob WindowsPerformance.Elevation.exe am erwarteten Pfad verfügbar ist.</summary>
    bool IsHelperAvailable { get; }

    /// <summary>Standby-Speicherliste leeren (erfordert Elevation).</summary>
    Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default);
}
