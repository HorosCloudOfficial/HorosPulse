namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Models;

/// <summary>
/// Setzt und stellt Prozessprioritäten für Cursor und Node zurück.
/// </summary>
public interface IProcessPriorityService
{
    /// <summary>Standard-Prioritätsregeln für Cursor/Node.</summary>
    Task<IReadOnlyList<ProcessPriorityRule>> GetDefaultRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>Cursor-Prozesse auf High Priority setzen.</summary>
    Task<OptimizationResult> ApplyCursorPrioritiesAsync(CancellationToken cancellationToken = default);

    /// <summary>Node-Prozesse auf Normal Priority setzen.</summary>
    Task<OptimizationResult> EnsureNodeNormalPriorityAsync(CancellationToken cancellationToken = default);

    /// <summary>Cursor-Prioritäten auf Standard zurücksetzen.</summary>
    Task<OptimizationResult> RollbackCursorPrioritiesAsync(CancellationToken cancellationToken = default);

    /// <summary>Priorität eines einzelnen Prozesses setzen.</summary>
    Task<OptimizationResult> SetPriorityAsync(int processId, ProcessPriorityLevel priority, CancellationToken cancellationToken = default);

    /// <summary>Status-Text zum Cursor-Prozess (gefunden/nicht gefunden).</summary>
    string? GetCursorProcessStatus();
}
