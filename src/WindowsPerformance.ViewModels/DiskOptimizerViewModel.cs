namespace WindowsPerformance.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

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
        try
        {
            var result = await _diskOptimizer.ApplyOptimizationsAsync();
            StatusMessage = result.Success ? string.Join("; ", result.Changes ?? []) : result.ErrorMessage;
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
        try
        {
            var result = await _diskOptimizer.RollbackAsync();
            StatusMessage = result.Success ? "Zurückgesetzt" : result.ErrorMessage;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
