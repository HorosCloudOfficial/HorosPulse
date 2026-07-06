namespace HorosPulse.Core.Models;

using HorosPulse.Core.Enums;

public sealed class ProcessPriorityRule
{
    public required string ProcessName { get; init; }
    public ProcessPriorityLevel Priority { get; init; }
    public bool ApplyOnStartup { get; init; }
}
