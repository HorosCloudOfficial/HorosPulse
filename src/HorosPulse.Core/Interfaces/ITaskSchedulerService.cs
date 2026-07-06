namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Verwaltet geplante Windows-Tasks (lesen, deaktivieren, Dev-Preset).
/// </summary>
public interface ITaskSchedulerService
{
    /// <summary>Alle relevanten geplanten Tasks auflisten.</summary>
    Task<IReadOnlyList<ScheduledTaskInfo>> GetTasksAsync(CancellationToken cancellationToken = default);

    /// <summary>Dev-Zeit-Schutz: störende Tasks temporär deaktivieren.</summary>
    Task<OptimizationResult> ApplyDevProtectionPresetAsync(CancellationToken cancellationToken = default);

    /// <summary>Deaktivierte Tasks wieder aktivieren.</summary>
    Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
