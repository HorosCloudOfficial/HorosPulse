namespace HorosPulse.App.Services;

using System.Windows;
using System.Windows.Threading;
using HorosPulse.Core.Interfaces;

public sealed class ElevationUiInvoker : IElevationUiInvoker
{
    public Task PrepareForUacPromptAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(() =>
        {
            var app = Application.Current;
            if (app is null)
                return;

            var window = app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w.IsVisible)
                ?? app.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible)
                ?? app.MainWindow;

            window?.Activate();
            window?.Focus();
        }, cancellationToken);

    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
    }

    public Task<T> InvokeAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            return Task.FromResult(func());

        return dispatcher.InvokeAsync(func, DispatcherPriority.Normal, cancellationToken).Task;
    }
}
