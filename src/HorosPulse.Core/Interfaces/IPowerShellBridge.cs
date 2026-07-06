namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Führt PowerShell-Skripte aus (normal oder elevated).
/// </summary>
public interface IPowerShellBridge
{
    /// <summary>Skript ausführen; optional elevated mit Timeout.</summary>
    Task<PowerShellResult> RunAsync(string script, bool elevated = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>Ob eine PowerShell-Executable verfügbar ist.</summary>
    bool IsPowerShellAvailable { get; }

    /// <summary>Pfad zur verwendeten PowerShell-Executable (pwsh oder powershell).</summary>
    string? PowerShellExecutable { get; }
}
