namespace WindowsPerformance.Data.Repositories;

using Microsoft.Data.Sqlite;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class AuditRepository : IAuditRepository
{
    private readonly DatabaseBootstrap _bootstrap;

    public AuditRepository(DatabaseBootstrap bootstrap) => _bootstrap = bootstrap;

    public async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        await _bootstrap.InitializeAsync(cancellationToken);

    public async Task<AuditEntry> InsertAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO audit_entries (timestamp, operation, module, actor, details)
            VALUES ($timestamp, $operation, $module, $actor, $details);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$timestamp", entry.Timestamp.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$operation", entry.Operation);
        command.Parameters.AddWithValue("$module", entry.Module);
        command.Parameters.AddWithValue("$actor", entry.Actor);
        command.Parameters.AddWithValue("$details", (object?)entry.Details ?? DBNull.Value);

        var id = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return new AuditEntry
        {
            Id = id,
            Timestamp = entry.Timestamp,
            Operation = entry.Operation,
            Module = entry.Module,
            Actor = entry.Actor,
            Details = entry.Details,
        };
    }

    public async Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int limit = 200, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, timestamp, operation, module, actor, details
            FROM audit_entries
            ORDER BY timestamp DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);

        var results = new List<AuditEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AuditEntry
            {
                Id = reader.GetInt64(0),
                Timestamp = DateTimeOffset.Parse(reader.GetString(1)),
                Operation = reader.GetString(2),
                Module = reader.GetString(3),
                Actor = reader.GetString(4),
                Details = reader.IsDBNull(5) ? null : reader.GetString(5),
            });
        }

        return results;
    }
}
