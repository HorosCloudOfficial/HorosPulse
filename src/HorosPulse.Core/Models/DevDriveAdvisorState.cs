namespace HorosPulse.Core.Models;

/// <summary>Gesamtbewertung des Dev-Drive-Advisors.</summary>
public sealed class DevDriveAdvisorState
{
    public bool HasDevDrive { get; init; }

    public IReadOnlyList<DevDriveVolumeInfo> DevDriveVolumes { get; init; } = [];

    public IReadOnlyList<DevDriveVolumeInfo> AllVolumes { get; init; } = [];

    public IReadOnlyList<DevPathPlacement> PathPlacements { get; init; } = [];

    public IReadOnlyList<DevDriveRecommendation> Recommendations { get; init; } = [];

    public int PathsOnDevDrive { get; init; }

    public int PathsNeedingMigration { get; init; }

    public bool HasActionableRecommendations => Recommendations.Any(r =>
        r.Priority is DevDriveRecommendationPriority.High or DevDriveRecommendationPriority.Medium);

    public string SummaryText
    {
        get
        {
            if (!HasDevDrive)
                return "Kein Dev Drive erkannt — bis zu 30 % schnellere Builds möglich.";

            if (PathsNeedingMigration == 0)
                return $"Dev Drive aktiv · {PathsOnDevDrive} Dev-Pfad(e) optimal platziert.";

            return $"Dev Drive aktiv · {PathsNeedingMigration} Pfad(e) sollten migriert werden.";
        }
    }
}
