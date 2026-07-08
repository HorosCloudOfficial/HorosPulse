using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HorosPulse.Core.Interfaces;
using HorosPulse.ViewModels;

namespace HorosPulse.App;

public partial class MainWindow : Window
{
    private bool _isExiting;
    private readonly ICompactWindowCoordinator _compactWindowCoordinator;

    public MainWindow(MainWindowViewModel viewModel, ICompactWindowCoordinator compactWindowCoordinator)
    {
        InitializeComponent();
        DataContext = viewModel;
        _compactWindowCoordinator = compactWindowCoordinator;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.CurrentViewModel))
            PlayContentTransition();
    }

    private void PlayContentTransition()
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100));
        var slideOut = new DoubleAnimation(0, -10, TimeSpan.FromMilliseconds(100));

        fadeOut.Completed += (_, _) =>
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160));
            var slideIn = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            };

            MainContent.BeginAnimation(OpacityProperty, fadeIn);
            ContentTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
        };

        MainContent.BeginAnimation(OpacityProperty, fadeOut);
        ContentTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            return;
        }

        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) =>
        MinimizeToTray();

    private void MaximizeButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        MinimizeToTray();

    private void MinimizeToTray()
    {
        Hide();
        ShowInTaskbar = false;
        TrayIcon.Visibility = Visibility.Visible;
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
        Activate();
        TrayIcon.Visibility = Visibility.Collapsed;
    }

    public void RestoreFromTrayOrShow() => RestoreFromTray();

    private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) =>
        RestoreFromTray();

    private void TrayOpen_Click(object sender, RoutedEventArgs e) =>
        RestoreFromTray();

    private void TrayOpenCompact_Click(object sender, RoutedEventArgs e) =>
        _compactWindowCoordinator.ShowCompactWindow();

    private void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        _isExiting = true;
        Application.Current.Shutdown();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_isExiting)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        MinimizeToTray();
    }
}
