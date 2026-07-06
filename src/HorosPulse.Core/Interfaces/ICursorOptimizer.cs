namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Optimiert Cursor-IDE settings.json (Performance-Template).
/// </summary>
public interface ICursorOptimizer
{
    /// <summary>Absoluter Pfad zur Cursor settings.json.</summary>
    string SettingsPath { get; }

    /// <summary>Vorschau der geplanten Änderungen ohne Schreiben.</summary>
    Task<CursorSettingsProfile> PreviewOptimizationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Performance-Optimierungen anwenden (mit Backup).</summary>
    Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default);

    /// <summary>settings.json aus Backup wiederherstellen.</summary>
    Task<OptimizationResult> RevertOptimizationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Ob ein Backup (.HorosPulse.bak) existiert.</summary>
    bool HasBackup { get; }
}
