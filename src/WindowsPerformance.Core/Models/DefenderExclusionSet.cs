namespace WindowsPerformance.Core.Models;

public sealed class DefenderExclusionSet
{
    public IReadOnlyList<string> CurrentExclusions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AddedByApp { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> DefaultPaths { get; init; } = Array.Empty<string>();
    public IReadOnlyList<PathValidationResult> PathValidations { get; init; } = Array.Empty<PathValidationResult>();
}

public sealed class PathValidationResult
{
    public string Path { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool DriveValid { get; init; }
    public string Message { get; init; } = string.Empty;
}
