namespace WindowsPerformance.Core.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string operation, string module, string details, CancellationToken cancellationToken = default);
    Task LogApplyAsync(string module, bool success, string? beforeValue, string? afterValue, CancellationToken cancellationToken = default);
    Task LogRollbackAsync(string module, bool success, string? details, CancellationToken cancellationToken = default);
}
