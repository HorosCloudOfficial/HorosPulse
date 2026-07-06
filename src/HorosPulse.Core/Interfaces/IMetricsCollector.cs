namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Sammelt System-Performance-Metriken (CPU, RAM, Disk) per Polling.
/// </summary>
public interface IMetricsCollector : IDisposable
{
    /// <summary>Aktuelle CPU-Auslastung in Prozent.</summary>
    Task<double> GetCpuUsagePercentAsync(CancellationToken cancellationToken = default);

    /// <summary>Verwendeter Arbeitsspeicher in MB.</summary>
    Task<long> GetMemoryUsedMbAsync(CancellationToken cancellationToken = default);

    /// <summary>Gesamter Arbeitsspeicher in MB.</summary>
    Task<long> GetMemoryTotalMbAsync(CancellationToken cancellationToken = default);

    /// <summary>Disk-Aktivität in Prozent.</summary>
    Task<double> GetDiskActivePercentAsync(CancellationToken cancellationToken = default);

    /// <summary>Aktuelle Metrik als zusammengesetztes Objekt.</summary>
    Task<PerformanceMetric> GetCurrentMetricAsync(CancellationToken cancellationToken = default);

    /// <summary>Wird ausgelöst wenn Polling eine neue Metrik liefert.</summary>
    event EventHandler<PerformanceMetric>? MetricUpdated;

    /// <summary>Periodisches Polling starten.</summary>
    void StartPolling();

    /// <summary>Polling stoppen.</summary>
    void StopPolling();

    /// <summary>Metriken als IObservable-Stream (Polling-basiert).</summary>
    IObservable<PerformanceMetric> ObserveMetrics();
}
