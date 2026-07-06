namespace WindowsPerformance.ViewModels;

using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed partial class EinstellungenViewModel : ViewModelBase
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IAuditRepository _auditRepository;
    private readonly IStartupRegistrationService _startupRegistration;
    private readonly ISettingsApplyService _settingsApplyService;

    public EinstellungenViewModel(
        IAppSettingsService appSettingsService,
        IAuditRepository auditRepository,
        IStartupRegistrationService startupRegistration,
        ISettingsApplyService settingsApplyService)
    {
        _appSettingsService = appSettingsService;
        _auditRepository = auditRepository;
        _startupRegistration = startupRegistration;
        _settingsApplyService = settingsApplyService;
        AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.1.0";
        _ = LoadAsync();
    }

    public string Title => "Einstellungen";

    public IReadOnlyList<string> AvailableThemes { get; } = ["Tokyo Night", "Dark", "Light (Skeleton)"];

    public IReadOnlyList<string> AvailableLogLevels { get; } =
        ["Verbose", "Debug", "Information", "Warning", "Error"];

    [ObservableProperty]
    private bool _defenderOptIn;

    [ObservableProperty]
    private string _defaultDevFoldersText = "node_modules, .git, dist, build";

    [ObservableProperty]
    private int _snapshotRetentionLimit = 50;

    [ObservableProperty]
    private string _selectedTheme = "Tokyo Night";

    [ObservableProperty]
    private string _selectedLogLevel = "Information";

    [ObservableProperty]
    private bool _autoStartWithWindows;

    [ObservableProperty]
    private int _powerShellTimeoutSeconds = 30;

    [ObservableProperty]
    private bool _useLightMode;

    [ObservableProperty]
    private int _processMonitorRefreshIntervalMs = 5000;

    [ObservableProperty]
    private bool _processMonitorCursorFilterOnly = true;

    [ObservableProperty]
    private bool _restartSearchServiceAfterIndexerChange;

    [ObservableProperty]
    private bool _reapplyCursorPrioritiesOnRestart;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _selectedTabIndex;

    public string AppVersion { get; }

    public ObservableCollection<AuditEntry> AuditEntries { get; } = new();

    [RelayCommand]
    private async Task LoadAsync()
    {
        await _appSettingsService.LoadAsync();
        var current = _appSettingsService.Current;

        DefenderOptIn = current.DefenderOptIn;
        SnapshotRetentionLimit = current.SnapshotRetentionLimit;
        DefaultDevFoldersText = string.Join(", ", current.DefaultDevFolders);
        SelectedTheme = ThemeToDisplay(current.Theme);
        SelectedLogLevel = current.MinimumLogLevel;
        PowerShellTimeoutSeconds = current.PowerShellTimeoutSeconds;
        UseLightMode = current.UseLightMode;
        ProcessMonitorRefreshIntervalMs = current.ProcessMonitorRefreshIntervalMs;
        ProcessMonitorCursorFilterOnly = current.ProcessMonitorCursorFilterOnly;
        RestartSearchServiceAfterIndexerChange = current.RestartSearchServiceAfterIndexerChange;
        ReapplyCursorPrioritiesOnRestart = current.ReapplyCursorPrioritiesOnRestart;
        AutoStartWithWindows = _startupRegistration.IsRegistered;

        await LoadAuditLogAsync();
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var current = _appSettingsService.Current;
        current.DefenderOptIn = DefenderOptIn;
        current.SnapshotRetentionLimit = SnapshotRetentionLimit;
        current.DefaultDevFolders = DefaultDevFoldersText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        current.Theme = DisplayToTheme(SelectedTheme);
        current.MinimumLogLevel = SelectedLogLevel;
        current.PowerShellTimeoutSeconds = Math.Clamp(PowerShellTimeoutSeconds, 5, 600);
        current.UseLightMode = UseLightMode;
        current.ProcessMonitorRefreshIntervalMs = Math.Clamp(ProcessMonitorRefreshIntervalMs, 1000, 60000);
        current.ProcessMonitorCursorFilterOnly = ProcessMonitorCursorFilterOnly;
        current.RestartSearchServiceAfterIndexerChange = RestartSearchServiceAfterIndexerChange;
        current.ReapplyCursorPrioritiesOnRestart = ReapplyCursorPrioritiesOnRestart;
        current.AutoStartWithWindows = AutoStartWithWindows;

        if (UseLightMode)
            current.Theme = nameof(AppTheme.Light);

        await _appSettingsService.SaveAsync();
        _settingsApplyService.ApplyCurrent();

        if (AutoStartWithWindows)
        {
            if (!_startupRegistration.Register())
                StatusMessage = "Einstellungen gespeichert, Autostart konnte nicht registriert werden.";
            else
                StatusMessage = "Einstellungen gespeichert.";
        }
        else
        {
            _startupRegistration.Unregister();
            StatusMessage = "Einstellungen gespeichert.";
        }
    }

    [RelayCommand]
    private async Task LoadAuditLogAsync()
    {
        AuditEntries.Clear();
        var entries = await _auditRepository.GetRecentAsync(200);
        foreach (var entry in entries)
            AuditEntries.Add(entry);
    }

    [RelayCommand]
    private async Task ExportAuditCsvAsync()
    {
        var csv = await _auditRepository.ExportToCsvAsync();
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"WindowsPerformance-Audit-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        await File.WriteAllTextAsync(path, csv);
        StatusMessage = $"Audit-CSV exportiert: {path}";
    }

    private static string ThemeToDisplay(string theme) =>
        theme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? "Dark"
            : theme.Equals("Light", StringComparison.OrdinalIgnoreCase) ? "Light (Skeleton)"
            : "Tokyo Night";

    private static string DisplayToTheme(string display) =>
        display.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? nameof(AppTheme.Dark)
            : display.StartsWith("Light", StringComparison.OrdinalIgnoreCase)
                ? nameof(AppTheme.Light)
                : nameof(AppTheme.TokyoNight);
}
