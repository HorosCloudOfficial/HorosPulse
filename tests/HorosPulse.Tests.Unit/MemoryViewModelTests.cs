namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.ViewModels;
using Moq;
using Xunit;

public class MemoryViewModelTests
{
    [Fact]
    public void Title_IsSpeicher()
    {
        var vm = CreateViewModel();
        vm.Title.Should().Be("Speicher");
    }

    [Fact]
    public void Constructor_LoadsPurgeDefaultsFromCompactWindowSettings()
    {
        var settings = new AppSettings
        {
            CompactWindow = new CompactWindowSettings
            {
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

        vm.PurgeWorkingSet.Should().BeFalse();
        vm.PurgeSystemFileCache.Should().BeTrue();
        vm.PurgeModifiedPageList.Should().BeFalse();
        vm.PurgeStandbyList.Should().BeTrue();
        vm.PurgeLowPriorityStandby.Should().BeFalse();
        vm.PurgeRegistryCache.Should().BeTrue();
        vm.PurgeCombineMemoryLists.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshMemoryAsync_PopulatesStatusAndRamValues()
    {
        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStatusSnapshot(8000, 4000, 16000, 12000, 2000, 500));

        var vm = CreateViewModel(memory: memory);
        await vm.RefreshMemoryCommand.ExecuteAsync(null);

        vm.RamBeforeMb.Should().Be(4000);
        vm.RamAfterMb.Should().Be(4000);
        vm.RamDisplay.Should().MatchRegex(@"4[.,]000");
        vm.PageFileDisplay.Should().MatchRegex(@"12[.,]000");
        vm.SystemReservedDisplay.Should().MatchRegex(@"1[.,]500");
        vm.PhysicalUsedPercent.Should().Be(50);
    }

    [Fact]
    public async Task ApplyPurgeAsync_UsesPurgeMemoryAndUpdatesRamAfter()
    {
        MemoryPurgeOptions? captured = null;
        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetAvailableMemoryMbAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3000);
        memory.Setup(m => m.PurgeMemoryAsync(It.IsAny<MemoryPurgeOptions>(), It.IsAny<CancellationToken>()))
            .Callback<MemoryPurgeOptions, CancellationToken>((options, _) => captured = options)
            .ReturnsAsync(OptimizationResult.Ok("ok"));
        memory.SetupSequence(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStatusSnapshot(8000, 3000, 16000, 12000, 2000, 500))
            .ReturnsAsync(new MemoryStatusSnapshot(8000, 4500, 16000, 12000, 2000, 500));

        var vm = CreateViewModel(memory: memory);
        vm.PurgeWorkingSet = true;
        vm.PurgeStandbyList = true;
        vm.PurgeRegistryCache = false;

        await vm.ApplyPurgeCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.PurgeWorkingSet.Should().BeTrue();
        captured.PurgeStandbyList.Should().BeTrue();
        captured.PurgeRegistryCache.Should().BeFalse();
        vm.RamBeforeMb.Should().Be(3000);
        vm.RamAfterMb.Should().Be(4500);
        vm.IsStatusSuccess.Should().BeTrue();
        vm.StatusMessage.Should().Contain("Bereinigung abgeschlossen");
    }

    [Fact]
    public async Task ApplyPurgeAsync_ReportsFailureWithoutChangingSuccessFlag()
    {
        var memory = new Mock<IMemoryOptimizerService>();
        memory.Setup(m => m.GetAvailableMemoryMbAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3000);
        memory.Setup(m => m.PurgeMemoryAsync(It.IsAny<MemoryPurgeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OptimizationResult.Fail("Elevation nicht verfügbar."));
        memory.Setup(m => m.GetMemoryStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStatusSnapshot(8000, 3000, 16000, 12000, 2000, 500));

        var vm = CreateViewModel(memory: memory);
        await vm.ApplyPurgeCommand.ExecuteAsync(null);

        vm.IsStatusSuccess.Should().BeFalse();
        vm.StatusMessage.Should().Contain("Elevation");
    }

    [Fact]
    public void ApplyPurgeCommand_IsDisabledWhenNoAreasSelected()
    {
        var vm = CreateViewModel();
        vm.PurgeWorkingSet = false;
        vm.PurgeSystemFileCache = false;
        vm.PurgeModifiedPageList = false;
        vm.PurgeStandbyList = false;
        vm.PurgeLowPriorityStandby = false;
        vm.PurgeRegistryCache = false;
        vm.PurgeCombineMemoryLists = false;

        vm.ApplyPurgeCommand.CanExecute(null).Should().BeFalse();
    }

    private static MemoryViewModel CreateViewModel(
        AppSettings? settings = null,
        Mock<IMemoryOptimizerService>? memory = null)
    {
        memory ??= CreateDefaultMemoryMock();

        var appSettings = new Mock<IAppSettingsService>();
        appSettings.SetupGet(s => s.Current).Returns(settings ?? new AppSettings());

        return new MemoryViewModel(memory.Object, appSettings.Object);
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
