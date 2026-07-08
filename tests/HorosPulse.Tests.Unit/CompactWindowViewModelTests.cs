namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.ViewModels;
using Moq;
using Xunit;

public class CompactWindowViewModelTests
{
    [Fact]
    public void Title_IsDigiPet()
    {
        var vm = CreateViewModel();
        vm.Title.Should().Be("HorosPulse Digi-Pet");
    }

    [Fact]
    public void ReloadFromSettings_AppliesCompactWindowFlags()
    {
        var settings = new AppSettings
        {
            CompactWindow = new CompactWindowSettings
            {
                ShowRamStats = false,
                ShowCpuStats = false,
                ShowDiskStats = true,
                ShowMemoryCleanAction = false,
                ShowCursorDevModeAction = false,
                ShowDiskOptimizeAction = true,
                ShowVisualEffectsAction = true,
                PurgeWorkingSet = false,
                PurgeSystemFileCache = true,
                PurgeModifiedPageList = false,
                PurgeStandbyList = true,
                PurgeLowPriorityStandby = false,
                PurgeRegistryCache = true,
                PurgeCombineMemoryLists = false,
            },
        };

        var vm = CreateViewModel(settings);
        vm.ReloadFromSettings();

        vm.ShowRamStats.Should().BeFalse();
        vm.ShowCpuStats.Should().BeFalse();
        vm.ShowDiskStats.Should().BeTrue();
        vm.ShowMemoryCleanAction.Should().BeFalse();
        vm.ShowCursorDevModeAction.Should().BeFalse();
        vm.ShowDiskOptimizeAction.Should().BeTrue();
        vm.ShowVisualEffectsAction.Should().BeTrue();
        vm.PurgeWorkingSet.Should().BeFalse();
        vm.PurgeSystemFileCache.Should().BeTrue();
        vm.PurgeModifiedPageList.Should().BeFalse();
        vm.PurgeStandbyList.Should().BeTrue();
        vm.PurgeLowPriorityStandby.Should().BeFalse();
        vm.PurgeRegistryCache.Should().BeTrue();
        vm.PurgeCombineMemoryLists.Should().BeFalse();
    }

    [Fact]
    public async Task RamPercentDisplay_FormatsFromMemoryStatus()
    {
        var metrics = new Mock<IMetricsCollector>();
        metrics.Setup(m => m.GetCurrentMetricAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceMetric(DateTimeOffset.UtcNow, 10, 4000, 8000, 5));

        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetAvailableMemoryMbAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(4000);
        memory.Setup(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStatusSnapshot(8000, 4000, 16000, 12000, 2000, 500));

        var vm = CreateViewModel(metrics: metrics, memory: memory);
        await vm.RefreshMetricsCommand.ExecuteAsync(null);

        vm.RamPercentDisplay.Should().Be("50%");
        vm.RamDisplay.Should().MatchRegex(@"4[.,]000");
        vm.PageFileDisplay.Should().MatchRegex(@"12[.,]000");
        vm.SystemReservedDisplay.Should().MatchRegex(@"1[.,]500");
        vm.SystemReservedUsedPercent.Should().Be(75);
    }

    [Fact]
    public async Task SystemReservedDisplay_MatchesUsedPercentBar()
    {
        var metrics = new Mock<IMetricsCollector>();
        metrics.Setup(m => m.GetCurrentMetricAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceMetric(DateTimeOffset.UtcNow, 0, 0, 0, 0));

        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStatusSnapshot(0, 0, 0, 0, 14219, 0));

        var vm = CreateViewModel(metrics: metrics, memory: memory);
        await vm.RefreshMetricsCommand.ExecuteAsync(null);

        vm.SystemReservedDisplay.Should().MatchRegex(@"14[.,]219");
        vm.SystemReservedUsedPercent.Should().Be(100);
    }

    [Fact]
    public async Task CleanMemoryAsync_DoesNotHealOnFailure()
    {
        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetAvailableMemoryMbAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(4000);
        memory.Setup(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStatusSnapshot(8000, 4000, 16000, 12000, 2000, 500));
        memory.Setup(m => m.PurgeMemoryAsync(It.IsAny<MemoryPurgeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Fail("Zeitüberschreitung beim Verbinden mit HorosPulse.Elevation.exe."));

        var vm = CreateViewModel(memory: memory);
        await vm.CleanMemoryCommand.ExecuteAsync(null);

        vm.IsHealing.Should().BeFalse();
        vm.IsStatusSuccess.Should().BeFalse();
        vm.StatusMessage.Should().Contain("Zeitüberschreitung");
    }

    [Theory]
    [InlineData(50, 50, PetHealthState.Healthy)]
    [InlineData(75, 50, PetHealthState.Tired)]
    [InlineData(90, 50, PetHealthState.Sick)]
    [InlineData(50, 90, PetHealthState.Sick)]
    public async Task PetState_ReflectsRamAndCpu(long ramUsedMb, double cpuPercent, PetHealthState expected)
    {
        var metrics = new Mock<IMetricsCollector>();
        metrics.Setup(m => m.GetCurrentMetricAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceMetric(DateTimeOffset.UtcNow, cpuPercent, ramUsedMb, 100, 0));

        var vm = CreateViewModel(metrics: metrics);
        await vm.RefreshMetricsCommand.ExecuteAsync(null);
        vm.PetState.Should().Be(expected);
    }

    [Fact]
    public void OpenMainWindowCommand_CallsCoordinator()
    {
        var coordinator = new Mock<ICompactWindowCoordinator>();
        var vm = CreateViewModel(coordinator: coordinator);

        vm.OpenMainWindowCommand.Execute(null);

        coordinator.Verify(c => c.ShowMainWindow(), Times.Once);
    }

    private static CompactWindowViewModel CreateViewModel(
        AppSettings? settings = null,
        Mock<IMetricsCollector>? metrics = null,
        Mock<IMemoryOptimizerService>? memory = null,
        Mock<ICompactWindowCoordinator>? coordinator = null)
    {
        metrics ??= CreateDefaultMetricsMock();
        memory ??= CreateDefaultMemoryMock();

        var presets = new Mock<IPresetOrchestrator>();
        var disk = new Mock<IDiskOptimizerService>();
        var visual = new Mock<IVisualEffectsService>();
        visual.Setup(v => v.ApplyPresetAsync(It.IsAny<VisualEffectsPreset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("ok"));

        var appSettings = new Mock<IAppSettingsService>();
        appSettings.SetupGet(s => s.Current).Returns(settings ?? new AppSettings());

        coordinator ??= new Mock<ICompactWindowCoordinator>();

        return new CompactWindowViewModel(
            metrics.Object,
            memory.Object,
            presets.Object,
            disk.Object,
            visual.Object,
            appSettings.Object,
            coordinator.Object);
    }

    private static Mock<IMetricsCollector> CreateDefaultMetricsMock()
    {
        var metrics = new Mock<IMetricsCollector>();
        metrics.Setup(m => m.GetCurrentMetricAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceMetric(DateTimeOffset.UtcNow, 0, 0, 0, 0));
        return metrics;
    }

    private static Mock<IMemoryOptimizerService> CreateDefaultMemoryMock()
    {
        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetAvailableMemoryMbAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        memory.Setup(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MemoryStatusSnapshot.Empty);
        memory.Setup(m => m.PurgeMemoryAsync(It.IsAny<MemoryPurgeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Ok("ok"));
        return memory;
    }
}
