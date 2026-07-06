namespace HorosPulse.Core.Models;

public sealed class AuditEntry
{
    public long Id { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public required string Operation { get; init; }
    public required string Module { get; init; }
    public string Actor { get; init; } = Environment.UserName;
    public string? Details { get; init; }
}
