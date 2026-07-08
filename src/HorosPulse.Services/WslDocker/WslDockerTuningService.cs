namespace HorosPulse.Services.WslDocker;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class WslDockerTuningService : IWslDockerTuningService
{
    private readonly IPowerShellBridge _powerShellBridge;
    private readonly IBuildToolDefenderService _buildToolDefenderService;
    private readonly ILogger<WslDockerTuningService> _logger;
    private readonly string _wslConfigPath;
    private readonly string _trackingPath;
    private readonly string _backupDirectory;

    public WslDockerTuningService(
        IPowerShellBridge powerShellBridge,
        IBuildToolDefenderService buildToolDefenderService,
        ILogger<WslDockerTuningService> logger)
    {
        _powerShellBridge = powerShellBridge;
        _buildToolDefenderService = buildToolDefenderService;
        _logger = logger;
        _wslConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".wslconfig");
        _trackingPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "wsl-docker-tracking.json");
        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "backups");
    }

    public async Task<WslDockerTuningState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (systemRamMb, logicalProcessors) = GetSystemResources();
        var wslInfo = await DetectWslAsync(cancellationToken);
        var dockerInfo = DetectDocker(wslInfo);
        var wslConfigExists = File.Exists(_wslConfigPath);
        var wslConfigContent = wslConfigExists ? await File.ReadAllTextAsync(_wslConfigPath, cancellationToken) : null;
        var document = WslConfigParser.Parse(wslConfigContent);

        var currentLimits = WslConfigParser.ResolveEffectiveLimits(document, systemRamMb, logicalProcessors);
        var defaultLimits = WslConfigParser.CreateDefaultLimits(systemRamMb, logicalProcessors);
        var systemLimits = WslConfigParser.CreateSystemResourceLimits(systemRamMb, logicalProcessors);
        var recommended = WslDockerRecommendationEngine.ComputeRecommendedLimits(systemRamMb, logicalProcessors);

        var defenderState = await _buildToolDefenderService.GetStateAsync(cancellationToken);
        var dockerExcluded = defenderState.Entries.Any(e =>
            e.ProcessName.Equals("docker.exe", StringComparison.OrdinalIgnoreCase) && e.IsApplied);
        var wslExcluded = defenderState.Entries.Any(e =>
            e.ProcessName.Equals("wsl.exe", StringComparison.OrdinalIgnoreCase) && e.IsApplied);
        var containerEntries = defenderState.Entries.Where(e => e.Category == "Container").ToList();
        var containerApplied = containerEntries.Count(e => e.IsApplied);

        var recommendations = WslDockerRecommendationEngine.BuildRecommendations(
            currentLimits,
            recommended,
            wslInfo.IsInstalled,
            wslInfo.IsWsl2Default,
            dockerInfo.IsLikelyInUse,
            dockerExcluded,
            wslExcluded);

        var hasChanges = HasTrackedChanges(LoadTracking());
        var isOptimal = WslDockerRecommendationEngine.IsDevSetupOptimal(recommendations);
        var summary = WslDockerRecommendationEngine.BuildSummary(
            wslInfo.IsInstalled,
            isOptimal,
            hasChanges,
            recommended);

        return new WslDockerTuningState
        {
            IsWslInstalled = wslInfo.IsInstalled,
            IsWsl2Default = wslInfo.IsWsl2Default,
            WslStatusSummary = wslInfo.Summary,
            IsDockerDesktopInstalled = dockerInfo.IsInstalled,
            IsDockerDesktopRunning = dockerInfo.IsRunning,
            IsDockerWslBackendLikely = dockerInfo.IsWslBackendLikely,
            DockerStatusSummary = dockerInfo.Summary,
            WslConfigExists = wslConfigExists,
            WslConfigPath = _wslConfigPath,
            CurrentLimits = currentLimits,
            DefaultLimits = defaultLimits,
            SystemResources = systemLimits,
            RecommendedLimits = recommended,
            Recommendations = recommendations,
            HasHorosPulseChanges = hasChanges,
            IsDevSetupOptimal = isOptimal,
            RecommendationSummary = summary,
            BuildToolDefenderDockerExcluded = dockerExcluded,
            BuildToolDefenderWslExcluded = wslExcluded,
            BuildToolDefenderContainerAppliedCount = containerApplied,
            BuildToolDefenderContainerRecommendedCount = containerEntries.Count,
        };
    }

    public async Task<OptimizationResult> ApplyRecommendedConfigAsync(bool userConfirmed, CancellationToken cancellationToken = default)
    {
        if (!userConfirmed)
            return OptimizationResult.Fail("WSL2-Tuning erfordert ausdrückliche Zustimmung.");

        var state = await GetStateAsync(cancellationToken);
        if (!state.IsWslInstalled)
            return OptimizationResult.Fail("WSL ist nicht installiert — .wslconfig kann nicht angewendet werden.");

        var existingContent = state.WslConfigExists
            ? await File.ReadAllTextAsync(_wslConfigPath, cancellationToken)
            : null;

        if (LoadTracking() is not null)
            return OptimizationResult.Fail("Es existieren bereits HorosPulse-Änderungen. Bitte zuerst Rollback ausführen.");

        Directory.CreateDirectory(_backupDirectory);
        var backupPath = Path.Combine(
            _backupDirectory,
            $"wslconfig-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.bak");

        if (existingContent is not null)
            await File.WriteAllTextAsync(backupPath, existingContent, cancellationToken);

        var merged = WslConfigParser.MergeRecommendedSettings(existingContent, state.RecommendedLimits);
        await File.WriteAllTextAsync(_wslConfigPath, merged, cancellationToken);

        SaveTracking(new WslDockerTrackingData
        {
            PreviousFileExisted = state.WslConfigExists,
            PreviousWslConfigContent = existingContent,
            BackupFilePath = existingContent is not null ? backupPath : null,
            AppliedAt = DateTimeOffset.Now,
        });

        _logger.LogInformation("WSL2 .wslconfig aktualisiert: {Path}", _wslConfigPath);

        return OptimizationResult.Ok(
            ".wslconfig mit empfohlenen Dev-Limits geschrieben",
            $"Backup: {(existingContent is not null ? backupPath : "—")}",
            "Wichtig: wsl --shutdown ausführen, damit die Änderungen wirksam werden");
    }

    public async Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var tracking = LoadTracking();
        if (!HasTrackedChanges(tracking))
            return OptimizationResult.Fail("Keine von HorosPulse geänderte .wslconfig.");

        if (tracking!.PreviousFileExisted)
        {
            await File.WriteAllTextAsync(
                _wslConfigPath,
                tracking.PreviousWslConfigContent ?? string.Empty,
                cancellationToken);
        }
        else if (File.Exists(_wslConfigPath))
        {
            File.Delete(_wslConfigPath);
        }

        SaveTracking(null);
        _logger.LogInformation("WSL2 .wslconfig Rollback durchgeführt");

        return OptimizationResult.Ok(
            "Vorherige .wslconfig wiederhergestellt",
            "wsl --shutdown ausführen, damit die Änderungen wirksam werden");
    }

    public async Task<OptimizationResult> ShutdownWslAsync(CancellationToken cancellationToken = default)
    {
        if (!_powerShellBridge.IsPowerShellAvailable)
            return OptimizationResult.Fail("PowerShell nicht verfügbar.");

        var result = await _powerShellBridge.RunAsync(
            "wsl --shutdown 2>&1 | Out-String",
            elevated: false,
            cancellationToken: cancellationToken);

        if (!result.Success)
        {
            var message = string.IsNullOrWhiteSpace(result.StdErr) ? result.StdOut : result.StdErr;
            return OptimizationResult.Fail(
                string.IsNullOrWhiteSpace(message)
                    ? "wsl --shutdown fehlgeschlagen."
                    : message.Trim());
        }

        _logger.LogInformation("WSL heruntergefahren (wsl --shutdown)");
        return OptimizationResult.Ok("WSL wurde heruntergefahren. Distributionen und Docker können neu gestartet werden.");
    }

    internal static (long SystemRamMb, int LogicalProcessors) GetSystemResourcesFromMemory(long totalPhysBytes) =>
        ((long)(totalPhysBytes / 1024 / 1024), Environment.ProcessorCount);

    private static (long SystemRamMb, int LogicalProcessors) GetSystemResources()
    {
        var mem = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (GlobalMemoryStatusEx(ref mem))
            return ((long)(mem.TotalPhys / 1024 / 1024), Environment.ProcessorCount);

        return (0, Environment.ProcessorCount);
    }

    private async Task<WslDetectionInfo> DetectWslAsync(CancellationToken cancellationToken)
    {
        if (!_powerShellBridge.IsPowerShellAvailable)
            return WslDetectionInfo.NotAvailable("PowerShell nicht verfügbar — WSL-Status unbekannt");

        var versionResult = await _powerShellBridge.RunAsync(
            "wsl --version 2>&1 | Out-String",
            elevated: false,
            cancellationToken: cancellationToken);

        var statusResult = await _powerShellBridge.RunAsync(
            "wsl --status 2>&1 | Out-String",
            elevated: false,
            cancellationToken: cancellationToken);

        var combined = (versionResult.StdOut + statusResult.StdOut + versionResult.StdErr + statusResult.StdErr).Trim();
        if (string.IsNullOrWhiteSpace(combined))
            return WslDetectionInfo.NotAvailable("WSL nicht installiert oder nicht im PATH");

        var installed = !combined.Contains("not recognized", StringComparison.OrdinalIgnoreCase) &&
                        !combined.Contains("nicht erkannt", StringComparison.OrdinalIgnoreCase) &&
                        !combined.Contains("is not recognized", StringComparison.OrdinalIgnoreCase);

        if (!installed)
            return WslDetectionInfo.NotAvailable("WSL nicht installiert");

        var isWsl2Default = combined.Contains("Default Version: 2", StringComparison.OrdinalIgnoreCase) ||
                            combined.Contains("Standardversion: 2", StringComparison.OrdinalIgnoreCase) ||
                            combined.Contains("Standard-Version: 2", StringComparison.OrdinalIgnoreCase);

        var summary = statusResult.StdOut.Trim();
        if (string.IsNullOrWhiteSpace(summary))
            summary = versionResult.StdOut.Trim();
        if (string.IsNullOrWhiteSpace(summary))
            summary = installed ? "WSL installiert" : "WSL nicht erkannt";

        return new WslDetectionInfo(installed, isWsl2Default, summary);
    }

    private static DockerDetectionInfo DetectDocker(WslDetectionInfo wslInfo)
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var dockerExe = Path.Combine(programFiles, "Docker", "Docker", "Docker Desktop.exe");
        var isInstalled = File.Exists(dockerExe) ||
                          File.Exists(Path.Combine(programFiles, "Docker", "Docker", "resources", "com.docker.backend.exe"));

        var isRunning = Process.GetProcessesByName("com.docker.backend").Length > 0 ||
                        Process.GetProcessesByName("Docker Desktop").Length > 0;

        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Docker", "settings-store.json");

        var isWslBackend = false;
        if (File.Exists(settingsPath))
        {
            try
            {
                var json = File.ReadAllText(settingsPath);
                isWslBackend = json.Contains("wsl", StringComparison.OrdinalIgnoreCase) &&
                                 (json.Contains("\"UseWSL2\":true", StringComparison.OrdinalIgnoreCase) ||
                                  json.Contains("wslEngineEnabled", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                // settings-store optional / format varies
            }
        }

        if (!isWslBackend && wslInfo.IsInstalled)
            isWslBackend = true;

        var summary = isInstalled
            ? isRunning
                ? isWslBackend
                    ? "Docker Desktop läuft (WSL2-Backend wahrscheinlich)"
                    : "Docker Desktop läuft"
                : "Docker Desktop installiert, nicht gestartet"
            : "Docker Desktop nicht gefunden";

        return new DockerDetectionInfo(
            isInstalled,
            isRunning,
            isWslBackend,
            isInstalled || isRunning,
            summary);
    }

    private WslDockerTrackingData? LoadTracking()
    {
        if (!File.Exists(_trackingPath))
            return null;

        try
        {
            return JsonSerializer.Deserialize<WslDockerTrackingData>(File.ReadAllText(_trackingPath), JsonDefaults.Options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WSL-Docker-Tracking konnte nicht gelesen werden.");
            return null;
        }
    }

    private void SaveTracking(WslDockerTrackingData? tracking)
    {
        var dir = Path.GetDirectoryName(_trackingPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        if (tracking is null)
        {
            if (File.Exists(_trackingPath))
                File.Delete(_trackingPath);
            return;
        }

        File.WriteAllText(_trackingPath, JsonSerializer.Serialize(tracking, JsonDefaults.Options));
    }

    private static bool HasTrackedChanges(WslDockerTrackingData? tracking) => tracking is not null;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    private readonly record struct WslDetectionInfo(bool IsInstalled, bool IsWsl2Default, string Summary)
    {
        public static WslDetectionInfo NotAvailable(string summary) => new(false, false, summary);
    }

    private readonly record struct DockerDetectionInfo(
        bool IsInstalled,
        bool IsRunning,
        bool IsWslBackendLikely,
        bool IsLikelyInUse,
        string Summary);
}
