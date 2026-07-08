namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>Plattform-Abstraktion zur Erkennung von Dev-Drive-Volumes (testbar).</summary>
public interface IDevDriveVolumeProbe
{
    IReadOnlyList<DevDriveVolumeInfo> GetVolumes();

    DevDriveVolumeInfo? GetVolumeForPath(string path);
}
