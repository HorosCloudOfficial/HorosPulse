namespace WindowsPerformance.Core.Models;

public sealed class DefenderExclusionSet
{
    public IReadOnlyList<string> CurrentExclusions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AddedByApp { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> DefaultPaths { get; init; } = Array.Empty<string>();
}
