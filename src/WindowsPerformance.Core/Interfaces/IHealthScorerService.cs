namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IHealthScorerService
{
    Task<HealthScoreResult> CalculateScoreAsync(CancellationToken cancellationToken = default);
}
