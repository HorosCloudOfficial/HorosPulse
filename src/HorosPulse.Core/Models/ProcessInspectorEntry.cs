namespace HorosPulse.Core.Models;

public sealed class ProcessInspectorEntry
{
    public string Name { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public string Priority { get; init; } = string.Empty;
    public int HandleCount { get; init; }
    public int ThreadCount { get; init; }
    public long IoReadBytes { get; init; }
    public long IoWriteBytes { get; init; }
    public long WorkingSetMb { get; init; }
    public double CpuPercent { get; init; }
}
