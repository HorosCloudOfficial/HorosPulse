namespace WindowsPerformance.Data.Repositories;

using Microsoft.Data.Sqlite;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class SnapshotRepository : ISnapshotRepository
{
    private readonly DatabaseBootstrap _bootstrap;

    public SnapshotRepository(DatabaseBootstrap bootstrap) => _bootstrap = bootstrap;

    public async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        await _bootstrap.InitializeAsync(cancellationToken);

    public async Task<SnapshotEntry> InsertAsync(SnapshotEntry entry, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO snapshots (id, created_at, label, module, state_json, checksum)
            VALUES ($id, $createdAt, $label, $module, $stateJson, $checksum);
            """;
        command.Parameters.AddWithValue("$id", entry.Id.ToString());
        command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$label", entry.Label);
        command.Parameters.AddWithValue("$module", entry.Module);
        command.Parameters.AddWithValue("$stateJson", entry.StateJson);
        command.Parameters.AddWithValue("$checksum", entry.Checksum);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return entry;
    }

    public async Task<IReadOnlyList<SnapshotEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, label, module, state_json, checksum
            FROM snapshots
            ORDER BY created_at DESC;
            """;

        var results = new List<SnapshotEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadEntry(reader));
        }

        return results;
    }

    public async Task<SnapshotEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, label, module, state_json, checksum
            FROM snapshots
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadEntry(reader) : null;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM snapshots WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteOldestBeyondLimitAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
            return;

        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM snapshots
            WHERE id IN (
                SELECT id FROM snapshots
                ORDER BY created_at DESC
                LIMIT -1 OFFSET $limit
            );
            """;
        command.Parameters.AddWithValue("$limit", limit);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SnapshotEntry ReadEntry(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var createdAt = DateTimeOffset.Parse(reader.GetString(1));
        var label = reader.GetString(2);
        var module = reader.GetString(3);
        var stateJson = reader.GetString(4);
        var checksum = reader.GetString(5);
        var isValid = SnapshotCompression.ValidateChecksum(stateJson, checksum);

        return new SnapshotEntry
        {
            Id = id,
            CreatedAt = createdAt,
            Label = label,
            Module = module,
            StateJson = stateJson,
            Checksum = checksum,
            IsValid = isValid,
            CanRollback = isValid,
        };
    }
}
