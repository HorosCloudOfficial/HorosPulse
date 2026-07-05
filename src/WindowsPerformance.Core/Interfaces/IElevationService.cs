namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IElevationService
{
    Task<PowerShellResult> RunElevatedScriptAsync(string script, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    bool IsHelperAvailable { get; }
}
