namespace WindowsPerformance.Core.Interfaces;

/// <summary>
/// Wendet persistierte App-Einstellungen auf Runtime-Optionen an (Theme, Logging, PowerShell).
/// </summary>
public interface ISettingsApplyService
{
    /// <summary>Aktuelle Einstellungen aus <see cref="IAppSettingsService"/> anwenden.</summary>
    void ApplyCurrent();
}
