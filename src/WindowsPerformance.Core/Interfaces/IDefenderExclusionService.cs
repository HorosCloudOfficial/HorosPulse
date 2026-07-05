namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IDefenderExclusionService
{
    Task<DefenderExclusionSet> GetExclusionSetAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> ApplyExclusionsAsync(bool userConfirmed, CancellationToken cancellationToken = default);
    Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<string> GetDefaultPaths();
}
