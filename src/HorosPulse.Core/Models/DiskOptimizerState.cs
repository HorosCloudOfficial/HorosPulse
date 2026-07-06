namespace HorosPulse.Core.Models;

public sealed class DiskOptimizerState
{
    public bool? PrefetchEnabled { get; init; }
    public bool? SuperfetchEnabled { get; init; }
    public bool? TrimEnabled { get; init; }
    public bool? WriteCacheEnabled { get; init; }
    public string? DefragStatus { get; init; }
}
