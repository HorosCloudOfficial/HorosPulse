namespace HorosPulse.Services.CodingBoost;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class CodingBoostService : ICodingBoostService
{
    private const string GameBarKeyPath = @"Software\Microsoft\GameBar";
    private const string GraphicsDriversKeyPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
    private const string DirectXUserGpuPreferencesPath = @"Software\Microsoft\DirectX\UserGpuPreferences";
    private const string DirectXGraphicsSettingsPath = @"Software\Microsoft\DirectX\GraphicsSettings";
    private const string AutoGameModeEnabledValue = "AutoGameModeEnabled";
    private const string AllowAutoGameModeValue = "AllowAutoGameMode";
    private const string HwSchModeValue = "HwSchMode";
    private const string DirectXUserGlobalSettingsValue = "DirectXUserGlobalSettings";
    private const string SwapEffectUpgradeCacheValue = "SwapEffectUpgradeCache";

    private const int HagsEnabled = 2;
    private const int HagsDisabled = 1;

    private readonly IPowerShellBridge _powerShellBridge;
    private readonly ILogger<CodingBoostService> _logger;
    private readonly string _trackingPath;

    public CodingBoostService(IPowerShellBridge powerShellBridge, ILogger<CodingBoostService> logger)
    {
        _powerShellBridge = powerShellBridge;
        _logger = logger;
        _trackingPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "coding-boost-tracking.json");
    }

    public Task<CodingBoostState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isWindows11 = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
        var gameModeEnabled = IsGameModeEnabled();
        var hagsEnabled = IsHagsEnabled();
        var directXSettings = ReadRegistryString(Registry.CurrentUser, DirectXUserGpuPreferencesPath, DirectXUserGlobalSettingsValue);
        var swapCache = ReadRegistryDword(Registry.CurrentUser, DirectXGraphicsSettingsPath, SwapEffectUpgradeCacheValue);
        var windowedEnabled = CodingBoostDirectXSettingsHelper.IsWindowedOptimizationEnabled(directXSettings, swapCache);
        var tracking = LoadTracking();
        var hasChanges = HasTrackedChanges(tracking);

        var gameMode = new CodingBoostSettingStatus
        {
            Name = "Game Mode",
            Description = "Priorisiert die Vordergrund-App (IDE, Browser, GPU-Tools) — Microsoft: HKCU\\Software\\Microsoft\\GameBar\\AutoGameModeEnabled.",
            IsEnabled = gameModeEnabled,
            IsRecommended = true,
            RequiresReboot = false,
            DetailText = FormatGameModeDetail(),
        };

        var hags = new CodingBoostSettingStatus
        {
            Name = "Hardware-beschleunigte GPU-Planung (HAGS)",
            Description = "GPU verwaltet VRAM-Scheduling selbst — reduziert Latenz bei GPU-lastigen Apps (WDDM 2.7+, HKLM\\GraphicsDrivers\\HwSchMode=2).",
            IsEnabled = hagsEnabled,
            IsRecommended = true,
            RequiresReboot = true,
            DetailText = FormatHagsDetail(),
        };

        var windowed = new CodingBoostSettingStatus
        {
            Name = "Optimierungen für Fensterspiele",
            Description = isWindows11
                ? "Flip-Model für fensterbasierte DirectX-Apps — weniger Latenz bei GPU-Vorschau/Emulatoren (Win11, DirectXUserGlobalSettings)."
                : "Nur unter Windows 11 verfügbar.",
            IsEnabled = windowedEnabled,
            IsRecommended = isWindows11,
            IsSupported = isWindows11,
            RequiresReboot = false,
            DetailText = isWindows11 ? FormatWindowedDetail(directXSettings, swapCache) : "Windows 11 erforderlich",
        };

        var isOptimal = gameModeEnabled && hagsEnabled && (!isWindows11 || windowedEnabled);
        var recommendation = BuildRecommendationSummary(gameModeEnabled, hagsEnabled, windowedEnabled, isWindows11, hasChanges);

        return Task.FromResult(new CodingBoostState
        {
            GameMode = gameMode,
            HardwareAcceleratedGpuScheduling = hags,
            WindowedGameOptimization = windowed,
            IsWindows11OrNewer = isWindows11,
            HasHorosPulseChanges = hasChanges,
            IsDevSetupOptimal = isOptimal,
            RecommendationSummary = recommendation,
        });
    }

    public async Task<OptimizationResult> ApplyDevSetupAsync(bool userConfirmed, CancellationToken cancellationToken = default)
    {
        if (!userConfirmed)
            return OptimizationResult.Fail("Coding-Boost erfordert ausdrückliche Zustimmung.");

        var changes = new List<string>();
        var tracking = LoadTracking() ?? new CodingBoostTrackingData();
        var isWindows11 = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

        if (!IsGameModeEnabled())
        {
            var previousAuto = ReadRegistryDword(Registry.CurrentUser, GameBarKeyPath, AutoGameModeEnabledValue);
            var previousAllow = ReadRegistryDword(Registry.CurrentUser, GameBarKeyPath, AllowAutoGameModeValue);

            if (tracking.GameMode is null)
            {
                tracking.GameMode = new CodingBoostGameModeTracking
                {
                    AutoGameModeEnabled = previousAuto,
                    AllowAutoGameMode = previousAllow,
                };
            }

            SetRegistryDword(Registry.CurrentUser, GameBarKeyPath, AutoGameModeEnabledValue, 1);
            SetRegistryDword(Registry.CurrentUser, GameBarKeyPath, AllowAutoGameModeValue, 1);
            changes.Add("Game Mode aktiviert");
            _logger.LogInformation("Coding-Boost: Game Mode aktiviert");
        }

        if (!IsHagsEnabled())
        {
            var previousHags = ReadRegistryDword(Registry.LocalMachine, GraphicsDriversKeyPath, HwSchModeValue);
            var valueExisted = previousHags is not null;

            if (tracking.Hags is null)
            {
                tracking.Hags = new CodingBoostHagsTracking
                {
                    HwSchMode = previousHags,
                    ValueExisted = valueExisted,
                };
            }

            var hagsResult = await SetHagsModeAsync(HagsEnabled, cancellationToken);
            if (!hagsResult.Success)
                return OptimizationResult.Fail(hagsResult.ErrorMessage ?? "HAGS konnte nicht gesetzt werden.");

            changes.Add("HAGS aktiviert (Neustart empfohlen)");
            _logger.LogInformation("Coding-Boost: HAGS aktiviert");
        }

        if (isWindows11)
        {
            var directXSettings = ReadRegistryString(Registry.CurrentUser, DirectXUserGpuPreferencesPath, DirectXUserGlobalSettingsValue);
            var swapCache = ReadRegistryDword(Registry.CurrentUser, DirectXGraphicsSettingsPath, SwapEffectUpgradeCacheValue);
            var windowedEnabled = CodingBoostDirectXSettingsHelper.IsWindowedOptimizationEnabled(directXSettings, swapCache);

            if (!windowedEnabled)
            {
                if (tracking.WindowedOptimization is null)
                {
                    tracking.WindowedOptimization = new CodingBoostWindowedOptimizationTracking
                    {
                        DirectXUserGlobalSettings = directXSettings,
                        DirectXSettingsExisted = directXSettings is not null,
                        SwapEffectUpgradeCache = swapCache,
                        SwapEffectCacheExisted = swapCache is not null,
                    };
                }

                var merged = CodingBoostDirectXSettingsHelper.SetSetting(
                    directXSettings,
                    CodingBoostDirectXSettingsHelper.SwapEffectUpgradeKey,
                    "1");
                SetRegistryString(Registry.CurrentUser, DirectXUserGpuPreferencesPath, DirectXUserGlobalSettingsValue, merged);
                SetRegistryDword(Registry.CurrentUser, DirectXGraphicsSettingsPath, SwapEffectUpgradeCacheValue, 1);
                changes.Add("Fenster-Optimierung aktiviert");
                _logger.LogInformation("Coding-Boost: Fenster-Optimierung aktiviert");
            }
        }

        if (changes.Count == 0)
            return OptimizationResult.Ok("Dev-Setup ist bereits optimal konfiguriert.");

        SaveTracking(tracking);
        return OptimizationResult.Ok(changes.ToArray());
    }

    public async Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var tracking = LoadTracking();
        if (!HasTrackedChanges(tracking))
            return OptimizationResult.Fail("Keine von HorosPulse gesetzten Coding-Boost-Werte.");

        var restored = new List<string>();

        if (tracking!.GameMode is not null)
        {
            RestoreGameMode(tracking.GameMode);
            restored.Add("Game Mode zurückgesetzt");
        }

        if (tracking.Hags is not null)
        {
            int? value = tracking.Hags.ValueExisted
                ? (int?)(tracking.Hags.HwSchMode ?? HagsDisabled)
                : null;
            var hagsResult = value is null
                ? await DeleteHagsModeAsync(cancellationToken)
                : await SetHagsModeAsync(value.Value, cancellationToken);

            if (!hagsResult.Success)
                return OptimizationResult.Fail(hagsResult.ErrorMessage ?? "HAGS-Rollback fehlgeschlagen.");

            restored.Add("HAGS zurückgesetzt (Neustart empfohlen)");
        }

        if (tracking.WindowedOptimization is not null)
        {
            RestoreWindowedOptimization(tracking.WindowedOptimization);
            restored.Add("Fenster-Optimierung zurückgesetzt");
        }

        SaveTracking(null);
        return OptimizationResult.Ok(restored.ToArray());
    }

    internal static bool IsGameModeEnabled(int? autoGameModeEnabled, int? allowAutoGameMode) =>
        autoGameModeEnabled == 1 || (autoGameModeEnabled is null && allowAutoGameMode == 1);

    internal static bool IsHagsEnabled(int? hwSchMode) => hwSchMode == HagsEnabled;

    private bool IsGameModeEnabled()
    {
        var auto = ReadRegistryDword(Registry.CurrentUser, GameBarKeyPath, AutoGameModeEnabledValue);
        var allow = ReadRegistryDword(Registry.CurrentUser, GameBarKeyPath, AllowAutoGameModeValue);
        return IsGameModeEnabled(auto, allow);
    }

    private bool IsHagsEnabled()
    {
        var mode = ReadRegistryDword(Registry.LocalMachine, GraphicsDriversKeyPath, HwSchModeValue);
        return IsHagsEnabled(mode);
    }

    private void RestoreGameMode(CodingBoostGameModeTracking tracking)
    {
        if (tracking.AutoGameModeEnabled is null)
            DeleteRegistryValue(Registry.CurrentUser, GameBarKeyPath, AutoGameModeEnabledValue);
        else
            SetRegistryDword(Registry.CurrentUser, GameBarKeyPath, AutoGameModeEnabledValue, tracking.AutoGameModeEnabled.Value);

        if (tracking.AllowAutoGameMode is null)
            DeleteRegistryValue(Registry.CurrentUser, GameBarKeyPath, AllowAutoGameModeValue);
        else
            SetRegistryDword(Registry.CurrentUser, GameBarKeyPath, AllowAutoGameModeValue, tracking.AllowAutoGameMode.Value);
    }

    private void RestoreWindowedOptimization(CodingBoostWindowedOptimizationTracking tracking)
    {
        if (!tracking.DirectXSettingsExisted || tracking.DirectXUserGlobalSettings is null)
            DeleteRegistryValue(Registry.CurrentUser, DirectXUserGpuPreferencesPath, DirectXUserGlobalSettingsValue);
        else
            SetRegistryString(
                Registry.CurrentUser,
                DirectXUserGpuPreferencesPath,
                DirectXUserGlobalSettingsValue,
                tracking.DirectXUserGlobalSettings);

        if (!tracking.SwapEffectCacheExisted || tracking.SwapEffectUpgradeCache is null)
            DeleteRegistryValue(Registry.CurrentUser, DirectXGraphicsSettingsPath, SwapEffectUpgradeCacheValue);
        else
            SetRegistryDword(
                Registry.CurrentUser,
                DirectXGraphicsSettingsPath,
                SwapEffectUpgradeCacheValue,
                tracking.SwapEffectUpgradeCache.Value);
    }

    private const string HagsPowerShellPath = @"HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers";

    private async Task<OptimizationResult> SetHagsModeAsync(int value, CancellationToken cancellationToken)
    {
        var script =
            "$path = '" + HagsPowerShellPath + "'\n" +
            "if (-not (Test-Path -LiteralPath $path)) {\n" +
            "    New-Item -Path $path -Force | Out-Null\n" +
            "}\n" +
            "Set-ItemProperty -LiteralPath $path -Name '" + HwSchModeValue + "' -Value " + value + " -Type DWord";

        var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);
        return result.Success
            ? OptimizationResult.Ok()
            : OptimizationResult.Fail(result.StdErr.Trim().Length > 0 ? result.StdErr.Trim() : "HAGS-Registry-Zugriff fehlgeschlagen.");
    }

    private async Task<OptimizationResult> DeleteHagsModeAsync(CancellationToken cancellationToken)
    {
        var script =
            "$path = '" + HagsPowerShellPath + "'\n" +
            "if (Test-Path -LiteralPath $path) {\n" +
            "    Remove-ItemProperty -LiteralPath $path -Name '" + HwSchModeValue + "' -ErrorAction SilentlyContinue\n" +
            "}";

        var result = await _powerShellBridge.RunAsync(script, elevated: true, cancellationToken: cancellationToken);
        return result.Success
            ? OptimizationResult.Ok()
            : OptimizationResult.Fail(result.StdErr.Trim().Length > 0 ? result.StdErr.Trim() : "HAGS-Rollback fehlgeschlagen.");
    }

    private static string BuildRecommendationSummary(
        bool gameMode,
        bool hags,
        bool windowed,
        bool isWindows11,
        bool hasHorosPulseChanges)
    {
        if (gameMode && hags && (!isWindows11 || windowed))
            return "Dev-Setup optimal — Game Mode, HAGS" + (isWindows11 ? " und Fenster-Optimierung" : "") + " aktiv.";

        var missing = new List<string>();
        if (!gameMode) missing.Add("Game Mode");
        if (!hags) missing.Add("HAGS");
        if (isWindows11 && !windowed) missing.Add("Fenster-Optimierung");

        var suffix = hasHorosPulseChanges ? " · HorosPulse-Änderungen können per Rollback zurückgesetzt werden." : string.Empty;
        return $"Empfehlung für Dev-Workflow: {string.Join(", ", missing)} aktivieren.{suffix}";
    }

    private string FormatGameModeDetail()
    {
        var auto = ReadRegistryDword(Registry.CurrentUser, GameBarKeyPath, AutoGameModeEnabledValue);
        var allow = ReadRegistryDword(Registry.CurrentUser, GameBarKeyPath, AllowAutoGameModeValue);
        return $"AutoGameModeEnabled={FormatDword(auto)}, AllowAutoGameMode={FormatDword(allow)}";
    }

    private string FormatHagsDetail()
    {
        var mode = ReadRegistryDword(Registry.LocalMachine, GraphicsDriversKeyPath, HwSchModeValue);
        return mode switch
        {
            2 => "HwSchMode=2 (aktiv)",
            1 => "HwSchMode=1 (aus)",
            0 => "HwSchMode=0 (Systemstandard)",
            null => "HwSchMode nicht gesetzt",
            _ => $"HwSchMode={mode}",
        };
    }

    private static string FormatWindowedDetail(string? directXSettings, int? swapCache) =>
        $"SwapEffectUpgradeEnable={(CodingBoostDirectXSettingsHelper.IsSettingEnabled(directXSettings, CodingBoostDirectXSettingsHelper.SwapEffectUpgradeKey) ? "1" : "0")}, " +
        $"SwapEffectUpgradeCache={FormatDword(swapCache)}";

    private static string FormatDword(int? value) => value?.ToString() ?? "—";

    private CodingBoostTrackingData? LoadTracking()
    {
        if (!File.Exists(_trackingPath))
            return null;

        try
        {
            return JsonSerializer.Deserialize<CodingBoostTrackingData>(File.ReadAllText(_trackingPath), JsonDefaults.Options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Coding-Boost-Tracking konnte nicht gelesen werden.");
            return null;
        }
    }

    private void SaveTracking(CodingBoostTrackingData? tracking)
    {
        var dir = Path.GetDirectoryName(_trackingPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        if (tracking is null || !HasTrackedChanges(tracking))
        {
            if (File.Exists(_trackingPath))
                File.Delete(_trackingPath);
            return;
        }

        File.WriteAllText(_trackingPath, JsonSerializer.Serialize(tracking, JsonDefaults.Options));
    }

    private static bool HasTrackedChanges(CodingBoostTrackingData? tracking) =>
        tracking is not null &&
        (tracking.GameMode is not null || tracking.Hags is not null || tracking.WindowedOptimization is not null);

    private static int? ReadRegistryDword(RegistryKey hive, string subKey, string valueName)
    {
        using var key = hive.OpenSubKey(subKey);
        var value = key?.GetValue(valueName);
        return value switch
        {
            int i => i,
            null => null,
            _ => Convert.ToInt32(value),
        };
    }

    private static string? ReadRegistryString(RegistryKey hive, string subKey, string valueName)
    {
        using var key = hive.OpenSubKey(subKey);
        return key?.GetValue(valueName) as string;
    }

    private static void SetRegistryDword(RegistryKey hive, string subKey, string valueName, int value)
    {
        using var key = hive.CreateSubKey(subKey, true)
            ?? throw new InvalidOperationException($"Registry-Schlüssel nicht beschreibbar: {subKey}");
        key.SetValue(valueName, value, RegistryValueKind.DWord);
    }

    private static void SetRegistryString(RegistryKey hive, string subKey, string valueName, string value)
    {
        using var key = hive.CreateSubKey(subKey, true)
            ?? throw new InvalidOperationException($"Registry-Schlüssel nicht beschreibbar: {subKey}");
        key.SetValue(valueName, value, RegistryValueKind.String);
    }

    private static void DeleteRegistryValue(RegistryKey hive, string subKey, string valueName)
    {
        using var key = hive.OpenSubKey(subKey, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
