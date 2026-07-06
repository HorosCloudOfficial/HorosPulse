namespace HorosPulse.Tests.Integration;

using FluentAssertions;
using HorosPulse.Core.Models;
using HorosPulse.Data;
using HorosPulse.Data.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

public class DatabaseBootstrapTests
{
    [Fact]
    public async Task InitializeAsync_CreatesTables_InMemoryDatabase()
    {
        DataPaths.TestDatabasePathOverride = $"file:wp-mem-{Guid.NewGuid():N}?mode=memory&cache=shared";

        try
        {
            var bootstrap = new DatabaseBootstrap();
            await bootstrap.InitializeAsync();

            var auditRepo = new AuditRepository(bootstrap);
            var entry = await auditRepo.InsertAsync(new AuditEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Operation = "Test",
                Module = "Integration",
                Actor = "Test",
                Details = "Bootstrap OK",
            });

            entry.Id.Should().BeGreaterThan(0);
        }
        finally
        {
            DataPaths.TestDatabasePathOverride = null;
        }
    }

    [Fact]
    public async Task InitializeAsync_DoesNotThrow_WhenBackupFileAlreadyExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"horospulse-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var dbPath = Path.Combine(tempDir, "data.db");
        await using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            await connection.OpenAsync();
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var existingBackup = $"{dbPath}.backup.{timestamp}";
        await File.WriteAllTextAsync(existingBackup, "existing backup");

        DataPaths.TestDatabasePathOverride = dbPath;

        try
        {
            var bootstrap = new DatabaseBootstrap();

            var act = async () =>
            {
                await bootstrap.InitializeAsync();
                await bootstrap.InitializeAsync();
            };

            await act.Should().NotThrowAsync();
        }
        finally
        {
            DataPaths.TestDatabasePathOverride = null;
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                catch (IOException)
                {
                    // Best-effort cleanup; temp dir is disposable.
                }
            }
        }
    }
}
