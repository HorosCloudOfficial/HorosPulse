namespace HorosPulse.Core.Models;

public sealed class SnapshotEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Label { get; init; } = string.Empty;
    public string Module { get; init; } = "FullSystem";
    public string StateJson { get; init; } = string.Empty;
    public string Checksum { get; init; } = string.Empty;
    public bool IsValid { get; init; } = true;
    public bool CanRollback { get; init; } = true;
}
