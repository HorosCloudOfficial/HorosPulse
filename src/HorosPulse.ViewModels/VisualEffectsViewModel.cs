namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;

public sealed partial class VisualEffectsViewModel : ViewModelBase
{
    private readonly IVisualEffectsService _visualEffectsService;

    public VisualEffectsViewModel(IVisualEffectsService visualEffectsService)
    {
        _visualEffectsService = visualEffectsService;
        _ = RefreshStateAsync();
    }

    public string Title => "Visuelle Effekte";

    [ObservableProperty]
    private bool _isPerformance = true;

    [ObservableProperty]
    private bool _isBalanced;

    [ObservableProperty]
    private bool _isBestAppearance;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string _currentStateText = "—";

    [RelayCommand]
    private async Task RefreshStateAsync()
    {
        var state = await _visualEffectsService.GetCurrentStateAsync();
        CurrentStateText = state.AnimationsEnabled ? "Animationen aktiv" : "Animationen deaktiviert";
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        var preset = IsBestAppearance
            ? VisualEffectsPreset.BestAppearance
            : IsBalanced
                ? VisualEffectsPreset.Balanced
                : VisualEffectsPreset.Performance;

        IsBusy = true;
        try
        {
            var result = await _visualEffectsService.ApplyPresetAsync(preset);
            StatusMessage = result.Success ? $"Preset {preset} angewendet" : result.ErrorMessage;
            await RefreshStateAsync();
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
            var result = await _visualEffectsService.RollbackAsync();
            StatusMessage = result.Success ? "Zurückgesetzt" : result.ErrorMessage;
            await RefreshStateAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnIsPerformanceChanged(bool value)
    {
        if (value) { IsBalanced = false; IsBestAppearance = false; }
    }

    partial void OnIsBalancedChanged(bool value)
    {
        if (value) { IsPerformance = false; IsBestAppearance = false; }
    }

    partial void OnIsBestAppearanceChanged(bool value)
    {
        if (value) { IsPerformance = false; IsBalanced = false; }
    }
}
