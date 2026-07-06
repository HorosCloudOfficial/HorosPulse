namespace HorosPulse.Services.Elevation;

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Core.Scripts;

public sealed class ElevationService : IElevationService
{
    public const string PipeName = "HorosPulse.Elevation.v1";

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
        string scriptHash,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (!ScriptHashValidator.IsAllowed(script, scriptHash))
            return PowerShellResult.Failed("Skript-Hash ist nicht auf der Whitelist.");

        var helperPath = ElevationHelperPathResolver.GetExpectedPath();
        if (!File.Exists(helperPath))
            return PowerShellResult.Failed($"ElevationHelper nicht gefunden: {helperPath}");

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        await EnsureHelperServerRunningAsync(helperPath, cancellationToken);

        var request = new ElevationRequest
        {
            Script = Convert.ToBase64String(Encoding.UTF8.GetBytes(script)),
            ScriptHash = scriptHash,
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

            return new PowerShellResult(
                response.ExitCode,
                response.Stdout ?? string.Empty,
                response.Stderr ?? string.Empty,
                response.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ElevationHelper-Aufruf fehlgeschlagen");
            return PowerShellResult.Failed(ex.Message);
        }
    }

    public async Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default)
    {
        var helperPath = ElevationHelperPathResolver.GetExpectedPath();
        if (!File.Exists(helperPath))
            return OptimizationResult.Fail($"ElevationHelper nicht gefunden: {helperPath}");

        try
        {
            await EnsureHelperServerRunningAsync(helperPath, cancellationToken);

            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await client.ConnectAsync(5000, cancellationToken);

            await using var stream = client;
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            var request = new ElevationRequest { Operation = "purge-standby" };
            await writer.WriteLineAsync(JsonSerializer.Serialize(request, JsonOptions).AsMemory(), cancellationToken);

            var responseLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
                return OptimizationResult.Fail("Leere Antwort vom ElevationHelper.");

            var response = JsonSerializer.Deserialize<ElevationResponse>(responseLine, JsonOptions);
            if (response is null)
                return OptimizationResult.Fail("Ungültige JSON-Antwort vom ElevationHelper.");

            return response.Success
                ? OptimizationResult.Ok(response.Stdout ?? "Standby-Liste geleert")
                : OptimizationResult.Fail(response.Stderr ?? "Standby-Purge fehlgeschlagen");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Standby-Purge fehlgeschlagen");
            return OptimizationResult.Fail(ex.Message);
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
        public string Operation { get; init; } = "script";
        public string Script { get; init; } = string.Empty;
        public string ScriptHash { get; init; } = string.Empty;
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
