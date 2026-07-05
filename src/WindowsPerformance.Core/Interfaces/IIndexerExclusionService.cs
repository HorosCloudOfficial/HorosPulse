namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IIndexerExclusionService
{
    Task<IReadOnlyList<IndexerExcludeEntry>> GetAvailableEntriesAsync(CancellationToken cancellationToken = default);
    Task<OptimizationResult> ApplyExclusionsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default);
    Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default);
}
