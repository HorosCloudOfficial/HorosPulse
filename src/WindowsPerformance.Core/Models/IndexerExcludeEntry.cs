namespace WindowsPerformance.Core.Models;

public sealed class IndexerExcludeEntry
{
    public required string Path { get; init; }
    public bool IsSelected { get; init; }
    public bool IsDefault { get; init; }
    public bool IsApplied { get; init; }
}
