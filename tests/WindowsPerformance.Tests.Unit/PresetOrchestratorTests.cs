namespace WindowsPerformance.Tests.Unit;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WindowsPerformance.Core;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Services.Presets;
using Xunit;

public class PresetOrchestratorTests
{
    [Fact]
    public async Task ApplyPresetAsync_CursorDevMode_CreatesSnapshotFirst()
    {
        var callOrder = new List<string>();

        var profileRepo = new Mock<IProfileRepository>();
        profileRepo.Setup(r => r.EnsureDefaultPresetsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        profileRepo.Setup(r => r.GetByIdAsync(PresetIds.CursorDevMode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDefinition { Id = PresetIds.CursorDevMode, Name = "Cursor Dev Mode" });

        var snapshotManager = new Mock<ISnapshotManager>();
        snapshotManager.Setup(s => s.CreateBaselineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SnapshotEntry { Id = Guid.NewGuid(), Label = "before_cursor_dev_mode" })
            .Callback(() => callOrder.Add("snapshot"));

        var powerPlan = new Mock<IPowerPlanService>();
        powerPlan.Setup(p => p.EnsureHighPerformancePlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("high"))
            .Callback(() => callOrder.Add("power"));

        var processPriority = new Mock<IProcessPriorityService>();
        processPriority.Setup(p => p.ApplyCursorPrioritiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("prio"))
            .Callback(() => callOrder.Add("priority"));

        var indexer = new Mock<IIndexerExclusionService>();
        indexer.Setup(i => i.GetAvailableEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IndexerExcludeEntry>());
        indexer.Setup(i => i.ApplyExclusionsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Fail("no paths"))
            .Callback(() => callOrder.Add("indexer"));

        var defender = new Mock<IDefenderExclusionService>();
        var cursor = new Mock<ICursorOptimizer>();
        cursor.Setup(c => c.ApplyOptimizationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("cursor"))
            .Callback(() => callOrder.Add("cursor"));

        var audit = new Mock<IAuditLogger>();
        audit.Setup(a => a.LogApplyAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        audit.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var settings = new Mock<IAppSettingsService>();
        settings.SetupGet(s => s.Current).Returns(new AppSettings { DefenderOptIn = false });

        var orchestrator = new PresetOrchestrator(
            profileRepo.Object,
            snapshotManager.Object,
            powerPlan.Object,
            processPriority.Object,
            indexer.Object,
            defender.Object,
            cursor.Object,
            audit.Object,
            settings.Object,
            NullLogger<PresetOrchestrator>.Instance);

        await orchestrator.ApplyPresetAsync(PresetIds.CursorDevMode);

        callOrder.First().Should().Be("snapshot");
        callOrder.Should().Contain("power");
        callOrder.IndexOf("snapshot").Should().BeLessThan(callOrder.IndexOf("power"));
        callOrder.IndexOf("power").Should().BeLessThan(callOrder.IndexOf("cursor"));
    }
}
