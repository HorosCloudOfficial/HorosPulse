namespace WindowsPerformance.Services.Audit;

using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class AuditLogger : IAuditLogger
{
    private readonly IAuditRepository _auditRepository;

    public AuditLogger(IAuditRepository auditRepository) => _auditRepository = auditRepository;

    public async Task LogAsync(
        string operation,
        string module,
        string details,
        CancellationToken cancellationToken = default)
    {
        await _auditRepository.InitializeAsync(cancellationToken);
        await _auditRepository.InsertAsync(new AuditEntry
        {
            Operation = operation,
            Module = module,
            Details = details,
        }, cancellationToken);
    }

    public Task LogApplyAsync(
        string module,
        bool success,
        string? beforeValue,
        string? afterValue,
        CancellationToken cancellationToken = default)
    {
        var details = $"Erfolg={success}; Vorher={beforeValue ?? "—"}; Nachher={afterValue ?? "—"}";
        return LogAsync(success ? "Apply" : "ApplyFailed", module, details, cancellationToken);
    }

    public Task LogRollbackAsync(
        string module,
        bool success,
        string? details,
        CancellationToken cancellationToken = default) =>
        LogAsync(success ? "Rollback" : "RollbackFailed", module, details ?? string.Empty, cancellationToken);
}
