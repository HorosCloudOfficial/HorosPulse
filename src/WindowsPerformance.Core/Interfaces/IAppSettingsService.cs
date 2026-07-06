namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// Lädt und speichert App-Einstellungen (JSON in %LOCALAPPDATA%).
/// </summary>
public interface IAppSettingsService
{
    /// <summary>Aktuell geladene Einstellungen.</summary>
    AppSettings Current { get; }

    /// <summary>Einstellungen von Disk laden.</summary>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Einstellungen auf Disk speichern.</summary>
    Task SaveAsync(CancellationToken cancellationToken = default);
}
