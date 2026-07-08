namespace HorosPulse.Services.Elevation;

using HorosPulse.Core.Interfaces;

/// <summary>Inline invoker for headless/tests when no WPF dispatcher exists.</summary>
internal sealed class SyncElevationUiInvoker : IElevationUiInvoker
{
    public Task PrepareForUacPromptAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        action();
        return Task.CompletedTask;
    }

    public Task<T> InvokeAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(func());
    }
}
