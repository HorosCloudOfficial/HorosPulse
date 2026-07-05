namespace WindowsPerformance.Core.Interfaces;

using WindowsPerformance.Core.Models;

public interface IAppSettingsService
{
    AppSettings Current { get; }
    Task LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}
