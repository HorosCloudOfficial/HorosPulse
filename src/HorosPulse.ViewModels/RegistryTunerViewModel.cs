namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class RegistryTunerViewModel : ViewModelBase
{
    private readonly IRegistryTunerService _registryTunerService;
    private readonly IUserConfirmationService _confirmationService;

    public RegistryTunerViewModel(
        IRegistryTunerService registryTunerService,
        IUserConfirmationService confirmationService)
    {
        _registryTunerService = registryTunerService;
        _confirmationService = confirmationService;

        foreach (var tweak in _registryTunerService.GetAvailableTweaks())
            Tweaks.Add(tweak);
    }

    public string Title => "Registry Tuner";

    public ObservableCollection<RegistryTweakDefinition> Tweaks { get; } = new();

    [ObservableProperty]
    private RegistryTweakDefinition? _selectedTweak;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand(CanExecute = nameof(CanApplyTweak))]
    private async Task ApplyTweakAsync()
    {
        if (SelectedTweak is null)
            return;

        if (!_confirmationService.Confirm(
                "Registry-Tweak anwenden",
                $"{SelectedTweak.Name}\n\n{SelectedTweak.Description}\n\nRollback ist vor Aktivierung garantiert. Fortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _registryTunerService.ApplyTweakAsync(SelectedTweak.Id, userConfirmed: true);
            StatusMessage = result.Success ? string.Join("; ", result.Changes ?? []) : result.ErrorMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _registryTunerService.RollbackAsync();
            StatusMessage = result.Success ? "Alle Tweaks zurückgesetzt" : result.ErrorMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanApplyTweak() => SelectedTweak is not null && !IsBusy;

    partial void OnSelectedTweakChanged(RegistryTweakDefinition? value) =>
        ApplyTweakCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value) =>
        ApplyTweakCommand.NotifyCanExecuteChanged();
}
