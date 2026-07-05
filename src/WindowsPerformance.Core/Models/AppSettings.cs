namespace WindowsPerformance.Core.Models;

public sealed class AppSettings
{
    public bool DefenderOptIn { get; set; }
    public IReadOnlyList<string> DefaultDevFolders { get; set; } =
    [
        "node_modules",
        ".git",
        "dist",
        "build",
    ];

    public int SnapshotRetentionLimit { get; set; } = 50;
    public int MetricsPollingIntervalMs { get; set; } = 2000;
}
