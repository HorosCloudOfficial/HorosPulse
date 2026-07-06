namespace HorosPulse.Core.Interfaces;

/// <summary>
/// Steuert die minimale Serilog-Logstufe zur Laufzeit.
/// </summary>
public interface ILoggingLevelService
{
    /// <summary>Aktuelle minimale Logstufe (Serilog-Namen).</summary>
    string CurrentLevel { get; }

    /// <summary>Minimale Logstufe setzen (Verbose, Debug, Information, Warning, Error).</summary>
    void SetMinimumLevel(string level);
}
