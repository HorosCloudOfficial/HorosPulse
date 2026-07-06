namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class SnapshotViewModel : ViewModelBase
{
    private readonly ISnapshotManager _snapshotManager;
    private readonly IRollbackEngine _rollbackEngine;
    private readonly IUserConfirmationService _confirmationService;
    private CancellationTokenSource? _operationCts;

    public SnapshotViewModel(
        ISnapshotManager snapshotManager,
        IRollbackEngine rollbackEngine,
        IUserConfirmationService confirmationService)
    {
        _snapshotManager = snapshotManager;
        _rollbackEngine = rollbackEngine;
        _confirmationService = confirmationService;
        _ = LoadSnapshotsAsync();
    }

    public string Title => "Snapshots";

    public ObservableCollection<SnapshotItemViewModel> Snapshots { get; } = new();

    [ObservableProperty]
    private SnapshotItemViewModel? _selectedSnapshot;

    [ObservableProperty]
    private string _newSnapshotLabel = "manual-snapshot";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _progressMessage;

    [RelayCommand]
    private async Task LoadSnapshotsAsync()
    {
        IsBusy = true;
        try
        {
            Snapshots.Clear();
            var entries = await _snapshotManager.GetSnapshotsAsync();
            foreach (var entry in entries.OrderByDescending(s => s.CreatedAt))
                Snapshots.Add(new SnapshotItemViewModel(entry));
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
    private async Task CreateSnapshotAsync()
    {
        var label = string.IsNullOrWhiteSpace(NewSnapshotLabel)
            ? $"manual-{DateTime.Now:yyyyMMdd-HHmm}"
            : NewSnapshotLabel.Trim();

        IsBusy = true;
        StatusMessage = null;
        ProgressMessage = "Snapshot wird erstellt…";
        try
        {
            await _snapshotManager.CreateBaselineAsync(label);
            StatusMessage = $"Snapshot \"{label}\" erstellt.";
            await LoadSnapshotsAsync();
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

    [RelayCommand(CanExecute = nameof(CanRollbackSelected))]
    private async Task RollbackSelectedAsync()
    {
        if (SelectedSnapshot is null)
            return;

        var snapshot = await _snapshotManager.GetSnapshotAsync(SelectedSnapshot.Id);
        if (snapshot is null)
        {
            StatusMessage = "Snapshot nicht mehr vorhanden.";
            return;
        }

        var confirmed = _confirmationService.Confirm(
            "Rollback bestätigen",
            $"Snapshot \"{snapshot.Label}\" ({snapshot.CreatedAt.LocalDateTime:g}) für Modul \"{snapshot.Module}\" zurücksetzen?",
            isWarning: true);

        if (!confirmed)
            return;

        _operationCts?.Cancel();
        _operationCts = new CancellationTokenSource();
        var progress = new Progress<string>(message => ProgressMessage = message);

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _rollbackEngine.RollbackSnapshotAsync(snapshot, progress, _operationCts.Token);
            StatusMessage = result.Success
                ? $"Rollback von \"{snapshot.Label}\" abgeschlossen."
                : result.ErrorMessage;
            await LoadSnapshotsAsync();
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
    private void CancelOperation()
    {
        _operationCts?.Cancel();
    }

    private bool CanCancelOperation() => IsBusy;

    private bool CanRollbackSelected() => SelectedSnapshot is { CanRollback: true, IsValid: true } && !IsBusy;

    partial void OnSelectedSnapshotChanged(SnapshotItemViewModel? value) =>
        RollbackSelectedCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
    {
        RollbackSelectedCommand.NotifyCanExecuteChanged();
        CancelOperationCommand.NotifyCanExecuteChanged();
    }
}
