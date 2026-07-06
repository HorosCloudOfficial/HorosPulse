namespace HorosPulse.Core.Services;

using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

/// <summary>No-op update service for hosts without Velopack wiring.</summary>
public sealed class NoOpVelopackUpdateService : IVelopackUpdateService
{
    public Task<UpdateCheckResult> CheckForUpdatesAsync(
        bool downloadIfAvailable = false,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(UpdateCheckResult.Skipped);

    public Task<bool> ApplyDownloadedUpdateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
