namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class EinstellungenViewModel : ViewModelBase
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IAuditRepository _auditRepository;
    private readonly IStartupRegistrationService _startupRegistration;
    private readonly ISettingsApplyService _settingsApplyService;
    private readonly IVelopackUpdateService _velopackUpdateService;
    private readonly ICompactWindowCoordinator _compactWindowCoordinator;

    public EinstellungenViewModel(
        IAppSettingsService appSettingsService,
        IAuditRepository auditRepository,
        IStartupRegistrationService startupRegistration,
        ISettingsApplyService settingsApplyService,
        IVelopackUpdateService velopackUpdateService,
        ICompactWindowCoordinator compactWindowCoordinator)
    {
        _appSettingsService = appSettingsService;
        _auditRepository = auditRepository;
        _startupRegistration = startupRegistration;
        _settingsApplyService = settingsApplyService;
        _velopackUpdateService = velopackUpdateService;
        _compactWindowCoordinator = compactWindowCoordinator;
        AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.1.0";
        _ = LoadAsync();
    }

    public string Title => "Einstellungen";

    public IReadOnlyList<string> AvailableThemes { get; } = ["Tokyo Night", "Dark", "Light"];

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
    private int _processMonitorRefreshIntervalMs = 5000;

    [ObservableProperty]
    private bool _processMonitorCursorFilterOnly = true;

    [ObservableProperty]
    private bool _restartSearchServiceAfterIndexerChange;

    [ObservableProperty]
    private bool _reapplyCursorPrioritiesOnRestart;

    [ObservableProperty]
    private bool _compactShowRamStats = true;

    [ObservableProperty]
    private bool _compactShowCpuStats = true;

    [ObservableProperty]
    private bool _compactShowDiskStats = true;

    [ObservableProperty]
    private bool _compactShowMemoryCleanAction = true;

    [ObservableProperty]
    private bool _compactShowCursorDevModeAction = true;

    [ObservableProperty]
    private bool _compactShowDiskOptimizeAction;

    [ObservableProperty]
    private bool _compactShowVisualEffectsAction;

    [ObservableProperty]
    private bool _compactOpenOnStartup;

    [ObservableProperty]
    private bool _compactPurgeWorkingSet = true;

    [ObservableProperty]
    private bool _compactPurgeSystemFileCache = true;

    [ObservableProperty]
    private bool _compactPurgeModifiedPageList = true;

    [ObservableProperty]
    private bool _compactPurgeStandbyList = true;

    [ObservableProperty]
    private bool _compactPurgeLowPriorityStandby = true;

    [ObservableProperty]
    private bool _compactPurgeRegistryCache;

    [ObservableProperty]
    private bool _compactPurgeCombineMemoryLists = true;

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
        SelectedTheme = ThemeToDisplay(current.Theme, current.UseLightMode);
        SelectedLogLevel = current.MinimumLogLevel;
        PowerShellTimeoutSeconds = current.PowerShellTimeoutSeconds;
        ProcessMonitorRefreshIntervalMs = current.ProcessMonitorRefreshIntervalMs;
        ProcessMonitorCursorFilterOnly = current.ProcessMonitorCursorFilterOnly;
        RestartSearchServiceAfterIndexerChange = current.RestartSearchServiceAfterIndexerChange;
        ReapplyCursorPrioritiesOnRestart = current.ReapplyCursorPrioritiesOnRestart;
        AutoStartWithWindows = _startupRegistration.IsRegistered;

        var compact = current.CompactWindow;
        CompactShowRamStats = compact.ShowRamStats;
        CompactShowCpuStats = compact.ShowCpuStats;
        CompactShowDiskStats = compact.ShowDiskStats;
        CompactShowMemoryCleanAction = compact.ShowMemoryCleanAction;
        CompactShowCursorDevModeAction = compact.ShowCursorDevModeAction;
        CompactShowDiskOptimizeAction = compact.ShowDiskOptimizeAction;
        CompactShowVisualEffectsAction = compact.ShowVisualEffectsAction;
        CompactOpenOnStartup = compact.OpenOnStartup;
        CompactPurgeWorkingSet = compact.PurgeWorkingSet;
        CompactPurgeSystemFileCache = compact.PurgeSystemFileCache;
        CompactPurgeModifiedPageList = compact.PurgeModifiedPageList;
        CompactPurgeStandbyList = compact.PurgeStandbyList;
        CompactPurgeLowPriorityStandby = compact.PurgeLowPriorityStandby;
        CompactPurgeRegistryCache = compact.PurgeRegistryCache;
        CompactPurgeCombineMemoryLists = compact.PurgeCombineMemoryLists;

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
        current.UseLightMode = current.Theme.Equals(nameof(AppTheme.Light), StringComparison.OrdinalIgnoreCase);
        current.ProcessMonitorRefreshIntervalMs = Math.Clamp(ProcessMonitorRefreshIntervalMs, 1000, 60000);
        current.ProcessMonitorCursorFilterOnly = ProcessMonitorCursorFilterOnly;
        current.RestartSearchServiceAfterIndexerChange = RestartSearchServiceAfterIndexerChange;
        current.ReapplyCursorPrioritiesOnRestart = ReapplyCursorPrioritiesOnRestart;
        current.AutoStartWithWindows = AutoStartWithWindows;

        current.CompactWindow.ShowRamStats = CompactShowRamStats;
        current.CompactWindow.ShowCpuStats = CompactShowCpuStats;
        current.CompactWindow.ShowDiskStats = CompactShowDiskStats;
        current.CompactWindow.ShowMemoryCleanAction = CompactShowMemoryCleanAction;
        current.CompactWindow.ShowCursorDevModeAction = CompactShowCursorDevModeAction;
        current.CompactWindow.ShowDiskOptimizeAction = CompactShowDiskOptimizeAction;
        current.CompactWindow.ShowVisualEffectsAction = CompactShowVisualEffectsAction;
        current.CompactWindow.OpenOnStartup = CompactOpenOnStartup;
        current.CompactWindow.PurgeWorkingSet = CompactPurgeWorkingSet;
        current.CompactWindow.PurgeSystemFileCache = CompactPurgeSystemFileCache;
        current.CompactWindow.PurgeModifiedPageList = CompactPurgeModifiedPageList;
        current.CompactWindow.PurgeStandbyList = CompactPurgeStandbyList;
        current.CompactWindow.PurgeLowPriorityStandby = CompactPurgeLowPriorityStandby;
        current.CompactWindow.PurgeRegistryCache = CompactPurgeRegistryCache;
        current.CompactWindow.PurgeCombineMemoryLists = CompactPurgeCombineMemoryLists;

        await _appSettingsService.SaveAsync();
        _settingsApplyService.ApplyCurrent();
        _compactWindowCoordinator.ReloadCompactSettings();

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
    private async Task CheckForUpdatesAsync()
    {
        StatusMessage = "Suche nach Updates…";
        var result = await _velopackUpdateService.CheckForUpdatesAsync(downloadIfAvailable: true);

        StatusMessage = result switch
        {
            { IsSkipped: true } => "Auto-Update ist nur für Velopack-Installationen verfügbar.",
            { IsUpToDate: true } => "HorosPulse ist auf dem neuesten Stand.",
            { AvailableVersion: not null } => $"Update {result.AvailableVersion} heruntergeladen. App neu starten zum Anwenden.",
            _ => $"Update-Prüfung fehlgeschlagen: {result.ErrorMessage ?? "Unbekannter Fehler"}",
        };
    }

    [RelayCommand]
    private void OpenCompactWindow()
    {
        _compactWindowCoordinator.ShowCompactWindow();
        StatusMessage = "Kompakt-Fenster geöffnet.";
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
            $"HorosPulse-Audit-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        await File.WriteAllTextAsync(path, csv);
        StatusMessage = $"Audit-CSV exportiert: {path}";
    }

    private static string ThemeToDisplay(string theme, bool useLightMode)
    {
        if (useLightMode && !theme.Equals(nameof(AppTheme.Light), StringComparison.OrdinalIgnoreCase))
            return "Light";

        return theme.Equals(nameof(AppTheme.Dark), StringComparison.OrdinalIgnoreCase) ? "Dark"
            : theme.Equals(nameof(AppTheme.Light), StringComparison.OrdinalIgnoreCase) ? "Light"
            : "Tokyo Night";
    }

    private static string DisplayToTheme(string display) =>
        display.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? nameof(AppTheme.Dark)
            : display.StartsWith("Light", StringComparison.OrdinalIgnoreCase)
                ? nameof(AppTheme.Light)
                : nameof(AppTheme.TokyoNight);
}
