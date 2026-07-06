namespace HorosPulse.Core.Interfaces;

using HorosPulse.Core.Models;

public interface IVelopackUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(
        bool downloadIfAvailable = false,
        CancellationToken cancellationToken = default);

    Task<bool> ApplyDownloadedUpdateAsync(CancellationToken cancellationToken = default);
}
