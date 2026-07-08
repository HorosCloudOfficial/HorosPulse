namespace HorosPulse.Core.Interfaces;

/// <summary>
/// Marshals elevation-sensitive UI work (UAC shell launch) onto the WPF UI thread.
/// </summary>
public interface IElevationUiInvoker
{
    /// <summary>Brings the active HorosPulse window to the foreground before a UAC prompt.</summary>
    Task PrepareForUacPromptAsync(CancellationToken cancellationToken = default);

    /// <summary>Runs an action on the UI thread when a dispatcher is available.</summary>
    Task InvokeAsync(Action action, CancellationToken cancellationToken = default);

    /// <summary>Runs a function on the UI thread when a dispatcher is available.</summary>
    Task<T> InvokeAsync<T>(Func<T> func, CancellationToken cancellationToken = default);
}
