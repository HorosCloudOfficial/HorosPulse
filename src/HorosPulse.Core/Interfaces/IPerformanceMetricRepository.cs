namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Persistiert Performance-Metriken (CPU, RAM, Disk) in SQLite.
/// </summary>
public interface IPerformanceMetricRepository
{
    /// <summary>Datenbank und Schema initialisieren.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Metrik-Wert einfügen.</summary>
    Task InsertAsync(PerformanceMetric metric, CancellationToken cancellationToken = default);

    /// <summary>Neueste Metriken abrufen.</summary>
    Task<IReadOnlyList<PerformanceMetric>> GetRecentAsync(int limit = 60, CancellationToken cancellationToken = default);

    /// <summary>Einträge älter als maxAge löschen.</summary>
    Task PurgeOlderThanAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

    /// <summary>Metriken seit einem Zeitpunkt abrufen.</summary>
    Task<IReadOnlyList<PerformanceMetric>> GetSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
}
