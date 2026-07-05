using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WindowsPerformance.Data;
using WindowsPerformance.Services;
using WindowsPerformance.Core.Interfaces;

using WindowsPerformance.ViewModels;

namespace WindowsPerformance.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _host = CreateHostBuilder().Build();
        await _host.StartAsync();

        await BootstrapApplicationAsync(_host.Services);

        CheckPowerShellAvailability(_host.Services.GetRequiredService<ILogger<App>>());

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .UseSerilog((context, _, config) => config
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs", "app-.log"),
                    rollingInterval: RollingInterval.Day))
            .ConfigureServices((_, services) =>
            {
                services.AddWindowsPerformanceData();
                services.AddWindowsPerformanceServices();
                services.AddWindowsPerformanceViewModels();
                services.AddSingleton<WindowsPerformance.Core.Interfaces.IUserConfirmationService, Services.UserConfirmationService>();
                services.AddSingleton<MainWindow>();
            });

    private static async Task BootstrapApplicationAsync(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IAppSettingsService>();
        await settings.LoadAsync();

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
