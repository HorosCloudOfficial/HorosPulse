namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// Persistiert und lädt Audit-Einträge aus SQLite.
/// Append-only: nur <see cref="InsertAsync"/> — keine Update- oder Delete-Operationen.
/// </summary>
public interface IAuditRepository
{
    /// <summary>Datenbank und Schema initialisieren.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Neuen Audit-Eintrag einfügen.</summary>
    Task<AuditEntry> InsertAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Neueste Einträge abrufen.</summary>
    Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int limit = 200, CancellationToken cancellationToken = default);

    /// <summary>Einträge seit einem Zeitpunkt abrufen.</summary>
    Task<IReadOnlyList<AuditEntry>> GetSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);

    /// <summary>Audit-Einträge als CSV exportieren.</summary>
    Task<string> ExportToCsvAsync(CancellationToken cancellationToken = default);
}
