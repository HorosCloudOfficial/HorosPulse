namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// SQLite-Persistenz für Snapshots.
/// </summary>
public interface ISnapshotRepository
{
    /// <summary>Datenbank und Schema initialisieren.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Snapshot einfügen.</summary>
    Task<SnapshotEntry> InsertAsync(SnapshotEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Alle Snapshots abrufen.</summary>
    Task<IReadOnlyList<SnapshotEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Snapshot per ID laden.</summary>
    Task<SnapshotEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Snapshot löschen.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Älteste Snapshots über Retention-Limit entfernen.</summary>
    Task DeleteOldestBeyondLimitAsync(int limit, CancellationToken cancellationToken = default);
}
