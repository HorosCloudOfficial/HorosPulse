namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Opt-in Registry-Tweaks mit dokumentierten, bewährten Einstellungen.
/// </summary>
public interface IRegistryTunerService
{
    /// <summary>Verfügbare dokumentierte Tweaks auflisten.</summary>
    IReadOnlyList<RegistryTweakDefinition> GetAvailableTweaks();

    /// <summary>Einzelnen Tweak anwenden (nach User-Zustimmung).</summary>
    Task<OptimizationResult> ApplyTweakAsync(string tweakId, bool userConfirmed, CancellationToken cancellationToken = default);

    /// <summary>Alle angewendeten Tweaks zurücksetzen.</summary>
    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
