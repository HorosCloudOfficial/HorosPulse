namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Verwaltet Windows Defender-Ausnahmen (Opt-in, User-Bestätigung).
/// </summary>
public interface IDefenderExclusionService
{
    /// <summary>Aktuelle Defender-Ausnahmen abrufen.</summary>
    Task<DefenderExclusionSet> GetExclusionSetAsync(CancellationToken cancellationToken = default);

    /// <summary>Ausnahmen anwenden (nur nach User-Bestätigung).</summary>
    Task<OptimizationResult> ApplyExclusionsAsync(bool userConfirmed, CancellationToken cancellationToken = default);

    /// <summary>Defender-Ausnahmen zurücksetzen.</summary>
    Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Standard-Pfade für Dev-Ausnahmen.</summary>
    IReadOnlyList<string> GetDefaultPaths();
}
