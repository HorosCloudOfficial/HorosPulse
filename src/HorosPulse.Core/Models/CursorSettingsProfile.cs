namespace HorosPulse.Core.Models;

public sealed class CursorSettingsProfile
{
    public IReadOnlyDictionary<string, object?> Settings { get; init; } =
        new Dictionary<string, object?>();

    public IReadOnlyList<string> ChangedKeys { get; init; } = Array.Empty<string>();

    public bool HasBackup { get; init; }
}
