namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Models;

public interface IProcessPriorityService
{
    Task<IReadOnlyList<ProcessPriorityRule>> GetDefaultRulesAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> ApplyCursorPrioritiesAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> RollbackCursorPrioritiesAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> SetPriorityAsync(int processId, ProcessPriorityLevel priority, CancellationToken cancellationToken = default);
    string? GetCursorProcessStatus();
}
