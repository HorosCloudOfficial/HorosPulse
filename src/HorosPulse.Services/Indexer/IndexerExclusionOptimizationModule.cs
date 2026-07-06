namespace HorosPulse.Services.Indexer;

using HorosPulse.Core.Interfaces;

public sealed class IndexerExclusionOptimizationModule : IOptimizationModule
{
    private readonly IIndexerExclusionService _indexerExclusionService;
    private IReadOnlyList<string> _selectedPaths = Array.Empty<string>();

    public IndexerExclusionOptimizationModule(IIndexerExclusionService indexerExclusionService) =>
        _indexerExclusionService = indexerExclusionService;

    public string ModuleName => "SearchIndexer";

    public bool CanApply => _selectedPaths.Count > 0;

    public void SetSelectedPaths(IReadOnlyList<string> paths) => _selectedPaths = paths;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _indexerExclusionService.ApplyExclusionsAsync(_selectedPaths, cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _indexerExclusionService.RollbackExclusionsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
