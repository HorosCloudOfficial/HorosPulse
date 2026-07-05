namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface ISnapshotRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<SnapshotEntry> InsertAsync(SnapshotEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SnapshotEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SnapshotEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteOldestBeyondLimitAsync(int limit, CancellationToken cancellationToken = default);
}
