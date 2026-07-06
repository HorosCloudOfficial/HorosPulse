namespace WindowsPerformance.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public MainWindowViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.Navigated += OnNavigated;
    }

    public ViewModelBase CurrentViewModel => _navigation.CurrentViewModel;

    public string SelectedNavItem => _navigation.SelectedNavItem;

    public bool CanGoBack => _navigation.CanGoBack;

    public string AppVersion { get; } = "0.1.0";

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => _navigation.GoBack();

    [RelayCommand]
    private void NavigateToDashboard() =>
        _navigation.Navigate(typeof(DashboardViewModel), "Dashboard");

    [RelayCommand]
    private void NavigateToEnergie() =>
        _navigation.Navigate(typeof(EnergieViewModel), "Energie");

    [RelayCommand]
    private void NavigateToCursor() =>
        _navigation.Navigate(typeof(CursorViewModel), "Cursor");

    [RelayCommand]
    private void NavigateToMonitor() =>
        _navigation.Navigate(typeof(MonitorViewModel), "Monitor");

    [RelayCommand]
    private void NavigateToSnapshots() =>
        _navigation.Navigate(typeof(SnapshotViewModel), "Snapshots");

    [RelayCommand]
    private void NavigateToPresets() =>
        _navigation.Navigate(typeof(PresetsViewModel), "Presets");

    [RelayCommand]
    private void NavigateToEinstellungen() =>
        _navigation.Navigate(typeof(EinstellungenViewModel), "Einstellungen");

    [RelayCommand]
    private void NavigateToServices() =>
        _navigation.Navigate(typeof(ServicesViewModel), "Dienste");

    [RelayCommand]
    private void NavigateToStartup() =>
        _navigation.Navigate(typeof(StartupViewModel), "Startup");

    [RelayCommand]
    private void NavigateToVisualEffects() =>
        _navigation.Navigate(typeof(VisualEffectsViewModel), "Visuell");

    [RelayCommand]
    private void NavigateToMemory() =>
        _navigation.Navigate(typeof(MemoryViewModel), "Speicher");

    [RelayCommand]
    private void NavigateToNetwork() =>
        _navigation.Navigate(typeof(NetworkViewModel), "Netzwerk");

    [RelayCommand]
    private void NavigateToTrends() =>
        _navigation.Navigate(typeof(TrendViewModel), "Trends");

    [RelayCommand]
    private void NavigateToProcessInspector() =>
        _navigation.Navigate(typeof(ProcessInspectorViewModel), "Prozesse");

    [RelayCommand]
    private void NavigateToDisk() =>
        _navigation.Navigate(typeof(DiskOptimizerViewModel), "Festplatte");

    [RelayCommand]
    private void NavigateToTaskScheduler() =>
        _navigation.Navigate(typeof(TaskSchedulerViewModel), "Tasks");

    [RelayCommand]
    private void NavigateToRegistryTuner() =>
        _navigation.Navigate(typeof(RegistryTunerViewModel), "Registry");

    private void OnNavigated(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentViewModel));
        OnPropertyChanged(nameof(SelectedNavItem));
        OnPropertyChanged(nameof(CanGoBack));
        GoBackCommand.NotifyCanExecuteChanged();
    }
}
