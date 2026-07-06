namespace HorosPulse.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed partial class StartupViewModel : ViewModelBase
{
    private readonly IStartupManagerService _startupManager;

    public StartupViewModel(IStartupManagerService startupManager)
    {
        _startupManager = startupManager;
        _ = LoadEntriesAsync();
    }

    public string Title => "Startup-Programme";

    public ObservableCollection<StartupEntryViewModel> Entries { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task LoadEntriesAsync()
    {
        IsBusy = true;
        try
        {
            Entries.Clear();
            foreach (var entry in await _startupManager.GetEntriesAsync())
                Entries.Add(new StartupEntryViewModel(entry, ToggleEntryAsync));
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
    private async Task RollbackAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _startupManager.RollbackAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await LoadEntriesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ToggleEntryAsync(StartupEntryViewModel item)
    {
        IsBusy = true;
        try
        {
            var result = await _startupManager.SetEnabledAsync(item.Entry, item.IsEnabled);
            if (result.Success)
                item.ConfirmToggle();
            else
            {
                StatusMessage = result.ErrorMessage;
                item.RevertToggle();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            item.RevertToggle();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed partial class StartupEntryViewModel : ObservableObject
{
    private readonly Func<StartupEntryViewModel, Task> _toggleHandler;
    private bool _previousEnabled;

    public StartupEntryViewModel(StartupEntry entry, Func<StartupEntryViewModel, Task> toggleHandler)
    {
        Entry = entry;
        _toggleHandler = toggleHandler;
        _isEnabled = entry.IsEnabled;
        _previousEnabled = entry.IsEnabled;
    }

    public StartupEntry Entry { get; }

    public string Name => Entry.Name;
    public string Command => Entry.Command;
    public string Source => Entry.Source;

    [ObservableProperty]
    private bool _isEnabled;

    partial void OnIsEnabledChanged(bool value)
    {
        if (value == _previousEnabled)
            return;

        _ = _toggleHandler(this);
    }

    internal void RevertToggle()
    {
        IsEnabled = _previousEnabled;
    }

    internal void ConfirmToggle() => _previousEnabled = IsEnabled;
}
