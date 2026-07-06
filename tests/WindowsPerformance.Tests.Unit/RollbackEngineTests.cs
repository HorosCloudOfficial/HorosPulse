namespace WindowsPerformance.Tests.Unit;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Services.Rollback;
using Xunit;

public class RollbackEngineTests
{
    [Fact]
    public async Task RollbackSnapshotAsync_FullSystem_CallsAllModules()
    {
        var rolledBack = new List<string>();
        var modules = new[]
        {
            CreateModule("PowerPlan", () => rolledBack.Add("PowerPlan")),
            CreateModule("Cursor", () => rolledBack.Add("Cursor")),
            CreateModule("ProcessPriority", () => rolledBack.Add("ProcessPriority")),
            CreateModule("IndexerExclusion", () => rolledBack.Add("IndexerExclusion")),
            CreateModule("DefenderExclusion", () => rolledBack.Add("DefenderExclusion")),
        };

        var snapshotManager = new Mock<ISnapshotManager>();
        snapshotManager.Setup(s => s.CreateBaselineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SnapshotEntry { Label = "post-rollback-test" });
        var audit = new Mock<IAuditLogger>();
        audit.Setup(a => a.LogRollbackAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var engine = new RollbackEngine(
            snapshotManager.Object,
            modules,
            audit.Object,
            NullLogger<RollbackEngine>.Instance);

        var snapshot = new SnapshotEntry
        {
            Label = "test",
            Module = "FullSystem",
            IsValid = true,
            CanRollback = true,
        };

        var result = await engine.RollbackSnapshotAsync(snapshot);

        result.Success.Should().BeTrue();
        rolledBack.Should().Contain("Cursor");
        rolledBack.Should().Contain("PowerPlan");
    }

    [Fact]
    public async Task RollbackSnapshotAsync_AggregatesPartialFailures()
    {
        var modules = new[]
        {
            CreateModule("Cursor", () => { }),
            CreateModule("PowerPlan", () => throw new InvalidOperationException("power fail")),
        };

        var audit = new Mock<IAuditLogger>();
        audit.Setup(a => a.LogRollbackAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var engine = new RollbackEngine(
            Mock.Of<ISnapshotManager>(),
            modules,
            audit.Object,
            NullLogger<RollbackEngine>.Instance);

        var snapshot = new SnapshotEntry
        {
            Module = "FullSystem",
            IsValid = true,
            CanRollback = true,
        };

        var result = await engine.RollbackSnapshotAsync(snapshot);

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().Contain("power fail");
    }

    private static IOptimizationModule CreateModule(string name, Action onRollback)
    {
        var module = new Mock<IOptimizationModule>();
        module.SetupGet(m => m.ModuleName).Returns(name);
        module.Setup(m => m.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                onRollback();
                return Task.CompletedTask;
            });
        return module.Object;
    }
}
