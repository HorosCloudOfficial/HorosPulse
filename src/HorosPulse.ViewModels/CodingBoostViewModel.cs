namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class CodingBoostViewModel : ViewModelBase
{
    private readonly ICodingBoostService _codingBoostService;
    private readonly IUserConfirmationService _confirmationService;

    public CodingBoostViewModel(
        ICodingBoostService codingBoostService,
        IUserConfirmationService confirmationService)
    {
        _codingBoostService = codingBoostService;
        _confirmationService = confirmationService;
        _ = RefreshAsync();
    }

    public string Title => "Coding-Boost";

    [ObservableProperty]
    private string _recommendationSummary = "—";

    [ObservableProperty]
    private bool _isDevSetupOptimal;

    [ObservableProperty]
    private bool _hasHorosPulseChanges;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private CodingBoostSettingItemViewModel? _gameMode;

    [ObservableProperty]
    private CodingBoostSettingItemViewModel? _hags;

    [ObservableProperty]
    private CodingBoostSettingItemViewModel? _windowedOptimization;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var state = await _codingBoostService.GetStateAsync();
            GameMode = new CodingBoostSettingItemViewModel(state.GameMode);
            Hags = new CodingBoostSettingItemViewModel(state.HardwareAcceleratedGpuScheduling);
            WindowedOptimization = new CodingBoostSettingItemViewModel(state.WindowedGameOptimization);
            RecommendationSummary = state.RecommendationSummary;
            IsDevSetupOptimal = state.IsDevSetupOptimal;
            HasHorosPulseChanges = state.HasHorosPulseChanges;
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
    private async Task ApplyAsync()
    {
        if (!_confirmationService.Confirm(
                "Coding-Boost anwenden",
                "HorosPulse aktiviert das empfohlene Dev-Setup für GPU-lastige Workflows:\n\n" +
                "• Game Mode — priorisiert Vordergrund-Apps (IDE, Browser, GPU-Tools)\n" +
                "• HAGS — hardware-beschleunigte GPU-Planung (Neustart empfohlen)\n" +
                "• Fenster-Optimierung (Win11) — Flip-Model für fensterbasierte DirectX-Apps\n\n" +
                "Nur dokumentierte Microsoft-Registry-Werte werden gesetzt. " +
                "HorosPulse merkt sich die vorherigen Werte für Rollback.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _codingBoostService.ApplyDevSetupAsync(userConfirmed: true);
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
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
        if (!_confirmationService.Confirm(
                "Coding-Boost zurücksetzen",
                "Nur die von HorosPulse gesetzten Werte für Game Mode, HAGS und Fenster-Optimierung werden zurückgesetzt.\n\n" +
                "Für HAGS wird ein Neustart empfohlen.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _codingBoostService.RollbackAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class CodingBoostSettingItemViewModel
{
    public CodingBoostSettingItemViewModel(CodingBoostSettingStatus status)
    {
        Name = status.Name;
        Description = status.Description;
        IsEnabled = status.IsEnabled;
        IsRecommended = status.IsRecommended;
        IsSupported = status.IsSupported;
        RequiresReboot = status.RequiresReboot;
        DetailText = status.DetailText ?? string.Empty;
    }

    public string Name { get; }
    public string Description { get; }
    public bool IsEnabled { get; }
    public bool IsRecommended { get; }
    public bool IsSupported { get; }
    public bool RequiresReboot { get; }
    public string DetailText { get; }

    public string StatusText => !IsSupported
        ? "Nicht verfügbar"
        : IsEnabled ? "An" : "Aus";

    public string RecommendationText => !IsSupported
        ? "—"
        : IsRecommended ? (IsEnabled ? "Optimal" : "Empfohlen") : "Optional";
}
