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

    private static readonly TimeSpan HelperStartupTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan PipeConnectTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PingResponseTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PurgeResponseTimeout = TimeSpan.FromSeconds(120);

    private readonly ILogger<ElevationService> _logger;
    private readonly IElevationUiInvoker _uiInvoker;
    private readonly SemaphoreSlim _launchLock = new(1, 1);

    public ElevationService(ILogger<ElevationService> logger, IElevationUiInvoker uiInvoker)
    {
        _logger = logger;
        _uiInvoker = uiInvoker;
    }

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
            return PowerShellResult.Failed($"HorosPulse.Elevation.exe nicht gefunden: {helperPath}");

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

            await client.ConnectAsync((int)PipeConnectTimeout.TotalMilliseconds, cancellationToken);

            await using var stream = client;
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            var requestJson = JsonSerializer.Serialize(request, JsonOptions);
            await writer.WriteLineAsync(requestJson.AsMemory(), cancellationToken);

            var responseLine = await ReadLineWithTimeoutAsync(
                reader,
                TimeSpan.FromMilliseconds(request.TimeoutMs) + TimeSpan.FromSeconds(10),
                cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
                return PowerShellResult.Failed("Leere Antwort von HorosPulse.Elevation.exe.");

            var response = JsonSerializer.Deserialize<ElevationResponse>(responseLine, JsonOptions);
            if (response is null)
                return PowerShellResult.Failed("Ungültige JSON-Antwort von HorosPulse.Elevation.exe.");

            return new PowerShellResult(
                response.ExitCode,
                response.Stdout ?? string.Empty,
                response.Stderr ?? string.Empty,
                response.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HorosPulse.Elevation.exe-Aufruf fehlgeschlagen");
            return PowerShellResult.Failed(FormatElevationError(ex));
        }
    }

    public async Task<OptimizationResult> PurgeMemoryAsync(
        MemoryPurgeOptions options,
        CancellationToken cancellationToken = default)
    {
        var helperPath = ElevationHelperPathResolver.GetExpectedPath();
        if (!File.Exists(helperPath))
            return OptimizationResult.Fail($"HorosPulse.Elevation.exe nicht gefunden: {helperPath}");

        var enabledAreas = options.GetEnabledAreas();
        if (enabledAreas.Count == 0)
            return OptimizationResult.Fail("Keine Speicherbereiche ausgewählt.");

        try
        {
            await EnsureHelperServerRunningAsync(helperPath, cancellationToken);

            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await client.ConnectAsync((int)PipeConnectTimeout.TotalMilliseconds, cancellationToken);

            await using var stream = client;
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            var request = new ElevationRequest
            {
                Operation = "purge-memory",
                PurgeAreas = enabledAreas.Select(area => (int)area).ToArray(),
            };
            await writer.WriteLineAsync(JsonSerializer.Serialize(request, JsonOptions).AsMemory(), cancellationToken);

            var responseLine = await ReadLineWithTimeoutAsync(reader, PurgeResponseTimeout, cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
                return OptimizationResult.Fail("Leere Antwort von HorosPulse.Elevation.exe.");

            var response = JsonSerializer.Deserialize<ElevationResponse>(responseLine, JsonOptions);
            if (response is null)
                return OptimizationResult.Fail("Ungültige JSON-Antwort von HorosPulse.Elevation.exe.");

            return response.Success
                ? OptimizationResult.Ok(response.Stdout ?? "Speicher bereinigt")
                : OptimizationResult.Fail(response.Stderr ?? "Speicher-Purge fehlgeschlagen");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Speicher-Purge fehlgeschlagen");
            return OptimizationResult.Fail(FormatElevationError(ex));
        }
    }

    public async Task<OptimizationResult> PurgeStandbyListAsync(CancellationToken cancellationToken = default) =>
        await PurgeMemoryAsync(new MemoryPurgeOptions { PurgeStandbyList = true }, cancellationToken);

    private async Task EnsureHelperServerRunningAsync(string helperPath, CancellationToken cancellationToken)
    {
        if (await TryPingHelperAsync(cancellationToken))
            return;

        await _launchLock.WaitAsync(cancellationToken);
        try
        {
            if (await TryPingHelperAsync(cancellationToken))
                return;

            await _uiInvoker.PrepareForUacPromptAsync(cancellationToken);

            var process = await _uiInvoker.InvokeAsync(
                () => TryLaunchHelperProcess(helperPath),
                cancellationToken);

            if (process is null)
            {
                throw new InvalidOperationException(
                    "UAC-Abfrage abgebrochen — Administratorrechte werden für diese Aktion benötigt.");
            }

            await Task.Delay(500, cancellationToken);
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"HorosPulse.Elevation.exe wurde sofort beendet (Exit-Code {process.ExitCode}). " +
                    "Stellen Sie sicher, dass HorosPulse.Elevation.dll neben der App liegt.");
            }

            var deadline = DateTime.UtcNow + HelperStartupTimeout;
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await TryPingHelperAsync(cancellationToken))
                    return;

                await Task.Delay(250, cancellationToken);
            }

            throw new InvalidOperationException(
                "HorosPulse.Elevation.exe antwortet nicht (Zeitüberschreitung nach UAC). " +
                "Der Elevation-Helper läuft möglicherweise nicht oder wurde blockiert.");
        }
        finally
        {
            _launchLock.Release();
        }
    }

    private static Process? TryLaunchHelperProcess(string helperPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = helperPath,
            Arguments = "--server",
            WorkingDirectory = Path.GetDirectoryName(helperPath) ?? AppContext.BaseDirectory,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        try
        {
            return Process.Start(psi);
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            return null;
        }
    }

    private static async Task<bool> TryPingHelperAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await client.ConnectAsync((int)PipeConnectTimeout.TotalMilliseconds, cancellationToken);

            await using var stream = client;
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            var request = new ElevationRequest { Operation = "ping" };
            await writer.WriteLineAsync(JsonSerializer.Serialize(request, JsonOptions).AsMemory(), cancellationToken);

            var responseLine = await ReadLineWithTimeoutAsync(reader, PingResponseTimeout, cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
                return false;

            var response = JsonSerializer.Deserialize<ElevationResponse>(responseLine, JsonOptions);
            return response is not null;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string?> ReadLineWithTimeoutAsync(
        StreamReader reader,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try
        {
            return await reader.ReadLineAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                "Zeitüberschreitung beim Warten auf HorosPulse.Elevation.exe. Bitte UAC bestätigen und erneut versuchen.");
        }
    }

    private static string FormatElevationError(Exception ex)
    {
        if (ex is TimeoutException)
            return ex.Message;

        if (ex.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase))
        {
            return "Zeitüberschreitung beim Verbinden mit HorosPulse.Elevation.exe. " +
                   "Bitte UAC bestätigen und erneut versuchen.";
        }

        return ex.Message;
    }

    private sealed class ElevationRequest
    {
        public string Operation { get; init; } = "script";
        public string Script { get; init; } = string.Empty;
        public string ScriptHash { get; init; } = string.Empty;
        public int TimeoutMs { get; init; } = 30000;
        public int[] PurgeAreas { get; init; } = [];
    }

    private sealed class ElevationResponse
    {
        public int ExitCode { get; init; }
        public string? Stdout { get; init; }
        public string? Stderr { get; init; }
        public bool Success { get; init; }
    }
}
