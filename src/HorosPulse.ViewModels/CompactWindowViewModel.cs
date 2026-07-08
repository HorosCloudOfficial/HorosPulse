namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class CompactWindowViewModel : ViewModelBase, IDisposable
{
    private const double TiredThresholdPercent = 70;
    private const double SickThresholdPercent = 85;

    private readonly IMetricsCollector _metricsCollector;
    private readonly IMemoryOptimizerService _memoryOptimizer;
    private readonly IPresetOrchestrator _presetOrchestrator;
    private readonly IDiskOptimizerService _diskOptimizer;
    private readonly IVisualEffectsService _visualEffectsService;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ICompactWindowCoordinator _coordinator;
    private DateTimeOffset _lastMemoryStatusRefreshUtc = DateTimeOffset.MinValue;

    public CompactWindowViewModel(
        IMetricsCollector metricsCollector,
        IMemoryOptimizerService memoryOptimizer,
        IPresetOrchestrator presetOrchestrator,
        IDiskOptimizerService diskOptimizer,
        IVisualEffectsService visualEffectsService,
        IAppSettingsService appSettingsService,
        ICompactWindowCoordinator coordinator)
    {
        _metricsCollector = metricsCollector;
        _memoryOptimizer = memoryOptimizer;
        _presetOrchestrator = presetOrchestrator;
        _diskOptimizer = diskOptimizer;
        _visualEffectsService = visualEffectsService;
        _appSettingsService = appSettingsService;
        _coordinator = coordinator;

        _metricsCollector.MetricUpdated += OnMetricUpdated;
        _ = RefreshMetricsAsync();
        ReloadFromSettings();
    }

    public string Title => "HorosPulse Digi-Pet";

    [ObservableProperty]
    private double _cpuPercent;

    [ObservableProperty]
    private long _ramUsedMb;

    [ObservableProperty]
    private long _ramTotalMb;

    [ObservableProperty]
    private long _ramAvailableMb;

    [ObservableProperty]
    private double _diskPercent;

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
    private bool _showRamStats = true;

    [ObservableProperty]
    private bool _showCpuStats = true;

    [ObservableProperty]
    private bool _showDiskStats = true;

    [ObservableProperty]
    private bool _showMemoryCleanAction = true;

    [ObservableProperty]
    private bool _showCursorDevModeAction = true;

    [ObservableProperty]
    private bool _showDiskOptimizeAction;

    [ObservableProperty]
    private bool _showVisualEffectsAction;

    [ObservableProperty]
    private bool _showPurgeOptions;

    [ObservableProperty]
    private bool _isHealing;

    [ObservableProperty]
    private int _statusAnimationTick;

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

    public double RamPercent => RamTotalMb > 0
        ? RamUsedMb * 100.0 / RamTotalMb
        : 0;

    public double PhysicalUsedPercent => PhysicalTotalMb > 0
        ? (PhysicalTotalMb - PhysicalAvailableMb) * 100.0 / PhysicalTotalMb
        : 0;

    public double PageFileUsedPercent => PageFileTotalMb > 0
        ? (PageFileTotalMb - PageFileAvailableMb) * 100.0 / PageFileTotalMb
        : 0;

    public double SystemReservedUsedPercent => SystemReservedTotalMb > 0
        ? (SystemReservedTotalMb - SystemReservedAvailableMb) * 100.0 / SystemReservedTotalMb
        : 0;

    public long SystemReservedUsedMb => Math.Max(0, SystemReservedTotalMb - SystemReservedAvailableMb);

    public string RamDisplay => PhysicalTotalMb > 0
        ? $"{PhysicalAvailableMb:N0} / {PhysicalTotalMb:N0} MB"
        : "—";

    public string PageFileDisplay => PageFileTotalMb > 0
        ? $"{PageFileAvailableMb:N0} / {PageFileTotalMb:N0} MB"
        : "—";

    public string SystemReservedDisplay => SystemReservedTotalMb > 0
        ? $"{SystemReservedUsedMb:N0} / {SystemReservedTotalMb:N0} MB"
        : "—";

    public string RamPercentDisplay => PhysicalTotalMb > 0
        ? $"{PhysicalUsedPercent:F0}%"
        : "—";

    public PetHealthState PetState
    {
        get
        {
            if (RamPercent >= SickThresholdPercent || CpuPercent >= SickThresholdPercent)
                return PetHealthState.Sick;

            if (RamPercent >= TiredThresholdPercent || CpuPercent >= TiredThresholdPercent)
                return PetHealthState.Tired;

            return PetHealthState.Healthy;
        }
    }

    public string PetEmoji => PetState switch
    {
        PetHealthState.Sick => "🤒",
        PetHealthState.Tired => "😴",
        _ => "⚡",
    };

    public string PetMoodText => PetState switch
    {
        PetHealthState.Sick => "KRANK! PC überlastet!",
        PetHealthState.Tired => "Müde… braucht Pause",
        _ => "Voller Energie!",
    };

    public string PetSpeciesName => PetState switch
    {
        PetHealthState.Sick => "Virusmon",
        PetHealthState.Tired => "Slothmon",
        _ => "Pulsemon",
    };

    public bool IsPetHealthy => PetState == PetHealthState.Healthy;

    public bool IsPetTired => PetState == PetHealthState.Tired;

    public bool IsPetSick => PetState == PetHealthState.Sick;

    [RelayCommand]
    private void OpenMainWindow() => _coordinator.ShowMainWindow();

    [RelayCommand]
    private void TogglePurgeOptions() => ShowPurgeOptions = !ShowPurgeOptions;

    [RelayCommand]
    private async Task RefreshMetricsAsync()
    {
        var metric = await Task.Run(async () =>
                await _metricsCollector.GetCurrentMetricAsync().ConfigureAwait(false))
            .ConfigureAwait(true);
        ApplyMetric(metric);

        var memoryStatus = await Task.Run(async () =>
                await _memoryOptimizer.GetMemoryStatusAsync().ConfigureAwait(false))
            .ConfigureAwait(true);
        ApplyMemoryStatus(memoryStatus);
        NotifyPetStateChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteActions))]
    private async Task CleanMemoryAsync()
    {
        IsBusy = true;
        ClearStatus();
        var showHealing = false;
        try
        {
            var options = BuildPurgeOptions();
            var before = await Task.Run(async () =>
                    await _memoryOptimizer.GetAvailableMemoryMbAsync().ConfigureAwait(false))
                .ConfigureAwait(true);

            var purgeResult = await Task.Run(async () =>
                    await _memoryOptimizer.PurgeMemoryAsync(options).ConfigureAwait(false))
                .ConfigureAwait(true);

            if (purgeResult.Success)
            {
                showHealing = true;
                IsHealing = true;
                await Task.Delay(600).ConfigureAwait(true);
            }

            var status = await Task.Run(async () =>
                    await _memoryOptimizer.GetMemoryStatusAsync().ConfigureAwait(false))
                .ConfigureAwait(true);
            ApplyMemoryStatus(status);
            RamAvailableMb = status.PhysicalAvailableMb;
            NotifyPetStateChanged();

            SetStatus(
                purgeResult.Success
                    ? $"Gefüttert! {before:N0} → {RamAvailableMb:N0} MB frei"
                    : purgeResult.ErrorMessage ?? "Bereinigung fehlgeschlagen.",
                purgeResult.Success);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, false);
        }
        finally
        {
            IsBusy = false;
            if (showHealing)
            {
                await Task.Delay(800).ConfigureAwait(true);
                IsHealing = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteActions))]
    private async Task ApplyCursorDevModeAsync()
    {
        IsBusy = true;
        ClearStatus();
        try
        {
            var applyResult = await Task.Run(async () =>
                    await _presetOrchestrator.ApplyPresetAsync(PresetIds.CursorDevMode).ConfigureAwait(false))
                .ConfigureAwait(true);
            SetStatus(
                applyResult.Success
                    ? "Cursor Dev Mode — Skill aktiviert!"
                    : FormatPresetFailureMessage(applyResult),
                applyResult.Success);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteActions))]
    private async Task OptimizeDiskAsync()
    {
        IsBusy = true;
        ClearStatus();
        try
        {
            var optimizeResult = await Task.Run(async () =>
                    await _diskOptimizer.ApplyOptimizationsAsync().ConfigureAwait(false))
                .ConfigureAwait(true);
            SetStatus(
                optimizeResult.Success
                    ? "Festplatte optimiert — Pet läuft schneller!"
                    : optimizeResult.ErrorMessage ?? "Festplatten-Optimierung fehlgeschlagen.",
                optimizeResult.Success);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteActions))]
    private async Task ApplyVisualPerformanceAsync()
    {
        IsBusy = true;
        ClearStatus();
        try
        {
            var fxResult = await Task.Run(async () =>
                    await _visualEffectsService.ApplyPresetAsync(VisualEffectsPreset.Performance).ConfigureAwait(false))
                .ConfigureAwait(true);
            SetStatus(
                fxResult.Success
                    ? "Visuelle Effekte: Performance!"
                    : fxResult.ErrorMessage ?? "Visuelle Effekte fehlgeschlagen.",
                fxResult.Success);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, false);
        }
        finally
        {
            IsBusy = false;
        }
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

    private static string FormatPresetFailureMessage(PresetApplyResult result)
    {
        var failedStep = result.Steps.FirstOrDefault(step => !step.Success);
        if (failedStep is not null && !string.IsNullOrWhiteSpace(failedStep.Message))
            return failedStep.Message;

        if (failedStep is not null)
            return $"{failedStep.StepName} fehlgeschlagen.";

        return result.ErrorMessage ?? "Cursor Dev Mode fehlgeschlagen.";
    }

    private void ClearStatus()
    {
        StatusMessage = null;
        IsStatusSuccess = null;
    }

    private void SetStatus(string message, bool isSuccess)
    {
        StatusMessage = message;
        IsStatusSuccess = isSuccess;
        StatusAnimationTick++;
    }

    private bool CanExecuteActions() => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        CleanMemoryCommand.NotifyCanExecuteChanged();
        ApplyCursorDevModeCommand.NotifyCanExecuteChanged();
        OptimizeDiskCommand.NotifyCanExecuteChanged();
        ApplyVisualPerformanceCommand.NotifyCanExecuteChanged();
    }

    partial void OnCpuPercentChanged(double value) => NotifyPetStateChanged();

    partial void OnRamUsedMbChanged(long value) => NotifyPetStateChanged();

    partial void OnRamTotalMbChanged(long value) => NotifyPetStateChanged();

    public void ReloadFromSettings()
    {
        var settings = _appSettingsService.Current.CompactWindow;
        ShowRamStats = settings.ShowRamStats;
        ShowCpuStats = settings.ShowCpuStats;
        ShowDiskStats = settings.ShowDiskStats;
        ShowMemoryCleanAction = settings.ShowMemoryCleanAction;
        ShowCursorDevModeAction = settings.ShowCursorDevModeAction;
        ShowDiskOptimizeAction = settings.ShowDiskOptimizeAction;
        ShowVisualEffectsAction = settings.ShowVisualEffectsAction;

        PurgeWorkingSet = settings.PurgeWorkingSet;
        PurgeSystemFileCache = settings.PurgeSystemFileCache;
        PurgeModifiedPageList = settings.PurgeModifiedPageList;
        PurgeStandbyList = settings.PurgeStandbyList;
        PurgeLowPriorityStandby = settings.PurgeLowPriorityStandby;
        PurgeRegistryCache = settings.PurgeRegistryCache;
        PurgeCombineMemoryLists = settings.PurgeCombineMemoryLists;
    }

    private void OnMetricUpdated(object? sender, PerformanceMetric metric)
    {
        ApplyMetric(metric);

        var now = DateTimeOffset.UtcNow;
        if (now - _lastMemoryStatusRefreshUtc < TimeSpan.FromSeconds(3))
            return;

        _lastMemoryStatusRefreshUtc = now;
        _ = RefreshMemoryStatusAsync();
    }

    private async Task RefreshMemoryStatusAsync()
    {
        try
        {
            var memoryStatus = await Task.Run(async () =>
                    await _memoryOptimizer.GetMemoryStatusAsync().ConfigureAwait(false))
                .ConfigureAwait(true);
            ApplyMemoryStatus(memoryStatus);
            NotifyMemoryDisplaysChanged();
        }
        catch
        {
            // Metrics polling must not break the compact window.
        }
    }

    private void ApplyMetric(PerformanceMetric metric)
    {
        CpuPercent = metric.CpuPercent;
        RamUsedMb = metric.RamUsedMb;
        RamTotalMb = metric.RamTotalMb;
        RamAvailableMb = Math.Max(0, metric.RamTotalMb - metric.RamUsedMb);
        DiskPercent = metric.DiskActivePercent;
        NotifyMemoryDisplaysChanged();
        NotifyPetStateChanged();
    }

    private void ApplyMemoryStatus(MemoryStatusSnapshot status)
    {
        PhysicalTotalMb = status.PhysicalTotalMb;
        PhysicalAvailableMb = status.PhysicalAvailableMb;
        PageFileTotalMb = status.PageFileTotalMb;
        PageFileAvailableMb = status.PageFileAvailableMb;
        SystemReservedTotalMb = status.SystemReservedTotalMb;
        SystemReservedAvailableMb = status.SystemReservedAvailableMb;
        NotifyMemoryDisplaysChanged();
    }

    private void NotifyMemoryDisplaysChanged()
    {
        OnPropertyChanged(nameof(RamPercent));
        OnPropertyChanged(nameof(PhysicalUsedPercent));
        OnPropertyChanged(nameof(PageFileUsedPercent));
        OnPropertyChanged(nameof(SystemReservedUsedPercent));
        OnPropertyChanged(nameof(SystemReservedUsedMb));
        OnPropertyChanged(nameof(RamDisplay));
        OnPropertyChanged(nameof(PageFileDisplay));
        OnPropertyChanged(nameof(SystemReservedDisplay));
        OnPropertyChanged(nameof(RamPercentDisplay));
    }

    private void NotifyPetStateChanged()
    {
        OnPropertyChanged(nameof(PetState));
        OnPropertyChanged(nameof(PetEmoji));
        OnPropertyChanged(nameof(PetMoodText));
        OnPropertyChanged(nameof(PetSpeciesName));
        OnPropertyChanged(nameof(IsPetHealthy));
        OnPropertyChanged(nameof(IsPetTired));
        OnPropertyChanged(nameof(IsPetSick));
    }

    public void Dispose()
    {
        _metricsCollector.MetricUpdated -= OnMetricUpdated;
    }
}
