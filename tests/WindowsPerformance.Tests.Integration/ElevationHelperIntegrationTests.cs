namespace WindowsPerformance.Tests.Integration;

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using WindowsPerformance.Core.Scripts;
using Xunit;

public class ElevationHelperIntegrationTests
{
    [Fact]
    public async Task ElevationHelper_ReturnsJson_ForWhitelistedDummyScript()
    {
        var helperPath = ResolveElevationHelperPath();
        if (!File.Exists(helperPath))
        {
            await BuildElevationHelperAsync();
            helperPath = ResolveElevationHelperPath();
        }

        if (!File.Exists(helperPath))
            return;

        var script = PowerShellScriptLibrary.ElevationTestScript;
        var scriptBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(script));
        var hash = ScriptHashValidator.ComputeHash(script);

        var psi = new ProcessStartInfo
        {
            FileName = helperPath,
            Arguments = $"\"{scriptBase64}\" --hash {hash}",
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

        process.ExitCode.Should().Be(0, $"stderr: {await process.StandardError.ReadToEndAsync()}");

        using var document = JsonDocument.Parse(stdout.Trim());
        document.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("stdout").GetString().Should().Contain("ok");
    }

    [Fact]
    public async Task ElevationHelper_RejectsUnknownScriptHash()
    {
        var helperPath = ResolveElevationHelperPath();
        if (!File.Exists(helperPath))
            return;

        var script = "Write-Output 'blocked'";
        var scriptBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(script));

        var psi = new ProcessStartInfo
        {
            FileName = helperPath,
            Arguments = $"\"{scriptBase64}\" --hash INVALIDHASH",
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

        using var document = JsonDocument.Parse(stdout.Trim());
        document.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        document.RootElement.GetProperty("stderr").GetString().Should().Contain("Whitelist");
    }

    private static string ResolveElevationHelperPath() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "WindowsPerformance.Elevation", "bin", "Debug", "net9.0", "WindowsPerformance.Elevation.exe"));

    private static async Task BuildElevationHelperAsync()
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build src/WindowsPerformance.Elevation/WindowsPerformance.Elevation.csproj -c Debug",
            WorkingDirectory = solutionRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return;

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await process.WaitForExitAsync(cts.Token);
    }
}
