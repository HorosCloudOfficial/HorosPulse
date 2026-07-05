namespace WindowsPerformance.Core.Models;

public sealed class ProfileDefinition
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsBuiltIn { get; init; }
    public IReadOnlyList<string> Steps { get; init; } = Array.Empty<string>();
}
