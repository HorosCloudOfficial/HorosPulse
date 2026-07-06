namespace HorosPulse.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosPulse.Core.Interfaces;

public sealed partial class MemoryViewModel : ViewModelBase
{
    private readonly IMemoryOptimizerService _memoryOptimizer;

    public MemoryViewModel(IMemoryOptimizerService memoryOptimizer)
    {
        _memoryOptimizer = memoryOptimizer;
        _ = RefreshMemoryAsync();
    }

    public string Title => "Speicher";

    [ObservableProperty]
    private long _ramBeforeMb;

    [ObservableProperty]
    private long _ramAfterMb;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public string WarningText =>
        "Standby-Speicher leeren ist ein temporärer Effekt. Kurzzeitig kann die Performance schlechter werden, bis Windows den Cache wieder aufbaut.";

    [RelayCommand]
    private async Task RefreshMemoryAsync()
    {
        RamBeforeMb = await _memoryOptimizer.GetAvailableMemoryMbAsync();
        RamAfterMb = RamBeforeMb;
    }

    [RelayCommand]
    private async Task FlushAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            RamBeforeMb = await _memoryOptimizer.GetAvailableMemoryMbAsync();
            var result = await _memoryOptimizer.PurgeStandbyListAsync();
            await Task.Delay(500);
            RamAfterMb = await _memoryOptimizer.GetAvailableMemoryMbAsync();
            StatusMessage = result.Success
                ? $"Flush abgeschlossen. Verfügbar: {RamBeforeMb} → {RamAfterMb} MB"
                : result.ErrorMessage;
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
}
