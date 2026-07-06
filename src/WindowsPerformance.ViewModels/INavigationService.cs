namespace WindowsPerformance.ViewModels;

public interface INavigationService
{
    ViewModelBase CurrentViewModel { get; }

    string SelectedNavItem { get; }

    bool CanGoBack { get; }

    event EventHandler? Navigated;

    void Navigate(Type viewModelType, string navItem);

    bool GoBack();
}
