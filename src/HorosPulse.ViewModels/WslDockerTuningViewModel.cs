namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class WslDockerTuningViewModel : ViewModelBase
{
    private readonly IWslDockerTuningService _wslDockerTuningService;
    private readonly IUserConfirmationService _confirmationService;
    private readonly INavigationService _navigationService;

    public WslDockerTuningViewModel(
        IWslDockerTuningService wslDockerTuningService,
        IUserConfirmationService confirmationService,
        INavigationService navigationService)
    {
        _wslDockerTuningService = wslDockerTuningService;
        _confirmationService = confirmationService;
        _navigationService = navigationService;
        _ = RefreshAsync();
    }

    public string Title => "WSL2 / Docker";

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
    private string _wslStatusSummary = "—";

    [ObservableProperty]
    private string _dockerStatusSummary = "—";

    [ObservableProperty]
    private string _wslConfigPath = "—";

    [ObservableProperty]
    private bool _wslConfigExists;

    [ObservableProperty]
    private string _currentMemoryDisplay = "—";

    [ObservableProperty]
    private string _currentProcessorsDisplay = "—";

    [ObservableProperty]
    private string _currentSwapDisplay = "—";

    [ObservableProperty]
    private string _systemMemoryDisplay = "—";

    [ObservableProperty]
    private string _systemProcessorsDisplay = "—";

    [ObservableProperty]
    private string _recommendedMemoryDisplay = "—";

    [ObservableProperty]
    private string _recommendedProcessorsDisplay = "—";

    [ObservableProperty]
    private string _recommendedSwapDisplay = "—";

    [ObservableProperty]
    private string _buildDefenderHint = "—";

    [ObservableProperty]
    private bool _showBuildDefenderLink;

    public ObservableCollection<WslDockerRecommendationItemViewModel> Recommendations { get; } = new();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var state = await _wslDockerTuningService.GetStateAsync();
            RecommendationSummary = state.RecommendationSummary;
            IsDevSetupOptimal = state.IsDevSetupOptimal;
            HasHorosPulseChanges = state.HasHorosPulseChanges;
            WslStatusSummary = state.WslStatusSummary;
            DockerStatusSummary = state.DockerStatusSummary;
            WslConfigPath = state.WslConfigPath;
            WslConfigExists = state.WslConfigExists;

            CurrentMemoryDisplay = state.CurrentLimits.FormatMemoryMb();
            CurrentProcessorsDisplay = state.CurrentLimits.FormatProcessors();
            CurrentSwapDisplay = state.CurrentLimits.FormatSwapMb();
            SystemMemoryDisplay = state.SystemResources.FormatMemoryMb();
            SystemProcessorsDisplay = state.SystemResources.FormatProcessors();
            RecommendedMemoryDisplay = WslConfigParserDisplay.FormatSizeMb(state.RecommendedLimits.MemoryMb);
            RecommendedProcessorsDisplay = state.RecommendedLimits.Processors.ToString();
            RecommendedSwapDisplay = WslConfigParserDisplay.FormatSizeMb(state.RecommendedLimits.SwapMb);

            ShowBuildDefenderLink = state.IsDockerDesktopInstalled || state.IsWslInstalled;
            BuildDefenderHint = state.BuildToolDefenderDockerExcluded && state.BuildToolDefenderWslExcluded
                ? "Build-Schutz: docker.exe und wsl.exe sind ausgeschlossen."
                : $"Build-Schutz: {state.BuildToolDefenderContainerAppliedCount}/{state.BuildToolDefenderContainerRecommendedCount} Container-Prozesse aktiv — docker.exe/wsl.exe prüfen.";

            Recommendations.Clear();
            foreach (var item in state.Recommendations)
                Recommendations.Add(new WslDockerRecommendationItemViewModel(item));
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
                "WSL2-Limits anwenden",
                "HorosPulse schreibt empfohlene Werte in %USERPROFILE%\\.wslconfig:\n\n" +
                "• memory, processors, swap — abgestimmt auf Ihr System\n" +
                "• localhostForwarding, nestedVirtualization, pageReporting\n\n" +
                "Die bestehende Datei wird gesichert (Backup + Rollback-Tracking).\n" +
                "Nach dem Apply ist wsl --shutdown erforderlich.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _wslDockerTuningService.ApplyRecommendedConfigAsync(userConfirmed: true);
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
                ".wslconfig zurücksetzen",
                "Nur die von HorosPulse geänderte .wslconfig wird wiederhergestellt.\n\n" +
                "Danach wsl --shutdown ausführen.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _wslDockerTuningService.RollbackAsync();
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
    private async Task ShutdownWslAsync()
    {
        if (!_confirmationService.Confirm(
                "WSL herunterfahren",
                "wsl --shutdown beendet alle laufenden WSL-Distributionen und die WSL2-VM.\n\n" +
                "Offene Linux-Sessions und Container werden beendet. " +
                "Erforderlich nach .wslconfig-Änderungen.\n\nFortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _wslDockerTuningService.ShutdownWslAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenBuildDefender() =>
        _navigationService.Navigate(typeof(BuildToolDefenderViewModel), "Build-Schutz");
}

public sealed class WslDockerRecommendationItemViewModel
{
    public WslDockerRecommendationItemViewModel(WslDockerRecommendation recommendation)
    {
        Key = recommendation.Key;
        Title = recommendation.Title;
        Description = recommendation.Description;
        CurrentDisplay = recommendation.CurrentDisplay;
        RecommendedDisplay = recommendation.RecommendedDisplay;
        IsOptimal = recommendation.IsOptimal;
        Severity = recommendation.Severity;
    }

    public string Key { get; }
    public string Title { get; }
    public string Description { get; }
    public string CurrentDisplay { get; }
    public string RecommendedDisplay { get; }
    public bool IsOptimal { get; }
    public WslDockerRecommendationSeverity Severity { get; }

    public string StatusText => IsOptimal ? "Optimal" : "Empfohlen";
}

/// <summary>Display-Helfer für ViewModel (ohne Services-Referenz in XAML-Bindings).</summary>
internal static class WslConfigParserDisplay
{
    public static string FormatSizeMb(long megabytes) =>
        megabytes % 1024 == 0
            ? $"{megabytes / 1024} GB"
            : $"{megabytes} MB";
}
