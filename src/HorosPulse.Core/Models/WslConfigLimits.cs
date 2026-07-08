namespace HorosPulse.Core.Models;

/// <summary>
/// Effektive Ressourcen-Limits für die WSL-2-VM (explizit oder Microsoft-Standard).
/// </summary>
public sealed class WslConfigLimits
{
    public long? MemoryMb { get; init; }

    public bool MemoryUsesDefault { get; init; }

    public int? Processors { get; init; }

    public bool ProcessorsUsesDefault { get; init; }

    public long? SwapMb { get; init; }

    public bool SwapUsesDefault { get; init; }

    public bool? LocalhostForwarding { get; init; }

    public bool? NestedVirtualization { get; init; }

    public bool? PageReporting { get; init; }

    public string FormatMemoryMb() => FormatSizeMb(MemoryMb, MemoryUsesDefault);

    public string FormatSwapMb() => FormatSizeMb(SwapMb, SwapUsesDefault);

    public string FormatProcessors() =>
        ProcessorsUsesDefault || Processors is null
            ? "Standard (alle Kerne)"
            : Processors.Value.ToString();

    private static string FormatSizeMb(long? mb, bool usesDefault)
    {
        if (usesDefault || mb is null)
            return "Standard";

        if (mb >= 1024 && mb % 1024 == 0)
            return $"{mb / 1024} GB";

        return $"{mb} MB";
    }
}

/// <summary>
/// Von HorosPulse empfohlene .wslconfig-Werte für Dev-/Docker-Setups.
/// </summary>
public sealed class WslConfigRecommendedLimits
{
    public required long MemoryMb { get; init; }

    public required int Processors { get; init; }

    public required long SwapMb { get; init; }

    public bool LocalhostForwarding { get; init; } = true;

    public bool NestedVirtualization { get; init; } = true;

    public bool PageReporting { get; init; } = true;
}
