namespace HorosPulse.Core.Models;

public sealed record PowerShellResult(
    int ExitCode,
    string StdOut,
    string StdErr,
    bool Success)
{
    public static PowerShellResult Failed(string stderr, int exitCode = -1) =>
        new(exitCode, string.Empty, stderr, false);
}
