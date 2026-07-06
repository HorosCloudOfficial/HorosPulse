namespace WindowsPerformance.Services.Monitoring;

using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

/// <summary>
/// Einfacher IObservable-Wrapper um <see cref="IMetricsCollector.MetricUpdated"/>.
/// </summary>
public sealed class MetricsObservableAdapter : IObservable<PerformanceMetric>
{
    private readonly IMetricsCollector _collector;

    public MetricsObservableAdapter(IMetricsCollector collector) => _collector = collector;

    public IDisposable Subscribe(IObserver<PerformanceMetric> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        async void Handler(object? sender, PerformanceMetric metric)
        {
            try
            {
                observer.OnNext(metric);
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }

        _collector.MetricUpdated += Handler;

        _ = EmitCurrentAsync(observer);

        return new Unsubscriber(_collector, Handler, observer);
    }

    private async Task EmitCurrentAsync(IObserver<PerformanceMetric> observer)
    {
        try
        {
            var current = await _collector.GetCurrentMetricAsync();
            observer.OnNext(current);
        }
        catch (Exception ex)
        {
            observer.OnError(ex);
        }
    }

    private sealed class Unsubscriber : IDisposable
    {
        private readonly IMetricsCollector _collector;
        private readonly EventHandler<PerformanceMetric> _handler;
        private readonly IObserver<PerformanceMetric> _observer;
        private bool _disposed;

        public Unsubscriber(
            IMetricsCollector collector,
            EventHandler<PerformanceMetric> handler,
            IObserver<PerformanceMetric> observer)
        {
            _collector = collector;
            _handler = handler;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _collector.MetricUpdated -= _handler;
            _observer.OnCompleted();
            _disposed = true;
        }
    }
}
