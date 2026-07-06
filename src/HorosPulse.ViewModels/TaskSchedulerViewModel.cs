namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class TaskSchedulerViewModel : ViewModelBase
{
    private readonly ITaskSchedulerService _taskSchedulerService;
    private readonly IUserConfirmationService _confirmationService;

    public TaskSchedulerViewModel(
        ITaskSchedulerService taskSchedulerService,
        IUserConfirmationService confirmationService)
    {
        _taskSchedulerService = taskSchedulerService;
        _confirmationService = confirmationService;
        _ = LoadTasksAsync();
    }

    public string Title => "Geplante Tasks";

    public ObservableCollection<ScheduledTaskInfo> Tasks { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task LoadTasksAsync()
    {
        IsBusy = true;
        try
        {
            Tasks.Clear();
            var tasks = await _taskSchedulerService.GetTasksAsync();
            foreach (var task in tasks.Where(t =>
                         t.Path.Contains("Update", StringComparison.OrdinalIgnoreCase) ||
                         t.Path.Contains("Maintenance", StringComparison.OrdinalIgnoreCase))
                         .Take(100))
                Tasks.Add(task);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApplyDevProtectionAsync()
    {
        if (!_confirmationService.Confirm(
                "Dev-Zeit-Schutz",
                "Störende Windows-Update-Tasks werden temporär deaktiviert. Fortfahren?"))
            return;

        IsBusy = true;
        try
        {
            var result = await _taskSchedulerService.ApplyDevProtectionPresetAsync();
            StatusMessage = result.Success ? string.Join("; ", result.Changes ?? []) : result.ErrorMessage;
            await LoadTasksAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RollbackAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _taskSchedulerService.RollbackAsync();
            StatusMessage = result.Success ? "Tasks wieder aktiviert" : result.ErrorMessage;
            await LoadTasksAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
