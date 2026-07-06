namespace WindowsPerformance.Tests.Unit;

using FluentAssertions;
using Moq;
using WindowsPerformance.Core;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.ViewModels;
using Xunit;

public class PresetsViewModelTests
{
    [Fact]
    public void Title_IsPresets()
    {
        var vm = CreateViewModel();
        vm.Title.Should().Be("Presets");
    }

    [Fact]
    public async Task LoadPresetsAsync_PopulatesBuiltInPresets()
    {
        var vm = CreateViewModel();
        await vm.LoadPresetsCommand.ExecuteAsync(null);
        vm.Presets.Should().Contain(p => p.Id == PresetIds.CursorDevMode);
    }

    [Fact]
    public void SavePresetCommand_DisabledWhenNameEmpty()
    {
        var vm = CreateViewModel();
        vm.NewPresetName = string.Empty;
        vm.SavePresetCommand.CanExecute(null).Should().BeFalse();
    }

    private static PresetsViewModel CreateViewModel()
    {
        var orchestrator = new Mock<IPresetOrchestrator>();
        orchestrator.Setup(o => o.GetPresetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ProfileDefinition { Id = PresetIds.CursorDevMode, Name = "Cursor Dev Mode", IsBuiltIn = true },
            ]);

        var profiles = new Mock<IProfileRepository>();
        var rollback = new Mock<IRollbackEngine>();
        var snapshots = new Mock<ISnapshotManager>();
        var confirmation = new Mock<IUserConfirmationService>();
        var filePicker = new Mock<IFilePickerService>();

        return new PresetsViewModel(
            orchestrator.Object,
            profiles.Object,
            rollback.Object,
            snapshots.Object,
            confirmation.Object,
            filePicker.Object);
    }
}
