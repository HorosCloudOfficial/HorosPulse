namespace HorosPulse.Core.Models;

public sealed class SystemSnapshot
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Label { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyDictionary<string, string> ModuleStates { get; init; } =
        new Dictionary<string, string>();
}
