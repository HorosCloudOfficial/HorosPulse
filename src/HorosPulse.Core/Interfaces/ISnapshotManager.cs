namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Erstellt und verwaltet System-Snapshots vor Optimierungen.
/// </summary>
public interface ISnapshotManager
{
    /// <summary>Baseline-Snapshot mit Label erstellen.</summary>
    Task<SnapshotEntry> CreateBaselineAsync(string label, CancellationToken cancellationToken = default);

    /// <summary>Alle Snapshots auflisten.</summary>
    Task<IReadOnlyList<SnapshotEntry>> GetSnapshotsAsync(CancellationToken cancellationToken = default);

    /// <summary>Einzelnen Snapshot per ID laden.</summary>
    Task<SnapshotEntry?> GetSnapshotAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Snapshot löschen.</summary>
    Task DeleteSnapshotAsync(Guid id, CancellationToken cancellationToken = default);
}
