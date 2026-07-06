namespace HorosPulse.Services.Ml;

using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class RecommendationEngine : IRecommendationEngine
{
    private readonly IPerformanceMetricRepository _metricRepository;
    private readonly IMetricsAnomalyService _anomalyService;
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<RecommendationEngine> _logger;

    public RecommendationEngine(
        IPerformanceMetricRepository metricRepository,
        IMetricsAnomalyService anomalyService,
        IAuditRepository auditRepository,
        ILogger<RecommendationEngine> logger)
    {
        _metricRepository = metricRepository;
        _anomalyService = anomalyService;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PerformanceRecommendation>> GetRecommendationsAsync(
        CancellationToken cancellationToken = default)
    {
        var recommendations = new List<PerformanceRecommendation>();

        try
        {
            await _metricRepository.InitializeAsync(cancellationToken);
            var since = DateTimeOffset.UtcNow.AddHours(-24);
            var metrics = await _metricRepository.GetSinceAsync(since, cancellationToken);

            if (metrics.Count > 0)
            {
                var peak = metrics.OrderByDescending(m => m.CpuPercent).First();
                if (peak.CpuPercent >= 90)
                {
                    recommendations.Add(new PerformanceRecommendation
                    {
                        Title = "Hohe CPU-Auslastung",
                        Message =
                            $"Gestern um {peak.Timestamp.LocalDateTime:t} war CPU bei {peak.CpuPercent:F0}%. " +
                            "Prüfen Sie Hintergrundprozesse (z. B. Defender, Indexer) und erwägen Sie Dev-Presets.",
                        Severity = "Warning",
                        RelatedTimestamp = peak.Timestamp,
                    });
                }
            }

            var anomalies = await _anomalyService.DetectAnomaliesAsync(cancellationToken);
            foreach (var anomaly in anomalies.Anomalies.Take(3))
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Title = "CPU-Anomalie erkannt",
                    Message =
                        $"Ungewöhnlicher CPU-Spike um {anomaly.Timestamp.LocalDateTime:t} " +
                        $"({anomaly.CpuPercent:F0}% vs. erwartet ~{anomaly.ExpectedCpuPercent:F0}%).",
                    Severity = "Info",
                    RelatedTimestamp = anomaly.Timestamp,
                });
            }

            var audit = await _auditRepository.GetSinceAsync(DateTimeOffset.UtcNow.AddDays(-1), cancellationToken);
            var defenderOps = audit.Count(e =>
                e.Module.Contains("Defender", StringComparison.OrdinalIgnoreCase));
            if (defenderOps == 0 && metrics.Any(m => m.CpuPercent > 80))
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Title = "Defender-Exclusions prüfen",
                    Message =
                        "Bei wiederholten CPU-Spitzen können workspace-spezifische Defender-Ausschlüsse helfen (Opt-in in Einstellungen).",
                    Severity = "Info",
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Empfehlungs-Engine fehlgeschlagen");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new PerformanceRecommendation
            {
                Title = "Alles in Ordnung",
                Message = "Keine auffälligen Muster in den letzten 24 Stunden.",
                Severity = "Info",
            });
        }

        return recommendations;
    }
}
