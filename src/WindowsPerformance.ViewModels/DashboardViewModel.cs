namespace WindowsPerformance.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;
using WindowsPerformance.Core;

public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly IPresetOrchestrator _presetOrchestrator;
    private readonly IRollbackEngine _rollbackEngine;
    private readonly ISnapshotManager _snapshotManager;

    public DashboardViewModel(
        IMetricsCollector metricsCollector,
        IPresetOrchestrator presetOrchestrator,
        IRollbackEngine rollbackEngine,
        ISnapshotManager snapshotManager)
    {
        _metricsCollector = metricsCollector;
        _presetOrchestrator = presetOrchestrator;
        _rollbackEngine = rollbackEngine;
        _snapshotManager = snapshotManager;

        _metricsCollector.MetricUpdated += OnMetricUpdated;
        _metricsCollector.StartPolling();
        _ = RefreshAsync();
    }

    public string Title => "Dashboard";

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
    private string? _lastSnapshotText;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public string RamDisplay => RamTotalMb > 0
        ? $"{RamUsedMb:N0} / {RamTotalMb:N0} MB"
        : "—";

    [RelayCommand]
    private async Task ApplyCursorDevModeAsync()
    {
        IsBusy = true;
        StatusMessage = null;
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
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackLatestAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _rollbackEngine.RollbackLatestAsync();
            StatusMessage = result.Success
                ? "Letzter Snapshot zurückgesetzt."
                : result.ErrorMessage;
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

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
        OnPropertyChanged(nameof(RamDisplay));
    }

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
