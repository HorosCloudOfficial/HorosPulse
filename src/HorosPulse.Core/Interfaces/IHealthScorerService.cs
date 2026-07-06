namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

public interface IHealthScorerService
{
    Task<HealthScoreResult> CalculateScoreAsync(CancellationToken cancellationToken = default);
}
