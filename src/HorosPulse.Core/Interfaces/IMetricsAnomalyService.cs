namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

/// <summary>
/// Erkennt CPU-Anomalien in historischen Metriken via ML.NET Time Series.
/// </summary>
public interface IMetricsAnomalyService
{
    /// <summary>Letzte Metriken auf Anomalien analysieren.</summary>
    Task<MetricsAnomalyResult> DetectAnomaliesAsync(CancellationToken cancellationToken = default);
}
