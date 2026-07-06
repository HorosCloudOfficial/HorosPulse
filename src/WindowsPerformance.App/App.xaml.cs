using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Velopack;
using WindowsPerformance.App.Services;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Data;
using WindowsPerformance.Services;
using WindowsPerformance.Services.PowerShell;
using WindowsPerformance.Services.Settings;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.ViewModels;

namespace WindowsPerformance.App;

public partial class App : Application
{
    private IHost? _host;
    private static readonly LoggingLevelSwitch LogLevelSwitch = new(Serilog.Events.LogEventLevel.Debug);

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        VelopackApp.Build()
            .OnFirstRun(_ => { })
            .Run();

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _host = CreateHostBuilder().Build();
        await _host.StartAsync();

        await BootstrapApplicationAsync(_host.Services);

        CheckPowerShellAvailability(_host.Services.GetRequiredService<ILogger<App>>());

        var settingsService = _host.Services.GetRequiredService<IAppSettingsService>();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        RestoreWindowGeometry(mainWindow, settingsService.Current);
        mainWindow.Show();

        var cursorWatch = _host.Services.GetRequiredService<ICursorProcessWatchService>();
        cursorWatch.Start();
    }

    private static void RestoreWindowGeometry(Window window, AppSettings settings)
    {
        window.Width = settings.WindowWidth > 0 ? settings.WindowWidth : 1100;
        window.Height = settings.WindowHeight > 0 ? settings.WindowHeight : 720;

        if (!double.IsNaN(settings.WindowLeft) && !double.IsNaN(settings.WindowTop))
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = settings.WindowLeft;
            window.Top = settings.WindowTop;
        }

        if (Enum.TryParse<WindowState>(settings.WindowState, ignoreCase: true, out var state) &&
            state != WindowState.Minimized)
        {
            window.WindowState = state;
        }
    }

    private static async Task SaveWindowGeometryAsync(MainWindow window, IAppSettingsService settingsService)
    {
        var settings = settingsService.Current;
        if (window.WindowState == WindowState.Normal)
        {
            settings.WindowWidth = window.Width;
            settings.WindowHeight = window.Height;
            settings.WindowLeft = window.Left;
            settings.WindowTop = window.Top;
        }

        settings.WindowState = window.WindowState.ToString();
        await settingsService.SaveAsync();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            var mainWindow = Current.MainWindow as MainWindow;
            if (mainWindow is not null)
            {
                await SaveWindowGeometryAsync(
                    mainWindow,
                    _host.Services.GetRequiredService<IAppSettingsService>());
            }

            _host.Services.GetRequiredService<ICursorProcessWatchService>().Stop();
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .UseSerilog((context, _, config) => config
                .MinimumLevel.ControlledBy(LogLevelSwitch)
                .WriteTo.Debug()
                .WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs", "app-.log"),
                    rollingInterval: RollingInterval.Day))
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(LogLevelSwitch);
                services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
                services.AddWindowsPerformanceData();
                services.AddWindowsPerformanceServices();
                services.AddWindowsPerformanceViewModels();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ILoggingLevelService, LoggingLevelService>();
                services.AddSingleton<ISettingsApplyService, SettingsApplyService>();
                services.AddSingleton<INavigationService, Services.NavigationService>();
                services.AddSingleton<IUserConfirmationService, Services.UserConfirmationService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();
                services.AddSingleton<MainWindow>();
            });

    private static async Task BootstrapApplicationAsync(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IAppSettingsService>();
        await settings.LoadAsync();

        AppSettingsApplicator.Apply(
            settings.Current,
            services.GetRequiredService<PowerShellOptions>(),
            services.GetRequiredService<ILoggingLevelService>(),
            services.GetRequiredService<IThemeService>());

        var snapshotRepository = services.GetRequiredService<ISnapshotRepository>();
        await snapshotRepository.InitializeAsync();

        var auditRepository = services.GetRequiredService<IAuditRepository>();
        await auditRepository.InitializeAsync();

        var profileRepository = services.GetRequiredService<IProfileRepository>();
        await profileRepository.EnsureDefaultPresetsAsync();
    }

    private static void CheckPowerShellAvailability(Microsoft.Extensions.Logging.ILogger logger)
    {
        var pwshPath = FindExecutable("pwsh.exe");
        if (pwshPath is null)
        {
            logger.LogWarning("pwsh.exe nicht gefunden. PowerShell-Features sind bis zur Installation deaktiviert.");
            Debug.WriteLine("[WindowsPerformance] WARN: pwsh.exe not found on PATH.");
        }
        else
        {
            logger.LogInformation("pwsh.exe gefunden: {Path}", pwshPath);
        }
    }

    private static string? FindExecutable(string fileName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv))
            return null;

        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), fileName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "UI thread");
        ShowErrorDialog(e.Exception);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex, "AppDomain");
            ShowErrorDialog(ex);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "Task");
        e.SetObserved();
    }

    private static void LogException(Exception exception, string source)
    {
        Debug.WriteLine($"[WindowsPerformance] {source} exception: {exception}");
        Log.Error(exception, "Unhandled exception on {Source}", source);
    }

    private static void ShowErrorDialog(Exception exception)
    {
        MessageBox.Show(
            $"Ein unerwarteter Fehler ist aufgetreten.\n\n{exception.Message}",
            "WindowsPerformance — Fehler",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
