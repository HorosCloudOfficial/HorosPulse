namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IProfileRepository
{
    Task<IReadOnlyList<ProfileDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProfileDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task SaveAsync(ProfileDefinition profile, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task EnsureDefaultPresetsAsync(CancellationToken cancellationToken = default);
}
