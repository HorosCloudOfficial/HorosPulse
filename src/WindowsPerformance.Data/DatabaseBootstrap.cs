namespace WindowsPerformance.Data;

using Microsoft.Data.Sqlite;

public sealed class DatabaseBootstrap
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        DataPaths.EnsureDirectories();

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS snapshots (
                id TEXT NOT NULL PRIMARY KEY,
                created_at TEXT NOT NULL,
                label TEXT NOT NULL,
                module TEXT NOT NULL,
                state_json TEXT NOT NULL,
                checksum TEXT NOT NULL
            );
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS audit_entries (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                operation TEXT NOT NULL,
                module TEXT NOT NULL,
                actor TEXT NOT NULL,
                details TEXT
            );
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS performance_metrics (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                cpu_percent REAL NOT NULL,
                ram_used_mb INTEGER NOT NULL,
                ram_total_mb INTEGER NOT NULL,
                disk_active_percent REAL NOT NULL
            );
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE INDEX IF NOT EXISTS idx_snapshots_created_at ON snapshots(created_at DESC);
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE INDEX IF NOT EXISTS idx_audit_timestamp ON audit_entries(timestamp DESC);
            """, cancellationToken);

        await ExecuteAsync(connection, """
            CREATE INDEX IF NOT EXISTS idx_metrics_timestamp ON performance_metrics(timestamp DESC);
            """, cancellationToken);
    }

    internal static SqliteConnection CreateConnection() =>
        new($"Data Source={DataPaths.DatabasePath}");

    private static async Task ExecuteAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
