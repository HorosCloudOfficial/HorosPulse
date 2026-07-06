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

        processPriority.Setup(p => p.EnsureNodeNormalPriorityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("node"))
            .Callback(() => callOrder.Add("node"));

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

        var visualEffects = new Mock<IVisualEffectsService>();
        visualEffects.Setup(v => v.ApplyPresetAsync(It.IsAny<WindowsPerformance.Core.Enums.VisualEffectsPreset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("visual"))
            .Callback(() => callOrder.Add("visual"));

        var orchestrator = new PresetOrchestrator(
            profileRepo.Object,
            snapshotManager.Object,
            powerPlan.Object,
            processPriority.Object,
            indexer.Object,
            defender.Object,
            cursor.Object,
            visualEffects.Object,
            audit.Object,
            settings.Object,
            NullLogger<PresetOrchestrator>.Instance);

        await orchestrator.ApplyPresetAsync(PresetIds.CursorDevMode);

        callOrder.First().Should().Be("snapshot");
        callOrder.Should().Contain("priority");
        callOrder.Should().Contain("node");
        callOrder.IndexOf("snapshot").Should().BeLessThan(callOrder.IndexOf("power"));
        callOrder.IndexOf("power").Should().BeLessThan(callOrder.IndexOf("cursor"));
    }

    [Fact]
    public async Task ApplyPresetAsync_UserPreset_ExecutesConfiguredSteps()
    {
        const string userPresetId = "user-test-preset";

        var profileRepo = new Mock<IProfileRepository>();
        profileRepo.Setup(r => r.EnsureDefaultPresetsAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        profileRepo.Setup(r => r.GetByIdAsync(userPresetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDefinition
            {
                Id = userPresetId,
                Name = "Mein Preset",
                IsBuiltIn = false,
                Steps = [PresetStepIds.Snapshot, PresetStepIds.PowerPlanHighPerformance],
            });

        var snapshotManager = new Mock<ISnapshotManager>();
        snapshotManager.Setup(s => s.CreateBaselineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SnapshotEntry { Id = Guid.NewGuid(), Label = "before_mein_preset" });

        var powerPlan = new Mock<IPowerPlanService>();
        powerPlan.Setup(p => p.EnsureHighPerformancePlanAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("high"));

        var orchestrator = new PresetOrchestrator(
            profileRepo.Object,
            snapshotManager.Object,
            powerPlan.Object,
            Mock.Of<IProcessPriorityService>(),
            Mock.Of<IIndexerExclusionService>(),
            Mock.Of<IDefenderExclusionService>(),
            Mock.Of<ICursorOptimizer>(),
            Mock.Of<IVisualEffectsService>(),
            Mock.Of<IAuditLogger>(),
            Mock.Of<IAppSettingsService>(),
            NullLogger<PresetOrchestrator>.Instance);

        var result = await orchestrator.ApplyPresetAsync(userPresetId);

        result.Success.Should().BeTrue();
        result.Steps.Should().HaveCount(2);
        snapshotManager.Verify(s => s.CreateBaselineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        powerPlan.Verify(p => p.EnsureHighPerformancePlanAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
