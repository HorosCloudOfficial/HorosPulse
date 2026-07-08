namespace HorosPulse.Core.Models;

/// <summary>Physischer Speicher, Page File und Systemreserviert (Mem-Reduct-Parität).</summary>
public sealed record MemoryStatusSnapshot(
    long PhysicalTotalMb,
    long PhysicalAvailableMb,
    long PageFileTotalMb,
    long PageFileAvailableMb,
    long SystemReservedTotalMb,
    long SystemReservedAvailableMb)
{
    public double PhysicalUsedPercent => PhysicalTotalMb > 0
        ? (PhysicalTotalMb - PhysicalAvailableMb) * 100.0 / PhysicalTotalMb
        : 0;

    public double PageFileUsedPercent => PageFileTotalMb > 0
        ? (PageFileTotalMb - PageFileAvailableMb) * 100.0 / PageFileTotalMb
        : 0;

    public double SystemReservedUsedPercent => SystemReservedTotalMb > 0
        ? (SystemReservedTotalMb - SystemReservedAvailableMb) * 100.0 / SystemReservedTotalMb
        : 0;

    public static MemoryStatusSnapshot Empty { get; } = new(0, 0, 0, 0, 0, 0);
}
