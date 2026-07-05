namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IAuditRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<AuditEntry> InsertAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int limit = 200, CancellationToken cancellationToken = default);
}
