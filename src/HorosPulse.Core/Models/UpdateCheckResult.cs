namespace HorosPulse.Core.Models;

public sealed record UpdateCheckResult(
    bool IsSkipped,
    bool IsUpToDate,
    string? AvailableVersion = null,
    string? ErrorMessage = null)
{
    public static UpdateCheckResult Skipped => new(true, false);

    public static UpdateCheckResult UpToDate => new(false, true);

    public static UpdateCheckResult Available(string version) => new(false, false, version);

    public static UpdateCheckResult Failed(string message) => new(false, false, null, message);
}
