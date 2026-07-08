namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class MemoryViewModel : ViewModelBase
{
    private readonly IMemoryOptimizerService _memoryOptimizer;
    private readonly IAppSettingsService _appSettingsService;

    public MemoryViewModel(IMemoryOptimizerService memoryOptimizer, IAppSettingsService appSettingsService)
    {
        _memoryOptimizer = memoryOptimizer;
        _appSettingsService = appSettingsService;
        LoadPurgeDefaultsFromSettings();
        _ = RefreshMemoryAsync();
    }

    public string Title => "Speicher";

    [ObservableProperty]
    private long _ramBeforeMb;

    [ObservableProperty]
    private long _ramAfterMb;

    [ObservableProperty]
    private long _physicalTotalMb;

    [ObservableProperty]
    private long _physicalAvailableMb;

    [ObservableProperty]
    private long _pageFileTotalMb;

    [ObservableProperty]
    private long _pageFileAvailableMb;

    [ObservableProperty]
    private long _systemReservedTotalMb;

    [ObservableProperty]
    private long _systemReservedAvailableMb;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool? _isStatusSuccess;

    [ObservableProperty]
    private bool _purgeWorkingSet = true;

    [ObservableProperty]
    private bool _purgeSystemFileCache = true;

    [ObservableProperty]
    private bool _purgeModifiedPageList = true;

    [ObservableProperty]
    private bool _purgeStandbyList = true;

    [ObservableProperty]
    private bool _purgeLowPriorityStandby = true;

    [ObservableProperty]
    private bool _purgeRegistryCache;

    [ObservableProperty]
    private bool _purgeCombineMemoryLists = true;

    public string WarningText =>
        "Speicher leeren ist ein temporärer Effekt. Kurzzeitig kann die Performance schlechter werden, bis Windows die Caches wieder aufbaut. Erfordert HorosPulse.Elevation (UAC).";

    public double PhysicalUsedPercent => PhysicalTotalMb > 0
        ? (PhysicalTotalMb - PhysicalAvailableMb) * 100.0 / PhysicalTotalMb
        : 0;

    public double PageFileUsedPercent => PageFileTotalMb > 0
        ? (PageFileTotalMb - PageFileAvailableMb) * 100.0 / PageFileTotalMb
        : 0;

    public double SystemReservedUsedPercent => SystemReservedTotalMb > 0
        ? (SystemReservedTotalMb - SystemReservedAvailableMb) * 100.0 / SystemReservedTotalMb
        : 0;

    public string RamDisplay => PhysicalTotalMb > 0
        ? $"{PhysicalAvailableMb:N0} / {PhysicalTotalMb:N0} MB"
        : "—";

    public string PageFileDisplay => PageFileTotalMb > 0
        ? $"{PageFileAvailableMb:N0} / {PageFileTotalMb:N0} MB"
        : "—";

    public string SystemReservedDisplay => SystemReservedTotalMb > 0
        ? $"{Math.Max(0, SystemReservedTotalMb - SystemReservedAvailableMb):N0} / {SystemReservedTotalMb:N0} MB"
        : "—";

    [RelayCommand]
    private async Task RefreshMemoryAsync()
    {
        var status = await _memoryOptimizer.GetMemoryStatusAsync();
        ApplyMemoryStatus(status);
        RamBeforeMb = status.PhysicalAvailableMb;
        RamAfterMb = RamBeforeMb;
        NotifyMemoryDisplaysChanged();
    }

    [RelayCommand(CanExecute = nameof(CanApplyPurge))]
    private async Task ApplyPurgeAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        IsStatusSuccess = null;
        try
        {
            var options = BuildPurgeOptions();
            RamBeforeMb = await _memoryOptimizer.GetAvailableMemoryMbAsync();
            var result = await _memoryOptimizer.PurgeMemoryAsync(options);
            await Task.Delay(500);
            var status = await _memoryOptimizer.GetMemoryStatusAsync();
            ApplyMemoryStatus(status);
            RamAfterMb = status.PhysicalAvailableMb;
            NotifyMemoryDisplaysChanged();

            if (result.Success)
            {
                StatusMessage = $"Bereinigung abgeschlossen. Verfügbar: {RamBeforeMb:N0} → {RamAfterMb:N0} MB";
                IsStatusSuccess = true;
            }
            else
            {
                StatusMessage = result.ErrorMessage ?? "Bereinigung fehlgeschlagen.";
                IsStatusSuccess = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusSuccess = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void LoadDefaultsFromSettings()
    {
        LoadPurgeDefaultsFromSettings();
        StatusMessage = "Standard-Bereiche aus Einstellungen geladen.";
        IsStatusSuccess = true;
    }

    private void LoadPurgeDefaultsFromSettings()
    {
        var compact = _appSettingsService.Current.CompactWindow;
        PurgeWorkingSet = compact.PurgeWorkingSet;
        PurgeSystemFileCache = compact.PurgeSystemFileCache;
        PurgeModifiedPageList = compact.PurgeModifiedPageList;
        PurgeStandbyList = compact.PurgeStandbyList;
        PurgeLowPriorityStandby = compact.PurgeLowPriorityStandby;
        PurgeRegistryCache = compact.PurgeRegistryCache;
        PurgeCombineMemoryLists = compact.PurgeCombineMemoryLists;
    }

    private MemoryPurgeOptions BuildPurgeOptions() => new()
    {
        PurgeWorkingSet = PurgeWorkingSet,
        PurgeSystemFileCache = PurgeSystemFileCache,
        PurgeModifiedPageList = PurgeModifiedPageList,
        PurgeStandbyList = PurgeStandbyList,
        PurgeLowPriorityStandby = PurgeLowPriorityStandby,
        PurgeRegistryCache = PurgeRegistryCache,
        PurgeCombineMemoryLists = PurgeCombineMemoryLists,
    };

    private void ApplyMemoryStatus(MemoryStatusSnapshot status)
    {
        PhysicalTotalMb = status.PhysicalTotalMb;
        PhysicalAvailableMb = status.PhysicalAvailableMb;
        PageFileTotalMb = status.PageFileTotalMb;
        PageFileAvailableMb = status.PageFileAvailableMb;
        SystemReservedTotalMb = status.SystemReservedTotalMb;
        SystemReservedAvailableMb = status.SystemReservedAvailableMb;
    }

    private void NotifyMemoryDisplaysChanged()
    {
        OnPropertyChanged(nameof(PhysicalUsedPercent));
        OnPropertyChanged(nameof(PageFileUsedPercent));
        OnPropertyChanged(nameof(SystemReservedUsedPercent));
        OnPropertyChanged(nameof(RamDisplay));
        OnPropertyChanged(nameof(PageFileDisplay));
        OnPropertyChanged(nameof(SystemReservedDisplay));
    }

    private bool CanApplyPurge() =>
        !IsBusy && BuildPurgeOptions().GetEnabledAreas().Count > 0;

    partial void OnIsBusyChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeWorkingSetChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeSystemFileCacheChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeModifiedPageListChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeStandbyListChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeLowPriorityStandbyChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeRegistryCacheChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();

    partial void OnPurgeCombineMemoryListsChanged(bool value) => ApplyPurgeCommand.NotifyCanExecuteChanged();
}
