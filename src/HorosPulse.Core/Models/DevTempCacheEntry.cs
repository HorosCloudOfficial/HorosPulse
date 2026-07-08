namespace HorosPulse.Core.Models;

using System.Globalization;

/// <summary>Ein gescannter Dev-Cache- oder Temp-Pfad.</summary>
public sealed class DevTempCacheEntry
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string Path { get; init; }

    public long SizeBytes { get; init; }

    public bool PathExists { get; init; }

    public DevTempCacheSafety Safety { get; init; }

    public DevTempCacheCleanupMethod CleanupMethod { get; init; }

    public required string SafetyReason { get; init; }

    public bool IsDeletable => Safety == DevTempCacheSafety.SafeDeletable && CleanupMethod != DevTempCacheCleanupMethod.None;

    public bool RequiresExtraConfirmation => Safety == DevTempCacheSafety.RequiresExtraConfirmation;

    public string SizeDisplay => FormatBytes(SizeBytes);

    public string DeletableLabel => Safety switch
    {
        DevTempCacheSafety.SafeDeletable when CleanupMethod != DevTempCacheCleanupMethod.None => "Ja",
        DevTempCacheSafety.RequiresExtraConfirmation => "Mit Extra-Bestätigung",
        _ => "Nein",
    };

    public static string FormatBytes(long bytes)
    {
        if (bytes < 0)
            return "—";

        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;

        return bytes switch
        {
            >= gb => $"{(bytes / (double)gb).ToString("F2", CultureInfo.InvariantCulture)} GB",
            >= mb => $"{(bytes / (double)mb).ToString("F1", CultureInfo.InvariantCulture)} MB",
            >= kb => $"{(bytes / (double)kb).ToString("F0", CultureInfo.InvariantCulture)} KB",
            _ => $"{bytes} B",
        };
    }
}
