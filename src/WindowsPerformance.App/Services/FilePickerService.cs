namespace WindowsPerformance.App.Services;

using System.Windows.Threading;
using Microsoft.Win32;
using WindowsPerformance.Core.Interfaces;

public sealed class FilePickerService : IFilePickerService
{
    public Task<string?> PickSaveFileAsync(string filter, string defaultFileName)
    {
        return InvokeOnUiThread(() =>
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName,
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        });
    }

    public Task<string?> PickOpenFileAsync(string filter)
    {
        return InvokeOnUiThread(() =>
        {
            var dialog = new OpenFileDialog { Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        });
    }

    private static Task<string?> InvokeOnUiThread(Func<string?> action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            return Task.FromResult(action());

        return dispatcher.InvokeAsync(action).Task;
    }
}
