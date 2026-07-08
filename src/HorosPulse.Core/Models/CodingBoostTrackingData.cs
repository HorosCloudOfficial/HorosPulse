namespace HorosPulse.Core.Models;

/// <summary>
/// Von HorosPulse gesetzte Coding-Boost-Werte für gezieltes Rollback.
/// </summary>
public sealed class CodingBoostTrackingData
{
    public CodingBoostGameModeTracking? GameMode { get; set; }

    public CodingBoostHagsTracking? Hags { get; set; }

    public CodingBoostWindowedOptimizationTracking? WindowedOptimization { get; set; }
}

public sealed class CodingBoostGameModeTracking
{
    public int? AutoGameModeEnabled { get; init; }

    public int? AllowAutoGameMode { get; init; }
}

public sealed class CodingBoostHagsTracking
{
    public int? HwSchMode { get; init; }

    public bool ValueExisted { get; init; }
}

public sealed class CodingBoostWindowedOptimizationTracking
{
    public string? DirectXUserGlobalSettings { get; init; }

    public bool DirectXSettingsExisted { get; init; }

    public int? SwapEffectUpgradeCache { get; init; }

    public bool SwapEffectCacheExisted { get; init; }
}
