namespace WindowsPerformance.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly DashboardViewModel _dashboard;
    private readonly EnergieViewModel _energie;
    private readonly CursorViewModel _cursor;
    private readonly MonitorViewModel _monitor;
    private readonly PresetsViewModel _presets;
    private readonly EinstellungenViewModel _einstellungen;

    public MainWindowViewModel(
        DashboardViewModel dashboard,
        EnergieViewModel energie,
        CursorViewModel cursor,
        MonitorViewModel monitor,
        PresetsViewModel presets,
        EinstellungenViewModel einstellungen)
    {
        _dashboard = dashboard;
        _energie = energie;
        _cursor = cursor;
        _monitor = monitor;
        _presets = presets;
        _einstellungen = einstellungen;
        CurrentViewModel = _dashboard;
        SelectedNavItem = "Dashboard";
    }

    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    [ObservableProperty]
    private string _selectedNavItem = "Dashboard";

    public string AppVersion { get; } = "0.1.0";

    [RelayCommand]
    private void NavigateToDashboard() => Navigate(_dashboard, "Dashboard");

    [RelayCommand]
    private void NavigateToEnergie() => Navigate(_energie, "Energie");

    [RelayCommand]
    private void NavigateToCursor() => Navigate(_cursor, "Cursor");

    [RelayCommand]
    private void NavigateToMonitor() => Navigate(_monitor, "Monitor");

    [RelayCommand]
    private void NavigateToPresets() => Navigate(_presets, "Presets");

    [RelayCommand]
    private void NavigateToEinstellungen() => Navigate(_einstellungen, "Einstellungen");

    private void Navigate(ViewModelBase viewModel, string navItem)
    {
        CurrentViewModel = viewModel;
        SelectedNavItem = navItem;
    }
}
