namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// Überwacht laufende Prozesse (CPU, RAM) für die Monitor-Ansicht.
/// </summary>
public interface IProcessMonitorService : IDisposable
{
    /// <summary>Wird ausgelöst wenn Prozessliste aktualisiert wurde.</summary>
    event EventHandler<IReadOnlyList<ProcessMonitorEntry>>? ProcessesUpdated;

    /// <summary>Aktuelle Prozessliste abrufen.</summary>
    Task<IReadOnlyList<ProcessMonitorEntry>> GetProcessesAsync(CancellationToken cancellationToken = default);

    /// <summary>Periodisches Polling starten.</summary>
    void StartPolling();

    /// <summary>Polling stoppen.</summary>
    void StopPolling();
}
