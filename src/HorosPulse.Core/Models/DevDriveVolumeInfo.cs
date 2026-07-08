namespace HorosPulse.Core.Models;

/// <summary>Informationen zu einem erkannten Volume (inkl. Dev-Drive-Status).</summary>
public sealed class DevDriveVolumeInfo
{
    public required string DriveLetter { get; init; }

    public string? VolumeLabel { get; init; }

    public required string FileSystem { get; init; }

    public bool IsDevDrive { get; init; }

    public bool IsReFs { get; init; }

    public long FreeBytes { get; init; }

    public long TotalBytes { get; init; }

    public string FreeSpaceDisplay => FormatBytes(FreeBytes);

    public string TotalSpaceDisplay => FormatBytes(TotalBytes);

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0)
            return "—";

        const double gb = 1024 * 1024 * 1024;
        if (bytes >= gb)
            return $"{bytes / gb:F1} GB";

        const double mb = 1024 * 1024;
        return $"{bytes / mb:F0} MB";
    }
}
