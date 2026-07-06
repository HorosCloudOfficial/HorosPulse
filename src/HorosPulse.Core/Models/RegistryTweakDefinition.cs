namespace HorosPulse.Core.Models;

public sealed class RegistryTweakDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Hive { get; init; } = "HKCU";
    public string KeyPath { get; init; } = string.Empty;
    public string ValueName { get; init; } = string.Empty;
    public int RecommendedValue { get; init; }
    public string DocumentationUrl { get; init; } = string.Empty;
}
