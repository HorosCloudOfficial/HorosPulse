namespace HorosPulse.Core.Models;

/// <summary>
/// Gesamtstatus für WSL2/Docker-Tuning (Erkennung, Limits, Empfehlungen).
/// </summary>
public sealed class WslDockerTuningState
{
    public bool IsWslInstalled { get; init; }

    public bool IsWsl2Default { get; init; }

    public string WslStatusSummary { get; init; } = string.Empty;

    public bool IsDockerDesktopInstalled { get; init; }

    public bool IsDockerDesktopRunning { get; init; }

    public bool IsDockerWslBackendLikely { get; init; }

    public string DockerStatusSummary { get; init; } = string.Empty;

    public bool WslConfigExists { get; init; }

    public required string WslConfigPath { get; init; }

    public required WslConfigLimits CurrentLimits { get; init; }

    public required WslConfigLimits DefaultLimits { get; init; }

    public required WslConfigLimits SystemResources { get; init; }

    public required WslConfigRecommendedLimits RecommendedLimits { get; init; }

    public IReadOnlyList<WslDockerRecommendation> Recommendations { get; init; } = [];

    public bool HasHorosPulseChanges { get; init; }

    public bool IsDevSetupOptimal { get; init; }

    public string RecommendationSummary { get; init; } = string.Empty;

    public bool BuildToolDefenderDockerExcluded { get; init; }

    public bool BuildToolDefenderWslExcluded { get; init; }

    public int BuildToolDefenderContainerAppliedCount { get; init; }

    public int BuildToolDefenderContainerRecommendedCount { get; init; }
}
