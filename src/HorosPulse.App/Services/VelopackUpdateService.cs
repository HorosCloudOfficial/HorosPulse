namespace HorosPulse.App.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Velopack;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class VelopackUpdateService : IVelopackUpdateService
{
    private readonly VelopackUpdateOptions _options;
    private readonly ILogger<VelopackUpdateService> _logger;
    private UpdateInfo? _pendingUpdate;

    public VelopackUpdateService(
        IOptions<VelopackUpdateOptions> options,
        ILogger<VelopackUpdateService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(
        bool downloadIfAvailable = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.UpdateFeedUrl))
        {
            _logger.LogDebug("Velopack update feed URL not configured.");
            return UpdateCheckResult.Skipped;
        }

        try
        {
            var mgr = new UpdateManager(_options.UpdateFeedUrl);
            var update = await mgr.CheckForUpdatesAsync();
            if (update is null)
                return UpdateCheckResult.UpToDate;

            _pendingUpdate = update;
            var version = update.TargetFullRelease.Version.ToString();

            if (downloadIfAvailable)
            {
                await mgr.DownloadUpdatesAsync(update);
                _logger.LogInformation("Velopack update {Version} downloaded.", version);
            }

            return UpdateCheckResult.Available(version);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Velopack update check failed.");
            return UpdateCheckResult.Failed(ex.Message);
        }
    }

    public Task<bool> ApplyDownloadedUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (_pendingUpdate is null)
            return Task.FromResult(false);

        try
        {
            var mgr = new UpdateManager(_options.UpdateFeedUrl);
            mgr.ApplyUpdatesAndRestart(_pendingUpdate);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Velopack apply update failed.");
            return Task.FromResult(false);
        }
    }
}
