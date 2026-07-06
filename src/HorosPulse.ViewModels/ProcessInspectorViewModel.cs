namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class ProcessInspectorViewModel : ViewModelBase
{
    private readonly IProcessInspectorService _processInspector;
    private readonly IUserConfirmationService _confirmationService;

    public ProcessInspectorViewModel(
        IProcessInspectorService processInspector,
        IUserConfirmationService confirmationService)
    {
        _processInspector = processInspector;
        _confirmationService = confirmationService;
        _ = RefreshAsync();
    }

    public string Title => "Prozess-Inspektor";

    public ObservableCollection<ProcessInspectorEntry> Processes { get; } = new();

    [ObservableProperty]
    private string _filterText = "cursor,node,git,eslint,tsc";

    [ObservableProperty]
    private ProcessInspectorEntry? _selectedProcess;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            Processes.Clear();
            foreach (var process in await _processInspector.GetProcessesAsync(FilterText))
                Processes.Add(process);
            StatusMessage = $"{Processes.Count} Prozesse";
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

    [RelayCommand(CanExecute = nameof(CanKill))]
    private async Task KillSelectedAsync()
    {
        if (SelectedProcess is null)
            return;

        if (!_confirmationService.Confirm(
                "Prozess beenden",
                $"Prozess „{SelectedProcess.Name}\" (PID {SelectedProcess.ProcessId}) wirklich beenden?",
                isWarning: true))
            return;

        IsBusy = true;
        try
        {
            var result = await _processInspector.KillProcessAsync(SelectedProcess.ProcessId);
            StatusMessage = result.Success ? result.Changes?.FirstOrDefault() : result.ErrorMessage;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanKill() => SelectedProcess is not null && !IsBusy;

    partial void OnSelectedProcessChanged(ProcessInspectorEntry? value) =>
        KillSelectedCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value) =>
        KillSelectedCommand.NotifyCanExecuteChanged();
}
