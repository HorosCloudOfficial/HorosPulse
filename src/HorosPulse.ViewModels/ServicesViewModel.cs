namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using System.ServiceProcess;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class ServicesViewModel : ViewModelBase
{
    private readonly IWindowsServiceManager _serviceManager;
    private readonly IUserConfirmationService _confirmationService;
    private readonly IServicesOptInGate _optInGate;

    public ServicesViewModel(
        IWindowsServiceManager serviceManager,
        IUserConfirmationService confirmationService,
        IServicesOptInGate optInGate)
    {
        _serviceManager = serviceManager;
        _confirmationService = confirmationService;
        _optInGate = optInGate;
        _ = LoadServicesAsync();
    }

    public string Title => "Dienste";

    public ObservableCollection<WindowsServiceInfo> Services { get; } = new();

    [ObservableProperty]
    private bool _optInConfirmed;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private WindowsServiceInfo? _selectedService;

    [RelayCommand]
    private async Task LoadServicesAsync()
    {
        IsBusy = true;
        try
        {
            Services.Clear();
            foreach (var service in await _serviceManager.GetServicesAsync())
                Services.Add(service);
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

    [RelayCommand(CanExecute = nameof(CanApplyDevPreset))]
    private async Task ApplyDevPresetAsync()
    {
        if (!_confirmationService.Confirm(
                "Dienste ändern",
                "Das Ändern von Windows-Diensten kann das System destabilisieren. Fortfahren?",
                isWarning: true))
            return;

        _optInGate.IsConfirmed = true;
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _serviceManager.ApplyDevPresetAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await LoadServicesAsync();
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

    [RelayCommand(CanExecute = nameof(CanSetManual))]
    private async Task SetSelectedManualAsync()
    {
        if (SelectedService is null)
            return;

        IsBusy = true;
        try
        {
            var result = await _serviceManager.SetStartupTypeAsync(SelectedService.Name, ServiceStartMode.Manual);
            StatusMessage = result.Success ? result.Changes?.FirstOrDefault() : result.ErrorMessage;
            await LoadServicesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackAsync()
    {
        if (!_confirmationService.Confirm("Rollback", "Service-Starttypen zurücksetzen?", isWarning: true))
            return;

        IsBusy = true;
        try
        {
            var result = await _serviceManager.RollbackStartupTypesAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await LoadServicesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanApplyDevPreset() => OptInConfirmed && !IsBusy;

    private bool CanSetManual() => SelectedService is not null && !IsBusy;

    partial void OnOptInConfirmedChanged(bool value)
    {
        _optInGate.IsConfirmed = value;
        ApplyDevPresetCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        ApplyDevPresetCommand.NotifyCanExecuteChanged();
        SetSelectedManualCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedServiceChanged(WindowsServiceInfo? value) =>
        SetSelectedManualCommand.NotifyCanExecuteChanged();
}
