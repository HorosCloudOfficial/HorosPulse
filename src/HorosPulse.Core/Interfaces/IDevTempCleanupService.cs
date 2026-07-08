namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Scannt und bereinigt typische Dev-Caches und Temp-Ordner sicher.
/// Löscht niemals %USERPROFILE%-Root, node_modules oder global-packages ohne Extra-Bestätigung.
/// </summary>
public interface IDevTempCleanupService
{
    /// <summary>Alle bekannten Dev-Cache-Pfade scannen und klassifizieren.</summary>
    Task<DevTempCleanupScanResult> ScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ausgewählte sichere Caches bereinigen.
    /// <paramref name="entryIds"/> — IDs aus dem Scan.
    /// <paramref name="allowGlobalPackages"/> — nur true nach expliziter Extra-Bestätigung.
    /// </summary>
    Task<DevTempCleanupResult> CleanupAsync(
        IReadOnlyList<string> entryIds,
        bool allowGlobalPackages = false,
        CancellationToken cancellationToken = default);
}
