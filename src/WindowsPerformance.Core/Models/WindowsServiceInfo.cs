namespace WindowsPerformance.Core.Models;

public sealed class WindowsServiceInfo
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StartupType { get; init; } = string.Empty;
}
