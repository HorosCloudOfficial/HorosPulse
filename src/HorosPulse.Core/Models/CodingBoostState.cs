namespace HorosPulse.Core.Models;

/// <summary>
/// Aktueller Status der Coding-Boost-Einstellungen (Game Mode, HAGS, Fenster-Optimierung).
/// </summary>
public sealed class CodingBoostState
{
    public required CodingBoostSettingStatus GameMode { get; init; }

    public required CodingBoostSettingStatus HardwareAcceleratedGpuScheduling { get; init; }

    public required CodingBoostSettingStatus WindowedGameOptimization { get; init; }

    public bool IsWindows11OrNewer { get; init; }

    public bool HasHorosPulseChanges { get; init; }

    public bool IsDevSetupOptimal { get; init; }

    public string RecommendationSummary { get; init; } = string.Empty;
}

/// <summary>
/// Status einer einzelnen Coding-Boost-Einstellung.
/// </summary>
public sealed class CodingBoostSettingStatus
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public bool IsEnabled { get; init; }

    public bool IsRecommended { get; init; }

    public bool IsSupported { get; init; } = true;

    public bool RequiresReboot { get; init; }

    public string? DetailText { get; init; }
}
