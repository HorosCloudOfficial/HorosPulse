namespace WindowsPerformance.Tests.Integration;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Services.Rollback;
using WindowsPerformance.Services.Stubs;
using Xunit;

public class SnapshotRollbackEndToEndTests
{
    [Fact]
    public async Task RollbackEngine_RollsBackFakeModule_FromSnapshotEntry()
    {
        var fakeModule = new FakeOptimizationModule();
        var modules = new IOptimizationModule[] { fakeModule };

        var snapshotManager = new Mock<ISnapshotManager>();
        var audit = new Mock<IAuditLogger>();
        audit.Setup(a => a.LogRollbackAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var rollbackEngine = new RollbackEngine(
            snapshotManager.Object,
            modules,
            audit.Object,
            NullLogger<RollbackEngine>.Instance);

        fakeModule.Applied = true;

        var snapshot = new SnapshotEntry
        {
            Label = "e2e-fake",
            Module = "Fake",
            IsValid = true,
            CanRollback = true,
        };

        var rollbackResult = await rollbackEngine.RollbackSnapshotAsync(snapshot);
        rollbackResult.Success.Should().BeTrue();
        fakeModule.Applied.Should().BeFalse();
    }

    private sealed class FakeOptimizationModule : IOptimizationModule
    {
        public bool Applied { get; set; }
        public string ModuleName => "Fake";
        public bool CanApply => true;

        public Task ApplyAsync(CancellationToken cancellationToken = default)
        {
            Applied = true;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            Applied = false;
            return Task.CompletedTask;
        }
    }
}
