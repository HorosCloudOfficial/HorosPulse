namespace WindowsPerformance.ViewModels;

using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed partial class EinstellungenViewModel : ViewModelBase
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IAuditRepository _auditRepository;

    public EinstellungenViewModel(
        IAppSettingsService appSettingsService,
        IAuditRepository auditRepository)
    {
        _appSettingsService = appSettingsService;
        _auditRepository = auditRepository;
        AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.1.0";
        _ = LoadAsync();
    }

    public string Title => "Einstellungen";

    [ObservableProperty]
    private bool _defenderOptIn;

    [ObservableProperty]
    private string _defaultDevFoldersText = "node_modules, .git, dist, build";

    [ObservableProperty]
    private int _snapshotRetentionLimit = 50;

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
        DefenderOptIn = _appSettingsService.Current.DefenderOptIn;
        SnapshotRetentionLimit = _appSettingsService.Current.SnapshotRetentionLimit;
        DefaultDevFoldersText = string.Join(", ", _appSettingsService.Current.DefaultDevFolders);

        await LoadAuditLogAsync();
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _appSettingsService.Current.DefenderOptIn = DefenderOptIn;
        _appSettingsService.Current.SnapshotRetentionLimit = SnapshotRetentionLimit;
        _appSettingsService.Current.DefaultDevFolders = DefaultDevFoldersText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        await _appSettingsService.SaveAsync();
        StatusMessage = "Einstellungen gespeichert.";
    }

    [RelayCommand]
    private async Task LoadAuditLogAsync()
    {
        AuditEntries.Clear();
        var entries = await _auditRepository.GetRecentAsync(200);
        foreach (var entry in entries)
            AuditEntries.Add(entry);
    }
}
