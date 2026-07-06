namespace WindowsPerformance.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using WindowsPerformance.Core.Enums;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed partial class TrendViewModel : ViewModelBase
{
    private readonly ITrendAnalysisService _trendAnalysisService;

    public TrendViewModel(ITrendAnalysisService trendAnalysisService)
    {
        _trendAnalysisService = trendAnalysisService;
        CpuSeries = [CreateLineSeries(_cpuValues, "#7AA2F7")];
        RamSeries = [CreateLineSeries(_ramValues, "#9ECE6A")];
        DiskSeries = [CreateLineSeries(_diskValues, "#BB9AF7")];
        XAxes = [new Axis { Labels = _labels, LabelsRotation = 45, TextSize = 10 }];
        YAxes = [new Axis { MinLimit = 0, MaxLimit = 100, Name = "%" }];
        _ = LoadTrendsAsync();
    }

    public string Title => "Trends";

    private readonly ObservableCollection<double> _cpuValues = new();
    private readonly ObservableCollection<double> _ramValues = new();
    private readonly ObservableCollection<double> _diskValues = new();
    private readonly ObservableCollection<string> _labels = new();

    public ISeries[] CpuSeries { get; }
    public ISeries[] RamSeries { get; }
    public ISeries[] DiskSeries { get; }
    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }

    public ObservableCollection<TrendAnnotation> Annotations { get; } = new();

    [ObservableProperty]
    private TrendTimeRange _selectedRange = TrendTimeRange.OneHour;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task SelectRangeAsync(string rangeName)
    {
        SelectedRange = rangeName switch
        {
            "FiveMinutes" => TrendTimeRange.FiveMinutes,
            "TwentyFourHours" => TrendTimeRange.TwentyFourHours,
            _ => TrendTimeRange.OneHour,
        };
        await LoadTrendsAsync();
    }

    [RelayCommand]
    private async Task LoadTrendsAsync()
    {
        IsBusy = true;
        try
        {
            var buckets = await _trendAnalysisService.GetTrendBucketsAsync(SelectedRange);
            var annotations = await _trendAnalysisService.GetAnnotationsAsync(SelectedRange);

            _cpuValues.Clear();
            _ramValues.Clear();
            _diskValues.Clear();
            _labels.Clear();
            Annotations.Clear();

            foreach (var bucket in buckets)
            {
                _cpuValues.Add(bucket.CpuPercent);
                _ramValues.Add(bucket.RamPercent);
                _diskValues.Add(bucket.DiskPercent);
                _labels.Add(bucket.Timestamp.LocalDateTime.ToString("HH:mm"));
            }

            foreach (var annotation in annotations)
                Annotations.Add(annotation);

            StatusMessage = buckets.Count == 0 ? "Keine Metrik-Daten im gewählten Zeitraum." : null;
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

    partial void OnSelectedRangeChanged(TrendTimeRange value) { }

    private static LineSeries<double> CreateLineSeries(ObservableCollection<double> values, string colorHex) =>
        new()
        {
            Values = values,
            Fill = null,
            Stroke = new SolidColorPaint(SKColor.Parse(colorHex)) { StrokeThickness = 2 },
            GeometrySize = 4,
            LineSmoothness = 0.3,
        };
}
