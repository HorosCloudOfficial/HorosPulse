namespace WindowsPerformance.Services.Ml;

using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class MetricsAnomalyService : IMetricsAnomalyService
{
    private readonly IPerformanceMetricRepository _metricRepository;
    private readonly ILogger<MetricsAnomalyService> _logger;

    public MetricsAnomalyService(
        IPerformanceMetricRepository metricRepository,
        ILogger<MetricsAnomalyService> logger)
    {
        _metricRepository = metricRepository;
        _logger = logger;
    }

    public async Task<MetricsAnomalyResult> DetectAnomaliesAsync(CancellationToken cancellationToken = default)
    {
        await _metricRepository.InitializeAsync(cancellationToken);
        var since = DateTimeOffset.UtcNow.AddHours(-24);
        var metrics = await _metricRepository.GetSinceAsync(since, cancellationToken);

        if (metrics.Count < 20)
        {
            return new MetricsAnomalyResult
            {
                HasAnomaly = false,
                Anomalies = Array.Empty<AnomalyPoint>(),
            };
        }

        try
        {
            var data = metrics
                .OrderBy(m => m.Timestamp)
                .Select(m => new MetricPoint { CpuPercent = (float)m.CpuPercent })
                .ToList();

            var ml = new MLContext(seed: 42);
            var dataView = ml.Data.LoadFromEnumerable(data);

            var pipeline = ml.Transforms.DetectIidSpike(
                outputColumnName: "Prediction",
                inputColumnName: nameof(MetricPoint.CpuPercent),
                confidence: 95,
                pvalueHistoryLength: Math.Min(30, data.Count / 2));

            var model = pipeline.Fit(dataView);
            var transformed = model.Transform(dataView);
            var predictions = ml.Data.CreateEnumerable<SpikePrediction>(transformed, reuseRowObject: false).ToList();

            var anomalies = new List<AnomalyPoint>();
            for (var i = 0; i < metrics.Count && i < predictions.Count; i++)
            {
                if (predictions[i].Prediction[0] != 1)
                    continue;

                anomalies.Add(new AnomalyPoint
                {
                    Timestamp = metrics[i].Timestamp,
                    CpuPercent = metrics[i].CpuPercent,
                    ExpectedCpuPercent = data[i].CpuPercent,
                });
            }

            return new MetricsAnomalyResult
            {
                HasAnomaly = anomalies.Count > 0,
                Anomalies = anomalies,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Anomalie-Erkennung fehlgeschlagen");
            return new MetricsAnomalyResult
            {
                HasAnomaly = false,
                Anomalies = Array.Empty<AnomalyPoint>(),
            };
        }
    }

    private sealed class MetricPoint
    {
        public float CpuPercent { get; set; }
    }

    private sealed class SpikePrediction
    {
        public float[] Prediction { get; set; } = Array.Empty<float>();
    }
}
