namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;

public sealed partial class NetworkViewModel : ViewModelBase
{
    private readonly INetworkOptimizerService _networkOptimizer;

    public NetworkViewModel(INetworkOptimizerService networkOptimizer)
    {
        _networkOptimizer = networkOptimizer;
        _ = RefreshAsync();
    }

    public string Title => "Netzwerk";

    [ObservableProperty]
    private string _currentSettingsText = "—";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var settings = await _networkOptimizer.GetCurrentSettingsAsync();
        CurrentSettingsText =
            $"TCPNoDelay={settings.TcpNoDelay?.ToString() ?? "—"}, " +
            $"TcpAckFrequency={settings.TcpAckFrequency?.ToString() ?? "—"}, " +
            $"DNS MaxCacheTtl={settings.DnsMaxCacheTtl?.ToString() ?? "—"}";
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _networkOptimizer.ApplyOptimizationsAsync();
            StatusMessage = result.Success
                ? string.Join("; ", result.Changes ?? [])
                : result.ErrorMessage;
            await RefreshAsync();
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
            var result = await _networkOptimizer.RollbackAsync();
            StatusMessage = result.Success ? "Zurückgesetzt" : result.ErrorMessage;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
