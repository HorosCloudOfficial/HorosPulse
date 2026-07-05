namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface ISnapshotManager
{
    Task<SnapshotEntry> CreateBaselineAsync(string label, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SnapshotEntry>> GetSnapshotsAsync(CancellationToken cancellationToken = default);
    Task<SnapshotEntry?> GetSnapshotAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteSnapshotAsync(Guid id, CancellationToken cancellationToken = default);
}
