namespace WindowsPerformance.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed partial class MonitorViewModel : ViewModelBase, IDisposable
{
    private readonly IProcessMonitorService _processMonitorService;
    private readonly SynchronizationContext? _uiContext;

    public MonitorViewModel(IProcessMonitorService processMonitorService)
    {
        _processMonitorService = processMonitorService;
        _uiContext = SynchronizationContext.Current;
        _processMonitorService.ProcessesUpdated += OnProcessesUpdated;
        _processMonitorService.StartPolling();
        _ = RefreshProcessesAsync();
    }

    public string Title => "Monitor";

    public string Description => "Live-Prozessliste mit CPU- und RAM-Auslastung";

    public ObservableCollection<ProcessMonitorEntry> Processes { get; } = new();

    [ObservableProperty]
    private string? _statusMessage;

    private async Task RefreshProcessesAsync()
    {
        try
        {
            var processes = await _processMonitorService.GetProcessesAsync();
            UpdateProcesses(processes);
            StatusMessage = $"{processes.Count} Prozesse";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void OnProcessesUpdated(object? sender, IReadOnlyList<ProcessMonitorEntry> processes)
    {
        if (_uiContext is not null)
            _uiContext.Post(_ => UpdateProcesses(processes), null);
        else
            UpdateProcesses(processes);
    }

    private void UpdateProcesses(IReadOnlyList<ProcessMonitorEntry> processes)
    {
        Processes.Clear();
        foreach (var process in processes.Take(200))
            Processes.Add(process);

        StatusMessage = $"{processes.Count} Prozesse (Top 200 nach CPU)";
    }

    public void Dispose()
    {
        _processMonitorService.ProcessesUpdated -= OnProcessesUpdated;
    }
}
