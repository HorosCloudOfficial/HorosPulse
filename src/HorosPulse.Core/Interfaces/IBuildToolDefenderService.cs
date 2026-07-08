namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Defender-Prozess-Ausschlüsse für Build- und Dev-Toolchain (Opt-in).
/// </summary>
public interface IBuildToolDefenderService
{
    /// <summary>Empfohlene Prozessnamen für Compiler, IDEs und Container-Tools.</summary>
    IReadOnlyList<BuildToolProcessEntry> GetDefaultProcesses();

    /// <summary>Aktuellen Ausschluss-Status abrufen.</summary>
    Task<BuildToolDefenderState> GetStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Prozess-Ausschlüsse anwenden (nur nach User-Bestätigung).</summary>
    Task<OptimizationResult> ApplyExclusionsAsync(bool userConfirmed, CancellationToken cancellationToken = default);

    /// <summary>Vom Tool hinzugefügte Prozess-Ausschlüsse entfernen.</summary>
    Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default);
}
