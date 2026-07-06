namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class EnergieViewModel : ViewModelBase
{
    private readonly IPowerPlanService _powerPlanService;

    public EnergieViewModel(IPowerPlanService powerPlanService)
    {
        _powerPlanService = powerPlanService;
        _ = LoadPlansAsync();
    }

    public string Title => "Energie";

    [ObservableProperty]
    private string _description = "Energieplan und Leistungsprofile";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private PowerPlanInfo? _selectedPlan;

    public ObservableCollection<PowerPlanInfo> Plans { get; } = new();

    [RelayCommand]
    private async Task LoadPlansAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            Plans.Clear();
            var plans = await _powerPlanService.GetAvailablePlansAsync();
            foreach (var plan in plans)
                Plans.Add(plan);

            SelectedPlan = plans.FirstOrDefault(p => p.IsActive) ?? plans.FirstOrDefault();
            Description = SelectedPlan is not null
                ? $"Aktiver Plan: {SelectedPlan.Name}"
                : "Keine Energiepläne gefunden";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Laden: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanApplyPlan))]
    private async Task ApplyPlanAsync()
    {
        if (SelectedPlan is null)
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _powerPlanService.SetActivePlanAsync(SelectedPlan.Guid);
            StatusMessage = result.Success
                ? $"Energieplan „{SelectedPlan.Name}“ aktiviert."
                : result.ErrorMessage;
            await LoadPlansAsync();
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
    private async Task ApplyHighPerformanceAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _powerPlanService.EnsureHighPerformancePlanAsync();
            StatusMessage = result.Success
                ? "High-Performance-Plan aktiviert."
                : result.ErrorMessage;
            await LoadPlansAsync();
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
    private async Task ApplyUltimatePerformanceAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _powerPlanService.EnsureUltimatePerformancePlanAsync();
            StatusMessage = result.Success
                ? "Ultimate-Performance-Plan aktiviert."
                : result.ErrorMessage;
            await LoadPlansAsync();
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

    private bool CanApplyPlan() => SelectedPlan is not null && !IsBusy;

    partial void OnSelectedPlanChanged(PowerPlanInfo? value) =>
        ApplyPlanCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value) =>
        ApplyPlanCommand.NotifyCanExecuteChanged();
}
