namespace HorosPulse.Services.PowerShell;

using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Core.Scripts;

public sealed class PowerShellBridge : IPowerShellBridge
{
    private readonly IElevationService _elevationService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<PowerShellBridge> _logger;
    private readonly PowerShellOptions _options;
    private readonly string? _executablePath;

    public PowerShellBridge(
        IElevationService elevationService,
        IAuditLogger auditLogger,
        ILogger<PowerShellBridge> logger,
        PowerShellOptions? options = null)
    {
        _elevationService = elevationService;
        _auditLogger = auditLogger;
        _logger = logger;
        _options = options ?? new PowerShellOptions();
        _executablePath = ResolveExecutable(_options);
    }

    public bool IsPowerShellAvailable => _executablePath is not null;

    public string? PowerShellExecutable => _executablePath;

    public async Task<PowerShellResult> RunAsync(
        string script,
        bool elevated = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ScriptSanitizer.RequireSafeScript(script);
        var effectiveTimeout = timeout ?? _options.DefaultTimeout;
        var scriptHash = ScriptHashValidator.ComputeHash(script);
        var stopwatch = Stopwatch.StartNew();
        var module = elevated ? "PowerShellElevated" : "PowerShell";

        PowerShellResult result;
        try
        {
            result = elevated
                ? await _elevationService.RunElevatedScriptAsync(script, scriptHash, effectiveTimeout, cancellationToken)
                : _executablePath is null
                    ? PowerShellResult.Failed("PowerShell ist nicht verfügbar (pwsh.exe / powershell.exe fehlt).")
                    : await RunProcessAsync(_executablePath, script, effectiveTimeout, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PowerShell-Aufruf fehlgeschlagen");
            result = PowerShellResult.Failed(ex.Message);
        }

        stopwatch.Stop();
        await LogInvocationAsync(module, scriptHash, result, stopwatch.ElapsedMilliseconds, cancellationToken);
        return result;
    }

    private async Task LogInvocationAsync(
        string module,
        string scriptHash,
        PowerShellResult result,
        long durationMs,
        CancellationToken cancellationToken)
    {
        var details = $"Hash={scriptHash}; ExitCode={result.ExitCode}; DurationMs={durationMs}; Success={result.Success}";
        try
        {
            await _auditLogger.LogAsync("PowerShellInvoke", module, details, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit-Log für PowerShell-Aufruf fehlgeschlagen");
        }
    }

    internal static async Task<PowerShellResult> RunProcessAsync(
        string executablePath,
        string script,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = $"-NonInteractive -NoProfile -Command \"{EscapeArgument(script)}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Process may have exited between timeout and kill.
            }

            return PowerShellResult.Failed($"PowerShell-Timeout nach {timeout.TotalSeconds:F0}s.");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return new PowerShellResult(process.ExitCode, stdout, stderr, process.ExitCode == 0);
    }

    internal static string? ResolveExecutable(PowerShellOptions options)
    {
        var preferred = FindOnPath(options.PreferredExecutable);
        if (preferred is not null)
            return preferred;

        return FindOnPath(options.FallbackExecutable);
    }

    private static string? FindOnPath(string fileName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv))
            return null;

        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), fileName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static string EscapeArgument(string script) =>
        script.Replace("\"", "\\\"", StringComparison.Ordinal);
}
