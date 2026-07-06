namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Verwaltet Windows Search Indexer-Ausschlüsse für Dev-Ordner.
/// </summary>
public interface IIndexerExclusionService
{
    /// <summary>Verfügbare Ausschluss-Einträge auflisten.</summary>
    Task<IReadOnlyList<IndexerExcludeEntry>> GetAvailableEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Ausschlüsse für angegebene Pfade anwenden.</summary>
    Task<OptimizationResult> ApplyExclusionsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default);

    /// <summary>Indexer-Ausschlüsse zurücksetzen.</summary>
    Task<OptimizationResult> RollbackExclusionsAsync(CancellationToken cancellationToken = default);
}
