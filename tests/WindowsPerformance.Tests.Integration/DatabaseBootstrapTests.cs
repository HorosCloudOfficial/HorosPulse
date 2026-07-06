namespace WindowsPerformance.Tests.Integration;

using FluentAssertions;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Data;
using WindowsPerformance.Data.Repositories;
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
}
