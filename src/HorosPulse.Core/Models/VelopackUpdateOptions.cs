namespace HorosPulse.Core.Models;

/// <summary>Velopack auto-update configuration (see appsettings.json Velopack section).</summary>
public sealed class VelopackUpdateOptions
{
    /// <summary>
    /// GitHub releases feed URL for Velopack UpdateManager.
    /// Placeholder until first GitHub release with vpk artifacts is published.
    /// </summary>
    public string UpdateFeedUrl { get; set; } =
        "https://github.com/HorosCloudOfficial/HorosPulse";

    /// <summary>Check for updates in the background after startup.</summary>
    public bool CheckOnStartup { get; set; } = true;
}
