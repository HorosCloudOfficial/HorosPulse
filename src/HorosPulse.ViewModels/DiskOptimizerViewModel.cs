namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class DiskOptimizerViewModel : ViewModelBase
{
    private readonly IDiskOptimizerService _diskOptimizer;

    public DiskOptimizerViewModel(IDiskOptimizerService diskOptimizer)
    {
        _diskOptimizer = diskOptimizer;
        _ = RefreshAsync();
    }

    public string Title => "Festplatte";

    [ObservableProperty]
    private string _statusText = "—";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool? _isStatusSuccess;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var state = await _diskOptimizer.GetCurrentStateAsync();
        StatusText =
            $"Prefetch: {(state.PrefetchEnabled == true ? "An" : state.PrefetchEnabled == false ? "Aus" : "—")}, " +
            $"Superfetch: {(state.SuperfetchEnabled == true ? "An" : state.SuperfetchEnabled == false ? "Aus" : "—")}, " +
            $"TRIM: {(state.TrimEnabled == true ? "Aktiv" : state.TrimEnabled == false ? "Deaktiviert" : "—")}, " +
            $"Defrag-Status: {state.DefragStatus ?? "—"}";
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        IsStatusSuccess = null;
        try
        {
            var result = await _diskOptimizer.ApplyOptimizationsAsync();
            if (result.Success)
            {
                StatusMessage = "Festplatten-Optimierung angewendet.";
                IsStatusSuccess = true;
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                IsStatusSuccess = false;
            }

            await RefreshAsync();
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
        StatusMessage = null;
        IsStatusSuccess = null;
        try
        {
            var result = await _diskOptimizer.RollbackAsync();
            if (result.Success)
            {
                StatusMessage = "Festplatten-Einstellungen zurückgesetzt.";
                IsStatusSuccess = true;
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                IsStatusSuccess = false;
            }

            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
