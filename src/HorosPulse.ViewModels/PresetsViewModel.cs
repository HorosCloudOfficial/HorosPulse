namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class PresetStepOptionViewModel : ObservableObject
{
    public PresetStepOptionViewModel(string stepId, string label, bool isSelected = false)
    {
        StepId = stepId;
        Label = label;
        _isSelected = isSelected;
    }

    public string StepId { get; }
    public string Label { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed partial class PresetsViewModel : ViewModelBase
{
    private readonly IPresetOrchestrator _presetOrchestrator;
    private readonly IProfileRepository _profileRepository;
    private readonly IRollbackEngine _rollbackEngine;
    private readonly ISnapshotManager _snapshotManager;
    private readonly IUserConfirmationService _confirmationService;
    private readonly IFilePickerService _filePickerService;
    private CancellationTokenSource? _operationCts;

    public PresetsViewModel(
        IPresetOrchestrator presetOrchestrator,
        IProfileRepository profileRepository,
        IRollbackEngine rollbackEngine,
        ISnapshotManager snapshotManager,
        IUserConfirmationService confirmationService,
        IFilePickerService filePickerService)
    {
        _presetOrchestrator = presetOrchestrator;
        _profileRepository = profileRepository;
        _rollbackEngine = rollbackEngine;
        _snapshotManager = snapshotManager;
        _confirmationService = confirmationService;
        _filePickerService = filePickerService;

        foreach (var (id, label) in PresetStepIds.AllSelectable)
            AvailableSteps.Add(new PresetStepOptionViewModel(id, label, id == PresetStepIds.Snapshot));

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

    [ObservableProperty]
    private string? _progressMessage;

    [ObservableProperty]
    private string _newPresetName = string.Empty;

    [ObservableProperty]
    private string _newPresetDescription = string.Empty;

    public ObservableCollection<ProfileDefinition> Presets { get; } = new();
    public ObservableCollection<PresetStepResult> ApplySteps { get; } = new();
    public ObservableCollection<PresetStepOptionViewModel> AvailableSteps { get; } = new();

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

    [RelayCommand(CanExecute = nameof(CanSavePreset))]
    private async Task SavePresetAsync()
    {
        var selectedSteps = AvailableSteps
            .Where(step => step.IsSelected)
            .Select(step => step.StepId)
            .ToList();

        if (selectedSteps.Count == 0)
        {
            StatusMessage = "Mindestens einen Schritt auswählen.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var profile = new ProfileDefinition
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = NewPresetName.Trim(),
                Description = string.IsNullOrWhiteSpace(NewPresetDescription)
                    ? "Benutzerdefiniertes Preset"
                    : NewPresetDescription.Trim(),
                IsBuiltIn = false,
                Steps = selectedSteps,
            };

            await _profileRepository.SaveAsync(profile);
            await LoadPresetsAsync();
            SelectedPreset = Presets.FirstOrDefault(p => p.Id == profile.Id);
            NewPresetName = string.Empty;
            NewPresetDescription = string.Empty;
            StatusMessage = $"Preset \"{profile.Name}\" gespeichert.";
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

    [RelayCommand(CanExecute = nameof(CanDeletePreset))]
    private async Task DeletePresetAsync()
    {
        if (SelectedPreset is null || SelectedPreset.IsBuiltIn)
            return;

        if (!_confirmationService.Confirm(
                "Preset löschen",
                $"Preset \"{SelectedPreset.Name}\" wirklich löschen?"))
            return;

        IsBusy = true;
        try
        {
            var name = SelectedPreset.Name;
            await _profileRepository.DeleteAsync(SelectedPreset.Id);
            SelectedPreset = null;
            await LoadPresetsAsync();
            StatusMessage = $"Preset \"{name}\" gelöscht.";
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

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ExportPresetAsync()
    {
        if (SelectedPreset is null)
            return;

        var path = await _filePickerService.PickSaveFileAsync("JSON Preset|*.json", $"{SelectedPreset.Name}.json");
        if (path is null)
            return;

        var json = System.Text.Json.JsonSerializer.Serialize(SelectedPreset, JsonDefaults.Options);
        await File.WriteAllTextAsync(path, json);
        StatusMessage = $"Preset exportiert: {path}";
    }

    [RelayCommand]
    private async Task ImportPresetAsync()
    {
        var path = await _filePickerService.PickOpenFileAsync("JSON Preset|*.json");
        if (path is null)
            return;

        IsBusy = true;
        try
        {
            var json = await File.ReadAllTextAsync(path);
            var profile = System.Text.Json.JsonSerializer.Deserialize<ProfileDefinition>(json, JsonDefaults.Options);
            if (profile is null || string.IsNullOrWhiteSpace(profile.Name))
            {
                StatusMessage = "Ungültige Preset-Datei.";
                return;
            }

            var imported = new ProfileDefinition
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = profile.Name,
                Description = string.IsNullOrWhiteSpace(profile.Description) ? "Importiert" : profile.Description,
                IsBuiltIn = false,
                Steps = profile.Steps,
            };

            await _profileRepository.SaveAsync(imported);
            await LoadPresetsAsync();
            SelectedPreset = Presets.FirstOrDefault(p => p.Id == imported.Id);
            StatusMessage = $"Preset \"{imported.Name}\" importiert.";
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

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyPresetAsync()
    {
        if (SelectedPreset is null)
            return;

        IsBusy = true;
        StatusMessage = null;
        ProgressMessage = $"Preset \"{SelectedPreset.Name}\" wird angewendet…";
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
            ProgressMessage = null;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackLatestAsync()
    {
        var snapshots = await _snapshotManager.GetSnapshotsAsync();
        var latest = snapshots.FirstOrDefault();
        if (latest is null)
        {
            StatusMessage = "Kein Snapshot zum Zurücksetzen vorhanden.";
            return;
        }

        if (!RollbackUiHelper.ConfirmRollback(_confirmationService, latest))
            return;

        _operationCts?.Cancel();
        _operationCts = new CancellationTokenSource();
        var progress = new Progress<string>(message => ProgressMessage = message);

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _rollbackEngine.RollbackSnapshotAsync(latest, progress, _operationCts.Token);
            StatusMessage = result.Success
                ? $"Rollback von \"{latest.Label}\" abgeschlossen."
                : result.ErrorMessage;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Rollback abgebrochen.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            ProgressMessage = null;
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelOperation))]
    private void CancelOperation() => _operationCts?.Cancel();

    private bool CanCancelOperation() => IsBusy;

    private bool CanApply() => SelectedPreset is not null && !IsBusy;

    private bool CanSavePreset() =>
        !IsBusy && !string.IsNullOrWhiteSpace(NewPresetName);

    private bool CanDeletePreset() =>
        SelectedPreset is not null && !SelectedPreset.IsBuiltIn && !IsBusy;

    partial void OnSelectedPresetChanged(ProfileDefinition? value)
    {
        ApplyPresetCommand.NotifyCanExecuteChanged();
        DeletePresetCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        ApplyPresetCommand.NotifyCanExecuteChanged();
        CancelOperationCommand.NotifyCanExecuteChanged();
        SavePresetCommand.NotifyCanExecuteChanged();
        DeletePresetCommand.NotifyCanExecuteChanged();
    }

    partial void OnNewPresetNameChanged(string value) =>
        SavePresetCommand.NotifyCanExecuteChanged();
}
