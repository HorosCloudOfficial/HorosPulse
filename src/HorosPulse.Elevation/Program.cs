using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using HorosPulse.Core.Scripts;

const string PipeName = "HorosPulse.Elevation.v1";
const int SystemMemoryListInformation = 0x50;
const int SystemFileCacheInformationEx = 0x54;
const int SystemRegistryReconciliationInformation = 0x9B;
const int SystemCombinePhysicalMemoryInformation = 0x82;

const int MemoryEmptyWorkingSets = 2;
const int MemoryFlushModifiedList = 3;
const int MemoryPurgeStandbyList = 4;
const int MemoryPurgeLowPriorityStandbyList = 5;

const ulong MaxSizeT = ulong.MaxValue;
const uint MemoryCombinePagesOnly = 1;

[DllImport("ntdll.dll")]
static extern int NtSetSystemInformation(int systemInformationClass, IntPtr systemInformation, int systemInformationLength);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

if (args.Length > 0 && string.Equals(args[0], "--server", StringComparison.OrdinalIgnoreCase))
{
    await RunServerAsync(jsonOptions);
    return 0;
}

if (args.Length > 0 && string.Equals(args[0], "--purge-standby", StringComparison.OrdinalIgnoreCase))
{
    var purgeResult = PurgeMemoryAreas([3]);
    var purgeJson = JsonSerializer.Serialize(purgeResult, jsonOptions);
    await Console.Out.WriteLineAsync(purgeJson);
    return purgeResult.Success ? 0 : 1;
}

if (args.Length == 0)
{
    await Console.Error.WriteLineAsync("Verwendung: HorosPulse.Elevation.exe --server | <Base64Script> [--hash <sha256>] [--timeout <ms>]");
    return 1;
}

var scriptBase64 = args[0];
var scriptHash = string.Empty;
var timeoutMs = 30000;
for (var i = 1; i < args.Length; i++)
{
    if (string.Equals(args[i], "--timeout", StringComparison.OrdinalIgnoreCase) &&
        i + 1 < args.Length &&
        int.TryParse(args[i + 1], out var parsedTimeout))
    {
        timeoutMs = parsedTimeout;
        i++;
        continue;
    }

    if (string.Equals(args[i], "--hash", StringComparison.OrdinalIgnoreCase) &&
        i + 1 < args.Length)
    {
        scriptHash = args[i + 1];
        i++;
    }
}

var result = await ExecuteScriptAsync(scriptBase64, scriptHash, timeoutMs);
var json = JsonSerializer.Serialize(result, jsonOptions);
await Console.Out.WriteLineAsync(json);
return result.Success ? 0 : result.ExitCode == 0 ? 1 : result.ExitCode;

static async Task RunServerAsync(JsonSerializerOptions jsonOptions)
{
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    while (!cts.Token.IsCancellationRequested)
    {
        await using var server = new NamedPipeServerStream(
            PipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        try
        {
            await server.WaitForConnectionAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }

        using var reader = new StreamReader(server, Encoding.UTF8, leaveOpen: true);
        using var writer = new StreamWriter(server, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

        var line = await reader.ReadLineAsync(cts.Token);
        if (string.IsNullOrWhiteSpace(line))
            continue;

        ElevationRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<ElevationRequest>(line, jsonOptions);
        }
        catch
        {
            var error = new ElevationResponse { ExitCode = -1, Stderr = "Ungültige Anfrage.", Success = false };
            await writer.WriteLineAsync(JsonSerializer.Serialize(error, jsonOptions));
            continue;
        }

        ElevationResponse response;
        if (string.Equals(request?.Operation, "ping", StringComparison.OrdinalIgnoreCase))
            response = new ElevationResponse { ExitCode = 0, Stdout = "pong", Success = true };
        else if (string.Equals(request?.Operation, "purge-standby", StringComparison.OrdinalIgnoreCase))
            response = PurgeMemoryAreas([3]);
        else if (string.Equals(request?.Operation, "purge-memory", StringComparison.OrdinalIgnoreCase))
            response = PurgeMemoryAreas(request?.PurgeAreas ?? []);
        else
            response = await ExecuteScriptAsync(request?.Script ?? string.Empty, request?.ScriptHash ?? string.Empty, request?.TimeoutMs ?? 30000);

        await writer.WriteLineAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

static async Task<ElevationResponse> ExecuteScriptAsync(string scriptBase64, string scriptHash, int timeoutMs)
{
    if (string.IsNullOrWhiteSpace(scriptBase64))
        return new ElevationResponse { ExitCode = -1, Stderr = "Leeres Skript.", Success = false };

    string script;
    try
    {
        script = Encoding.UTF8.GetString(Convert.FromBase64String(scriptBase64));
    }
    catch
    {
        return new ElevationResponse { ExitCode = -1, Stderr = "Base64-Dekodierung fehlgeschlagen.", Success = false };
    }

    if (!ScriptHashValidator.IsAllowed(script, scriptHash))
        return new ElevationResponse { ExitCode = -1, Stderr = "Skript-Hash nicht auf der Whitelist.", Success = false };

    var pwsh = FindPowerShell();
    if (pwsh is null)
        return new ElevationResponse { ExitCode = -1, Stderr = "pwsh.exe / powershell.exe nicht gefunden.", Success = false };

    var psi = new ProcessStartInfo
    {
        FileName = pwsh,
        Arguments = $"-NonInteractive -NoProfile -Command \"{script.Replace("\"", "\\\"", StringComparison.Ordinal)}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8,
    };

    using var process = new Process { StartInfo = psi };
    process.Start();

    var stdoutTask = process.StandardOutput.ReadToEndAsync();
    var stderrTask = process.StandardError.ReadToEndAsync();

    using var cts = new CancellationTokenSource(timeoutMs);
    try
    {
        await process.WaitForExitAsync(cts.Token);
    }
    catch (OperationCanceledException)
    {
        try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
        return new ElevationResponse { ExitCode = -1, Stderr = "Timeout.", Success = false };
    }

    return new ElevationResponse
    {
        ExitCode = process.ExitCode,
        Stdout = await stdoutTask,
        Stderr = await stderrTask,
        Success = process.ExitCode == 0,
    };
}

static string? FindPowerShell()
{
    foreach (var name in new[] { "pwsh.exe", "powershell.exe" })
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv))
            continue;

        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), name);
            if (File.Exists(candidate))
                return candidate;
        }
    }

    return null;
}

static ElevationResponse PurgeMemoryAreas(IReadOnlyList<int> areas)
{
    if (areas.Count == 0)
        return new ElevationResponse { ExitCode = -1, Stderr = "Keine Speicherbereiche angegeben.", Success = false };

    var messages = new List<string>();
    foreach (var area in areas)
    {
        var result = area switch
        {
            0 => PurgeMemoryListCommand(MemoryEmptyWorkingSets, "Working Sets"),
            1 => PurgeSystemFileCache(),
            2 => PurgeMemoryListCommand(MemoryFlushModifiedList, "Modified Page List"),
            3 => PurgeMemoryListCommand(MemoryPurgeStandbyList, "Standby-Liste"),
            4 => PurgeMemoryListCommand(MemoryPurgeLowPriorityStandbyList, "Standby Priority-0"),
            5 => PurgeRegistryCache(),
            6 => PurgeCombineMemoryLists(),
            _ => new PurgeStepResult(false, $"Unbekannter Bereich: {area}"),
        };

        if (!result.Success)
            return new ElevationResponse { ExitCode = -1, Stderr = result.Message, Success = false };

        messages.Add(result.Message);
    }

    return new ElevationResponse
    {
        ExitCode = 0,
        Stdout = string.Join("; ", messages),
        Success = true,
    };
}

static PurgeStepResult PurgeMemoryListCommand(int command, string label)
{
    try
    {
        var size = sizeof(int);
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.WriteInt32(ptr, command);
            var status = NtSetSystemInformation(SystemMemoryListInformation, ptr, size);
            return status == 0
                ? new PurgeStepResult(true, $"{label} geleert")
                : new PurgeStepResult(false, $"{label}: NtSetSystemInformation 0x{status:X8}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    catch (Exception ex)
    {
        return new PurgeStepResult(false, $"{label}: {ex.Message}");
    }
}

static PurgeStepResult PurgeSystemFileCache()
{
    try
    {
        var info = new SystemFileCacheInformation
        {
            MinimumWorkingSet = MaxSizeT,
            MaximumWorkingSet = MaxSizeT,
        };

        var size = Marshal.SizeOf<SystemFileCacheInformation>();
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, ptr, false);
            var status = NtSetSystemInformation(SystemFileCacheInformationEx, ptr, size);
            return status == 0
                ? new PurgeStepResult(true, "System File Cache geleert")
                : new PurgeStepResult(false, $"System File Cache: NtSetSystemInformation 0x{status:X8}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    catch (Exception ex)
    {
        return new PurgeStepResult(false, $"System File Cache: {ex.Message}");
    }
}

static PurgeStepResult PurgeRegistryCache()
{
    try
    {
        var status = NtSetSystemInformation(SystemRegistryReconciliationInformation, IntPtr.Zero, 0);
        return status == 0
            ? new PurgeStepResult(true, "Registry-Cache geleert")
            : new PurgeStepResult(false, $"Registry-Cache: NtSetSystemInformation 0x{status:X8}");
    }
    catch (Exception ex)
    {
        return new PurgeStepResult(false, $"Registry-Cache: {ex.Message}");
    }
}

static PurgeStepResult PurgeCombineMemoryLists()
{
    try
    {
        var info = new MemoryCombineInformationEx { Flags = MemoryCombinePagesOnly };
        var size = Marshal.SizeOf<MemoryCombineInformationEx>();
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, ptr, false);
            var status = NtSetSystemInformation(SystemCombinePhysicalMemoryInformation, ptr, size);
            return status == 0
                ? new PurgeStepResult(true, "Speicherlisten kombiniert")
                : new PurgeStepResult(false, $"Combine Memory: NtSetSystemInformation 0x{status:X8}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    catch (Exception ex)
    {
        return new PurgeStepResult(false, $"Combine Memory: {ex.Message}");
    }
}

file readonly record struct PurgeStepResult(bool Success, string Message);

[StructLayout(LayoutKind.Sequential)]
file struct SystemFileCacheInformation
{
    public ulong CurrentSize;
    public ulong PeakSize;
    public uint PageFaultCount;
    public ulong MinimumWorkingSet;
    public ulong MaximumWorkingSet;
    public ulong CurrentSizeIncludingTransitionInPages;
    public ulong PeakSizeIncludingTransitionInPages;
    public uint TransitionRePurposeCount;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
file struct MemoryCombineInformationEx
{
    public IntPtr Section;
    public UIntPtr PagesCombined;
    public uint Flags;
}

file sealed class ElevationRequest
{
    public string Operation { get; init; } = "script";
    public string Script { get; init; } = string.Empty;
    public string ScriptHash { get; init; } = string.Empty;
    public int TimeoutMs { get; init; } = 30000;
    public int[] PurgeAreas { get; init; } = [];
}

file sealed class ElevationResponse
{
    public int ExitCode { get; init; }
    public string? Stdout { get; init; }
    public string? Stderr { get; init; }
    public bool Success { get; init; }
}
