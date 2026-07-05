namespace WindowsPerformance.Services.Elevation;

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsPerformance.Core.Interfaces;
using WindowsPerformance.Core.Models;

public sealed class ElevationService : IElevationService
{
    public const string PipeName = "WindowsPerformance.Elevation.v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<ElevationService> _logger;
    private readonly SemaphoreSlim _launchLock = new(1, 1);

    public ElevationService(ILogger<ElevationService> logger) => _logger = logger;

    public bool IsHelperAvailable => ElevationHelperPathResolver.IsHelperPresent();

    public async Task<PowerShellResult> RunElevatedScriptAsync(
        string script,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var helperPath = ElevationHelperPathResolver.GetExpectedPath();
        if (!File.Exists(helperPath))
            return PowerShellResult.Failed($"ElevationHelper nicht gefunden: {helperPath}");

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        await EnsureHelperServerRunningAsync(helperPath, cancellationToken);

        var request = new ElevationRequest
        {
            Script = Convert.ToBase64String(Encoding.UTF8.GetBytes(script)),
            TimeoutMs = (int)effectiveTimeout.TotalMilliseconds,
        };

        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await client.ConnectAsync(5000, cancellationToken);

            await using var stream = client;
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            var requestJson = JsonSerializer.Serialize(request, JsonOptions);
            await writer.WriteLineAsync(requestJson.AsMemory(), cancellationToken);

            var responseLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
                return PowerShellResult.Failed("Leere Antwort vom ElevationHelper.");

            var response = JsonSerializer.Deserialize<ElevationResponse>(responseLine, JsonOptions);
            if (response is null)
                return PowerShellResult.Failed("Ungültige JSON-Antwort vom ElevationHelper.");

            return new PowerShellResult(response.ExitCode, response.Stdout ?? string.Empty, response.Stderr ?? string.Empty, response.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ElevationHelper-Aufruf fehlgeschlagen");
            return PowerShellResult.Failed(ex.Message);
        }
    }

    private async Task EnsureHelperServerRunningAsync(string helperPath, CancellationToken cancellationToken)
    {
        if (await TryPingHelperAsync(cancellationToken))
            return;

        await _launchLock.WaitAsync(cancellationToken);
        try
        {
            if (await TryPingHelperAsync(cancellationToken))
                return;

            var psi = new ProcessStartInfo
            {
                FileName = helperPath,
                Arguments = "--server",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            Process.Start(psi);
            await Task.Delay(1500, cancellationToken);

            if (!await TryPingHelperAsync(cancellationToken))
                throw new InvalidOperationException("ElevationHelper konnte nicht gestartet werden (UAC abgebrochen?).");
        }
        finally
        {
            _launchLock.Release();
        }
    }

    private static async Task<bool> TryPingHelperAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await client.ConnectAsync(500, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class ElevationRequest
    {
        public string Script { get; init; } = string.Empty;
        public int TimeoutMs { get; init; } = 30000;
    }

    private sealed class ElevationResponse
    {
        public int ExitCode { get; init; }
        public string? Stdout { get; init; }
        public string? Stderr { get; init; }
        public bool Success { get; init; }
    }
}
