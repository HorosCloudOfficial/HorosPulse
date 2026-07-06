namespace HorosPulse.App.Services;

using Microsoft.Extensions.DependencyInjection;
using HorosPulse.ViewModels;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<(Type ViewModelType, string NavItem)> _history = new();

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        CurrentViewModel = serviceProvider.GetRequiredService<DashboardViewModel>();
        SelectedNavItem = "Dashboard";
    }

    public ViewModelBase CurrentViewModel { get; private set; }

    public string SelectedNavItem { get; private set; }

    public bool CanGoBack => _history.Count > 0;

    public event EventHandler? Navigated;

    public void Navigate(Type viewModelType, string navItem)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        if (!typeof(ViewModelBase).IsAssignableFrom(viewModelType))
            throw new ArgumentException($"Typ muss von {nameof(ViewModelBase)} erben.", nameof(viewModelType));

        if (CurrentViewModel.GetType() == viewModelType && SelectedNavItem == navItem)
            return;

        _history.Push((CurrentViewModel.GetType(), SelectedNavItem));

        CurrentViewModel = (ViewModelBase)_serviceProvider.GetRequiredService(viewModelType);
        SelectedNavItem = navItem;
        Navigated?.Invoke(this, EventArgs.Empty);
    }

    public bool GoBack()
    {
        if (_history.Count == 0)
            return false;

        var (viewModelType, navItem) = _history.Pop();
        CurrentViewModel = (ViewModelBase)_serviceProvider.GetRequiredService(viewModelType);
        SelectedNavItem = navItem;
        Navigated?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
