namespace HorosPulse.Core.Models;

/// <summary>Prüfung, ob ein typischer Dev-Pfad auf einem Dev Drive liegt.</summary>
public sealed class DevPathPlacement
{
    public required string DisplayName { get; init; }

    public required string Path { get; init; }

    public bool PathExists { get; init; }

    public string? ResolvedDriveLetter { get; init; }

    public bool IsOnDevDrive { get; init; }

    public bool IsOnReFs { get; init; }

    public DevPathPlacementStatus Status { get; init; }

    public string StatusLabel => Status switch
    {
        DevPathPlacementStatus.OnDevDrive => "Auf Dev Drive",
        DevPathPlacementStatus.OnReFsNotDev => "ReFS (kein Dev Drive)",
        DevPathPlacementStatus.OnSlowVolume => "Langsames Volume",
        DevPathPlacementStatus.NotFound => "Nicht vorhanden",
        DevPathPlacementStatus.NoDevDriveAvailable => "Kein Dev Drive",
        _ => "Unbekannt",
    };
}

public enum DevPathPlacementStatus
{
    OnDevDrive,
    OnReFsNotDev,
    OnSlowVolume,
    NotFound,
    NoDevDriveAvailable,
    Unknown,
}
