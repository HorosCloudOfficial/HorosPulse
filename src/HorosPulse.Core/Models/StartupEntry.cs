namespace HorosPulse.Core.Models;

public sealed class StartupEntry
{
    public required string Name { get; init; }
    public required string Command { get; init; }
    public required string Source { get; init; }
    public string? RegistryKey { get; init; }
    public string? RegistryValueName { get; init; }
    public bool IsEnabled { get; init; }
}
