namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IPowerShellBridge
{
    Task<PowerShellResult> RunAsync(string script, bool elevated = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    bool IsPowerShellAvailable { get; }
    string? PowerShellExecutable { get; }
}
