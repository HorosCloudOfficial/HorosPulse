namespace HorosPulse.Data.Repositories;

using Microsoft.Data.Sqlite;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class PerformanceMetricRepository : IPerformanceMetricRepository
{
    private readonly DatabaseBootstrap _bootstrap;

    public PerformanceMetricRepository(DatabaseBootstrap bootstrap) => _bootstrap = bootstrap;

    public async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        await _bootstrap.InitializeAsync(cancellationToken);

    public async Task InsertAsync(PerformanceMetric metric, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO performance_metrics (timestamp, cpu_percent, ram_used_mb, ram_total_mb, disk_active_percent)
            VALUES ($timestamp, $cpu, $ramUsed, $ramTotal, $disk);
            """;
        command.Parameters.AddWithValue("$timestamp", metric.Timestamp.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$cpu", metric.CpuPercent);
        command.Parameters.AddWithValue("$ramUsed", metric.RamUsedMb);
        command.Parameters.AddWithValue("$ramTotal", metric.RamTotalMb);
        command.Parameters.AddWithValue("$disk", metric.DiskActivePercent);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PerformanceMetric>> GetRecentAsync(int limit = 60, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT timestamp, cpu_percent, ram_used_mb, ram_total_mb, disk_active_percent
            FROM performance_metrics
            ORDER BY timestamp DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);

        var results = new List<PerformanceMetric>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PerformanceMetric(
                DateTimeOffset.Parse(reader.GetString(0)),
                reader.GetDouble(1),
                reader.GetInt64(2),
                reader.GetInt64(3),
                reader.GetDouble(4)));
        }

        results.Reverse();
        return results;
    }

    public async Task<IReadOnlyList<PerformanceMetric>> GetSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT timestamp, cpu_percent, ram_used_mb, ram_total_mb, disk_active_percent
            FROM performance_metrics
            WHERE timestamp >= $since
            ORDER BY timestamp ASC;
            """;
        command.Parameters.AddWithValue("$since", since.UtcDateTime.ToString("O"));

        var results = new List<PerformanceMetric>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PerformanceMetric(
                DateTimeOffset.Parse(reader.GetString(0)),
                reader.GetDouble(1),
                reader.GetInt64(2),
                reader.GetInt64(3),
                reader.GetDouble(4)));
        }

        return results;
    }

    public async Task PurgeOlderThanAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(maxAge).UtcDateTime.ToString("O");

        await using var connection = DatabaseBootstrap.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM performance_metrics WHERE timestamp < $cutoff;";
        command.Parameters.AddWithValue("$cutoff", cutoff);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
