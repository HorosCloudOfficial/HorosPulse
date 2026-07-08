namespace HorosPulse.App.Services;

using System.Windows;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.ViewModels;
using Microsoft.Extensions.DependencyInjection;

public sealed class CompactWindowCoordinator : ICompactWindowCoordinator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppSettingsService _appSettingsService;
    private CompactWindow? _compactWindow;

    public CompactWindowCoordinator(
        IServiceProvider serviceProvider,
        IAppSettingsService appSettingsService)
    {
        _serviceProvider = serviceProvider;
        _appSettingsService = appSettingsService;
    }

    public bool IsCompactWindowVisible => _compactWindow?.IsVisible == true;

    public void ShowCompactWindow()
    {
        _compactWindow ??= _serviceProvider.GetRequiredService<CompactWindow>();
        ReloadCompactSettings();
        RestoreCompactGeometry(_compactWindow, _appSettingsService.Current.CompactWindow);

        if (Application.Current.MainWindow is { } mainWindow)
            _compactWindow.Owner = mainWindow;

        _compactWindow.Show();
        _compactWindow.Activate();
    }

    public void HideCompactWindow()
    {
        if (_compactWindow is null)
            return;

        _ = SaveCompactGeometryAsync(_compactWindow);
        _compactWindow.Hide();
    }

    public void ShowMainWindow()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
            mainWindow.RestoreFromTrayOrShow();
    }

    public void ReloadCompactSettings()
    {
        var viewModel = _serviceProvider.GetRequiredService<CompactWindowViewModel>();
        viewModel.ReloadFromSettings();
    }

    internal async Task SaveCompactGeometryIfOpenAsync()
    {
        if (_compactWindow is { IsVisible: true })
            await SaveCompactGeometryAsync(_compactWindow);
    }

    private const double MinCompactWidth = 330;
    private const double DefaultCompactWidth = 350;
    private const double DefaultCompactHeight = 640;

    private static void RestoreCompactGeometry(Window window, CompactWindowSettings settings)
    {
        var width = settings.WindowWidth > 0 ? settings.WindowWidth : DefaultCompactWidth;
        window.Width = Math.Max(MinCompactWidth, width);
        window.Height = settings.WindowHeight > 0 ? settings.WindowHeight : DefaultCompactHeight;

        if (!double.IsNaN(settings.WindowLeft) && !double.IsNaN(settings.WindowTop))
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = settings.WindowLeft;
            window.Top = settings.WindowTop;
        }
    }

    private async Task SaveCompactGeometryAsync(Window window)
    {
        var settings = _appSettingsService.Current.CompactWindow;
        if (window.WindowState == WindowState.Normal)
        {
            settings.WindowWidth = window.Width;
            settings.WindowHeight = window.Height;
            settings.WindowLeft = window.Left;
            settings.WindowTop = window.Top;
        }

        await _appSettingsService.SaveAsync();
    }
}
