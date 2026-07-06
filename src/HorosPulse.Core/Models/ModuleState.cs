namespace HorosPulse.Core.Models;

public sealed record ModuleState(
    string ModuleName,
    bool IsEnabled,
    IReadOnlyDictionary<string, string> Settings);
