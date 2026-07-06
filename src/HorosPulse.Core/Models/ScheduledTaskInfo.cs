namespace HorosPulse.Core.Models;

public sealed class ScheduledTaskInfo
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string? Description { get; init; }
}
