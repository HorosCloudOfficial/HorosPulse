namespace HorosPulse.Core.Models;

/// <summary>Ergebnis eines Dev-Cache-Scans.</summary>
public sealed class DevTempCleanupScanResult
{
    public required IReadOnlyList<DevTempCacheEntry> Entries { get; init; }

    public long TotalSafeDeletableBytes { get; init; }

    public string SummaryText
    {
        get
        {
            var safeCount = Entries.Count(e => e.IsDeletable);
            var infoCount = Entries.Count(e => e.Safety == DevTempCacheSafety.InfoOnly);
            return safeCount > 0
                ? $"{safeCount} sicher löschbare(r) Cache(s), {DevTempCacheEntry.FormatBytes(TotalSafeDeletableBytes)} freigebbar. {infoCount} nur Info."
                : infoCount > 0
                    ? $"Keine sicheren Caches zum Löschen. {infoCount} Eintrag/Einträge nur zur Information."
                    : "Keine Dev-Caches gefunden.";
        }
    }
}
