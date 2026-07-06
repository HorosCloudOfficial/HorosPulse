namespace WindowsPerformance.Core.Interfaces;

/// <summary>
/// Schreibt Audit-Einträge für Operationen und Modul-Änderungen.
/// </summary>
public interface IAuditLogger
{
    /// <summary>Allgemeinen Audit-Eintrag schreiben.</summary>
    Task LogAsync(string operation, string module, string details, CancellationToken cancellationToken = default);

    /// <summary>Apply-Operation eines Moduls protokollieren.</summary>
    Task LogApplyAsync(string module, bool success, string? beforeValue, string? afterValue, CancellationToken cancellationToken = default);

    /// <summary>Rollback-Operation eines Moduls protokollieren.</summary>
    Task LogRollbackAsync(string module, bool success, string? details, CancellationToken cancellationToken = default);
}
