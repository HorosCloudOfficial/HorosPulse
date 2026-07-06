namespace HorosPulse.Core.Interfaces;

/// <summary>
/// Einzelnes Optimierungsmodul mit Apply- und Rollback-Operationen.
/// </summary>
public interface IOptimizationModule
{
    /// <summary>Eindeutiger Modulname (z. B. PowerPlan, Cursor).</summary>
    string ModuleName { get; }

    /// <summary>Ob das Modul aktuell angewendet werden kann.</summary>
    bool CanApply { get; }

    /// <summary>Optimierung anwenden.</summary>
    Task ApplyAsync(CancellationToken cancellationToken = default);

    /// <summary>Letzte Änderung des Moduls rückgängig machen.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
