namespace HorosPulse.Core.Models;

/// <summary>
/// Tracking für .wslconfig-Änderungen durch HorosPulse (Rollback).
/// </summary>
public sealed class WslDockerTrackingData
{
    public bool PreviousFileExisted { get; init; }

    public string? PreviousWslConfigContent { get; init; }

    public string? BackupFilePath { get; init; }

    public DateTimeOffset AppliedAt { get; init; }
}
