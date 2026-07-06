namespace HorosPulse.Core.Models;

public sealed class ProcessMonitorEntry
{
    public string Name { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public string Priority { get; init; } = string.Empty;
    public double CpuPercent { get; init; }
    public long RamMb { get; init; }
}
