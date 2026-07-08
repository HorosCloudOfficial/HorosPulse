namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using HorosPulse.Core;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private const int SparklinePointCount = 60;

    private readonly IMetricsCollector _metricsCollector;
    private readonly IPresetOrchestrator _presetOrchestrator;
    private readonly IRollbackEngine _rollbackEngine;
    private readonly ISnapshotManager _snapshotManager;
    private readonly IUserConfirmationService _confirmationService;
    private readonly ICursorOptimizer _cursorOptimizer;
    private readonly IPowerPlanService _powerPlanService;
    private readonly IHealthScorerService _healthScorerService;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly IIndexerExclusionService _indexerExclusionService;
    private readonly IDefenderExclusionService _defenderExclusionService;
    private readonly IWindowsServiceManager _windowsServiceManager;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IProcessPriorityService _processPriorityService;
    private readonly IDevDriveAdvisorService _devDriveAdvisorService;
    private readonly IEnumerable<IOptimizationModule> _modules;
    private readonly ObservableCollection<double> _cpuHistory = new();
    private readonly ObservableCollection<double> _ramHistory = new();
    private CancellationTokenSource? _operationCts;

    public DashboardViewModel(
        IMetricsCollector metricsCollector,
        IPresetOrchestrator presetOrchestrator,
        IRollbackEngine rollbackEngine,
        ISnapshotManager snapshotManager,
        IUserConfirmationService confirmationService,
        ICursorOptimizer cursorOptimizer,
        IPowerPlanService powerPlanService,
        IHealthScorerService healthScorerService,
        IRecommendationEngine recommendationEngine,
        IIndexerExclusionService indexerExclusionService,
        IDefenderExclusionService defenderExclusionService,
        IWindowsServiceManager windowsServiceManager,
        IAppSettingsService appSettingsService,
        IProcessPriorityService processPriorityService,
        IDevDriveAdvisorService devDriveAdvisorService,
        IEnumerable<IOptimizationModule> modules)
    {
        _metricsCollector = metricsCollector;
        _presetOrchestrator = presetOrchestrator;
        _rollbackEngine = rollbackEngine;
        _snapshotManager = snapshotManager;
        _confirmationService = confirmationService;
        _cursorOptimizer = cursorOptimizer;
        _powerPlanService = powerPlanService;
        _healthScorerService = healthScorerService;
        _recommendationEngine = recommendationEngine;
        _indexerExclusionService = indexerExclusionService;
        _defenderExclusionService = defenderExclusionService;
        _windowsServiceManager = windowsServiceManager;
        _appSettingsService = appSettingsService;
        _processPriorityService = processPriorityService;
        _devDriveAdvisorService = devDriveAdvisorService;
        _modules = modules;

        CpuSeries = [CreateSparklineSeries(_cpuHistory, "#7AA2F7")];
        RamSeries = [CreateSparklineSeries(_ramHistory, "#9ECE6A")];
        CpuXAxes = CreateHiddenAxes();
        CpuYAxes = CreatePercentAxes();
        RamXAxes = CreateHiddenAxes();
        RamYAxes = CreatePercentAxes();

        _metricsCollector.MetricUpdated += OnMetricUpdated;
        _metricsCollector.StartPolling();
        _ = RefreshAsync();
    }

    public string Title => "Dashboard";

    public ISeries[] CpuSeries { get; }

    public ISeries[] RamSeries { get; }

    public Axis[] CpuXAxes { get; }

    public Axis[] CpuYAxes { get; }

    public Axis[] RamXAxes { get; }

    public Axis[] RamYAxes { get; }

    public ObservableCollection<ModuleStatusItemViewModel> ModuleStatuses { get; } = new();

    [ObservableProperty]
    private double _cpuPercent;

    [ObservableProperty]
    private long _ramUsedMb;

    [ObservableProperty]
    private long _ramTotalMb;

    [ObservableProperty]
    private double _diskPercent;

    [ObservableProperty]
    private string _healthHint = "Metriken werden geladen…";

    [ObservableProperty]
    private int _healthScore;

    [ObservableProperty]
    private string _healthScoreColor = "#9ECE6A";

    public ObservableCollection<HealthScoreFactor> HealthFactors { get; } = new();

    public ObservableCollection<PerformanceRecommendation> Recommendations { get; } = new();

    [ObservableProperty]
    private string? _lastSnapshotText;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _progressMessage;

    public string RamDisplay => RamTotalMb > 0
        ? $"{RamUsedMb:N0} / {RamTotalMb:N0} MB"
        : "—";

    public double RamPercent => RamTotalMb > 0
        ? RamUsedMb * 100.0 / RamTotalMb
        : 0;

    [RelayCommand]
    private async Task ApplyCursorDevModeAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        ProgressMessage = "Cursor Dev Mode wird angewendet…";
        try
        {
            var result = await _presetOrchestrator.ApplyPresetAsync(PresetIds.CursorDevMode);
            StatusMessage = result.Success
                ? "Cursor Dev Mode erfolgreich angewendet."
                : result.ErrorMessage ?? "Cursor Dev Mode teilweise fehlgeschlagen.";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            ProgressMessage = null;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackLatestAsync()
    {
        var snapshots = await _snapshotManager.GetSnapshotsAsync();
        var latest = snapshots.FirstOrDefault();
        if (latest is null)
        {
            StatusMessage = "Kein Snapshot zum Zurücksetzen vorhanden.";
            return;
        }

        if (!RollbackUiHelper.ConfirmRollback(_confirmationService, latest))
            return;

        _operationCts?.Cancel();
        _operationCts = new CancellationTokenSource();
        var progress = new Progress<string>(message => ProgressMessage = message);

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _rollbackEngine.RollbackSnapshotAsync(latest, progress, _operationCts.Token);
            StatusMessage = result.Success
                ? "Letzter Snapshot zurückgesetzt."
                : result.ErrorMessage;
            await RefreshAsync();
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Rollback abgebrochen.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            ProgressMessage = null;
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelOperation))]
    private void CancelOperation() => _operationCts?.Cancel();

    private bool CanCancelOperation() => IsBusy;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var metric = await _metricsCollector.GetCurrentMetricAsync();
        ApplyMetric(metric);

        var snapshots = await _snapshotManager.GetSnapshotsAsync();
        var latest = snapshots.FirstOrDefault();
        LastSnapshotText = latest is null
            ? "Kein Snapshot vorhanden"
            : $"Letzter Snapshot: {latest.Label} ({latest.CreatedAt.LocalDateTime:g})";

        await RefreshModuleStatusesAsync();
        await RefreshHealthScoreAsync();
        await RefreshRecommendationsAsync();
    }

    private async Task RefreshRecommendationsAsync()
    {
        Recommendations.Clear();
        var items = await _recommendationEngine.GetRecommendationsAsync();
        foreach (var item in items.Take(5))
            Recommendations.Add(item);
    }

    private async Task RefreshHealthScoreAsync()
    {
        var result = await _healthScorerService.CalculateScoreAsync();
        HealthScore = result.Score;
        HealthScoreColor = result.ColorHex;
        HealthFactors.Clear();
        foreach (var factor in result.Factors)
            HealthFactors.Add(factor);

        AdjustHealthHintForOptimizationScore();
    }

    private void AdjustHealthHintForOptimizationScore()
    {
        if (HealthScore >= 50)
            return;

        if (HealthHint.StartsWith("✓ System wirkt entspannt", StringComparison.Ordinal))
            HealthHint = $"Optimierungs-Score {HealthScore}/100 — Faktoren und Empfehlungen prüfen.";
    }

    private async Task RefreshModuleStatusesAsync()
    {
        ModuleStatuses.Clear();
        foreach (var module in _modules.OrderBy(m => m.ModuleName))
            ModuleStatuses.Add(await BuildModuleStatusAsync(module));
    }

    private async Task<ModuleStatusItemViewModel> BuildModuleStatusAsync(IOptimizationModule module)
    {
        switch (module.ModuleName)
        {
            case "PowerPlan":
            {
                var active = await _powerPlanService.GetActivePlanAsync();
                var isHigh = PowerPlanNames.IsHighPerformance(active?.Name);
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    isHigh ? StatusBadgeKind.Active : StatusBadgeKind.Neutral,
                    active?.Name ?? "Kein aktiver Plan");
            }
            case "Cursor":
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    _cursorOptimizer.HasBackup ? StatusBadgeKind.Active : StatusBadgeKind.Neutral,
                    _cursorOptimizer.HasBackup ? "Backup vorhanden" : "Kein Backup");
            case "DefenderExclusions":
            {
                if (!_appSettingsService.Current.DefenderOptIn)
                {
                    return new ModuleStatusItemViewModel(
                        module.ModuleName,
                        StatusBadgeKind.Neutral,
                        "Opt-in nicht aktiv");
                }

                var defenderSet = await _defenderExclusionService.GetExclusionSetAsync();
                var defenderCount = defenderSet.AddedByApp.Count;
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    defenderCount > 0 ? StatusBadgeKind.Active : StatusBadgeKind.Neutral,
                    $"{defenderCount} Ausschlüsse angewendet");
            }
            case "SearchIndexer":
            {
                var entries = await _indexerExclusionService.GetAvailableEntriesAsync();
                var applied = entries.Count(e => e.IsApplied);
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    applied >= 2 ? StatusBadgeKind.Active : applied == 1 ? StatusBadgeKind.Neutral : StatusBadgeKind.Warning,
                    $"{applied} Ausschlüsse angewendet");
            }
            case "Services":
            {
                var services = await _windowsServiceManager.GetServicesAsync();
                var sysMain = services.FirstOrDefault(s =>
                    s.Name.Equals("SysMain", StringComparison.OrdinalIgnoreCase));
                var wSearch = services.FirstOrDefault(s =>
                    s.Name.Equals("WSearch", StringComparison.OrdinalIgnoreCase));
                var optimized =
                    (sysMain?.StartupType is "Manual" or "Disabled" ? 1 : 0) +
                    (wSearch?.StartupType is "Manual" or "Disabled" ? 1 : 0);
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    optimized == 2 ? StatusBadgeKind.Active : StatusBadgeKind.Neutral,
                    $"SysMain: {sysMain?.StartupType ?? "—"}, WSearch: {wSearch?.StartupType ?? "—"}");
            }
            case "ProcessPriority":
            {
                var cursorStatus = _processPriorityService.GetCursorProcessStatus();
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    cursorStatus is not null ? StatusBadgeKind.Active : StatusBadgeKind.Neutral,
                    cursorStatus ?? "Keine Cursor-Prozesse");
            }
            case "DevDriveAdvisor":
            {
                var devState = await _devDriveAdvisorService.GetAssessmentAsync();
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    devState.HasDevDrive && devState.PathsNeedingMigration == 0
                        ? StatusBadgeKind.Active
                        : devState.HasDevDrive
                            ? StatusBadgeKind.Warning
                            : StatusBadgeKind.Neutral,
                    devState.SummaryText);
            }
            default:
                return new ModuleStatusItemViewModel(
                    module.ModuleName,
                    StatusBadgeKind.Neutral,
                    module.CanApply ? "Bereit" : "Manuelle Bestätigung nötig");
        }
    }

    private void OnMetricUpdated(object? sender, PerformanceMetric metric) =>
        ApplyMetric(metric);

    private void ApplyMetric(PerformanceMetric metric)
    {
        CpuPercent = metric.CpuPercent;
        RamUsedMb = metric.RamUsedMb;
        RamTotalMb = metric.RamTotalMb;
        DiskPercent = metric.DiskActivePercent;
        HealthHint = BuildHealthHint(metric);
        AppendHistoryPoint(_cpuHistory, metric.CpuPercent);
        var ramPercent = metric.RamTotalMb > 0
            ? metric.RamUsedMb * 100.0 / metric.RamTotalMb
            : 0;
        AppendHistoryPoint(_ramHistory, ramPercent);
        OnPropertyChanged(nameof(RamDisplay));
        OnPropertyChanged(nameof(RamPercent));
    }

    private static void AppendHistoryPoint(ObservableCollection<double> history, double value)
    {
        history.Add(value);
        while (history.Count > SparklinePointCount)
            history.RemoveAt(0);
    }

    private static LineSeries<double> CreateSparklineSeries(ObservableCollection<double> values, string colorHex) =>
        new()
        {
            Values = values,
            Fill = new SolidColorPaint(SKColor.Parse(colorHex).WithAlpha(32)),
            Stroke = new SolidColorPaint(SKColor.Parse(colorHex)) { StrokeThickness = 1.5f },
            GeometryFill = null,
            GeometryStroke = null,
            GeometrySize = 0,
            LineSmoothness = 0.35,
        };

    private static Axis[] CreateHiddenAxes() =>
    [
        new Axis
        {
            IsVisible = false,
            ShowSeparatorLines = false,
        }
    ];

    private static Axis[] CreatePercentAxes() =>
    [
        new Axis
        {
            MinLimit = 0,
            MaxLimit = 100,
            IsVisible = false,
            ShowSeparatorLines = false,
        }
    ];

    private static string BuildHealthHint(PerformanceMetric metric)
    {
        if (metric.CpuPercent >= 90)
            return "⚠ CPU-Auslastung sehr hoch — Hintergrundprozesse prüfen.";
        if (metric.RamTotalMb > 0 && metric.RamUsedMb * 100.0 / metric.RamTotalMb >= 90)
            return "⚠ Arbeitsspeicher fast voll — andere Anwendungen schließen.";
        if (metric.DiskActivePercent >= 80)
            return "⚠ Festplatte stark ausgelastet — I/O-Engpass möglich.";
        if (metric.CpuPercent < 50 && metric.RamTotalMb > 0 && metric.RamUsedMb * 100.0 / metric.RamTotalMb < 75)
            return "✓ System wirkt entspannt — gut für Entwicklung.";
        return "System im normalen Bereich.";
    }

    public void Dispose()
    {
        _metricsCollector.MetricUpdated -= OnMetricUpdated;
    }
}
