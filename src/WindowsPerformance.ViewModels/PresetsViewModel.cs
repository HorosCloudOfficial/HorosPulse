namespace WindowsPerformance.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Core;

public sealed partial class PresetsViewModel : ViewModelBase
{
    private readonly IPresetOrchestrator _presetOrchestrator;
    private readonly IRollbackEngine _rollbackEngine;
    private readonly ISnapshotManager _snapshotManager;

    public PresetsViewModel(
        IPresetOrchestrator presetOrchestrator,
        IRollbackEngine rollbackEngine,
        ISnapshotManager snapshotManager)
    {
        _presetOrchestrator = presetOrchestrator;
        _rollbackEngine = rollbackEngine;
        _snapshotManager = snapshotManager;
        _ = LoadPresetsAsync();
    }

    public string Title => "Presets";

    [ObservableProperty]
    private string _description = "Performance-Presets mit Snapshot-Sicherung";

    [ObservableProperty]
    private ProfileDefinition? _selectedPreset;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ObservableCollection<ProfileDefinition> Presets { get; } = new();
    public ObservableCollection<PresetStepResult> ApplySteps { get; } = new();

    [RelayCommand]
    private async Task LoadPresetsAsync()
    {
        IsBusy = true;
        try
        {
            Presets.Clear();
            var presets = await _presetOrchestrator.GetPresetsAsync();
            foreach (var preset in presets)
                Presets.Add(preset);

            SelectedPreset ??= Presets.FirstOrDefault(p =>
                p.Id == PresetIds.CursorDevMode) ?? Presets.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyPresetAsync()
    {
        if (SelectedPreset is null)
            return;

        IsBusy = true;
        StatusMessage = null;
        ApplySteps.Clear();
        try
        {
            var result = await _presetOrchestrator.ApplyPresetAsync(SelectedPreset.Id);
            foreach (var step in result.Steps)
                ApplySteps.Add(step);

            StatusMessage = result.Success
                ? $"Preset \"{result.PresetName}\" erfolgreich angewendet."
                : result.ErrorMessage ?? "Preset-Anwendung fehlgeschlagen.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackLatestAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var snapshots = await _snapshotManager.GetSnapshotsAsync();
            var latest = snapshots.FirstOrDefault();
            if (latest is null)
            {
                StatusMessage = "Kein Snapshot zum Zurücksetzen vorhanden.";
                return;
            }

            var result = await _rollbackEngine.RollbackSnapshotAsync(latest);
            StatusMessage = result.Success
                ? $"Rollback von \"{latest.Label}\" abgeschlossen."
                : result.ErrorMessage;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanApply() => SelectedPreset is not null && !IsBusy;

    partial void OnSelectedPresetChanged(ProfileDefinition? value) =>
        ApplyPresetCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value) =>
        ApplyPresetCommand.NotifyCanExecuteChanged();
}
