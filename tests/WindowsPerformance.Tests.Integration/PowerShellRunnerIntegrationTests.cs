namespace WindowsPerformance.Tests.Integration;

using System.Diagnostics;
using FluentAssertions;
using Xunit;

public class PowerShellRunnerIntegrationTests
{
    [Fact]
    public async Task Pwsh_ReturnsOutput_WhenAvailableOnCi()
    {
        var pwsh = FindPwsh();
        if (pwsh is null)
            return;

        var psi = new ProcessStartInfo
        {
            FileName = pwsh,
            Arguments = "-NoProfile -NonInteractive -Command \"Write-Output 'ci-ok'\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        process.Should().NotBeNull();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var stdout = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync(cts.Token);

        process.ExitCode.Should().Be(0);
        stdout.Trim().Should().Be("ci-ok");
    }

    private static string? FindPwsh()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv))
            return null;

        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), "pwsh.exe");
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }
}
