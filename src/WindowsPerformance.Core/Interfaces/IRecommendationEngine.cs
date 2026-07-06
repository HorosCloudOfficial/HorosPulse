namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

/// <summary>
/// Generiert lokale Performance-Empfehlungen aus SQLite-Metriken und Audit-Daten.
/// </summary>
public interface IRecommendationEngine
{
    /// <summary>Empfehlungen basierend auf lokalen Daten generieren.</summary>
    Task<IReadOnlyList<PerformanceRecommendation>> GetRecommendationsAsync(CancellationToken cancellationToken = default);
}
