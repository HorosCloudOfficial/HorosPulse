namespace HorosPulse.Core.Models;

public sealed class NetworkSettingsState
{
    public int? TcpAckFrequency { get; init; }
    public int? TcpNoDelay { get; init; }
    public int? TcpDelAckTicks { get; init; }
    public int? DnsMaxCacheTtl { get; init; }
}
