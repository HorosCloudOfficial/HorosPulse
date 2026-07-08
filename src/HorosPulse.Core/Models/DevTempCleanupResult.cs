namespace HorosPulse.Core.Models;

/// <summary>Ergebnis einer Dev-Cache-Bereinigung.</summary>
public sealed class DevTempCleanupResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public long BytesFreed { get; init; }

    public IReadOnlyList<string> Messages { get; init; } = [];

    public string BytesFreedDisplay => DevTempCacheEntry.FormatBytes(BytesFreed);

    public static DevTempCleanupResult Ok(long bytesFreed, params string[] messages) => new()
    {
        Success = true,
        BytesFreed = bytesFreed,
        Messages = messages,
    };

    public static DevTempCleanupResult Fail(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
    };
}
