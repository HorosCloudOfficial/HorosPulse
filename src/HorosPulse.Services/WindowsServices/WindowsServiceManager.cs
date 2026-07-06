namespace HorosPulse.Services.WindowsServices;

using System.ServiceProcess;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class WindowsServiceManager : IWindowsServiceManager
{
    private static readonly (string Name, ServiceStartMode Mode)[] DevPresetRules =
    [
        ("SysMain", ServiceStartMode.Manual),
        ("WSearch", ServiceStartMode.Manual),
        ("DiagTrack", ServiceStartMode.Manual),
    ];

    private readonly ILogger<WindowsServiceManager> _logger;
    private readonly string _snapshotPath;
    private Dictionary<string, ServiceStartMode>? _savedStartupTypes;

    public WindowsServiceManager(ILogger<WindowsServiceManager> logger)
    {
        _logger = logger;
        _snapshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "service-startup-types.json");
    }

    public Task<IReadOnlyList<WindowsServiceInfo>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var services = ServiceController.GetServices()
            .Select(sc =>
            {
                using (sc)
                {
                    return new WindowsServiceInfo
                    {
                        Name = sc.ServiceName,
                        DisplayName = sc.DisplayName,
                        Status = sc.Status.ToString(),
                        StartupType = GetStartupType(sc.ServiceName),
                    };
                }
            })
            .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<WindowsServiceInfo>>(services);
    }

    public Task<OptimizationResult> SetStartupTypeAsync(
        string name,
        ServiceStartMode mode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _savedStartupTypes ??= LoadSnapshot();

        try
        {
            using var sc = new ServiceController(name);
            if (!_savedStartupTypes.ContainsKey(name))
                _savedStartupTypes[name] = GetStartupTypeEnum(sc.ServiceName);

            SetServiceStartMode(name, mode);
            SaveSnapshot();
            _logger.LogInformation("Dienst {Name} Starttyp → {Mode}", name, mode);
            return Task.FromResult(OptimizationResult.Ok($"{name}: {mode}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OptimizationResult.Fail($"{name}: {ex.Message}"));
        }
    }

    public async Task<OptimizationResult> ApplyDevPresetAsync(CancellationToken cancellationToken = default)
    {
        _savedStartupTypes ??= LoadSnapshot();
        var changes = new List<string>();

        foreach (var (name, mode) in DevPresetRules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var sc = new ServiceController(name);
                var current = GetStartupTypeEnum(name);
                if (current == mode)
                    continue;

                if (!_savedStartupTypes.ContainsKey(name))
                    _savedStartupTypes[name] = current;

                SetServiceStartMode(name, mode);
                changes.Add($"{name} → {mode}");
            }
            catch (InvalidOperationException)
            {
                _logger.LogDebug("Dienst {Name} nicht gefunden, übersprungen", name);
            }
        }

        if (changes.Count == 0)
            return OptimizationResult.Ok("Dev-Preset bereits aktiv oder keine Dienste gefunden.");

        SaveSnapshot();
        return OptimizationResult.Ok(changes.ToArray());
    }

    public Task<OptimizationResult> RollbackStartupTypesAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = LoadSnapshot();
        if (snapshot.Count == 0)
            return Task.FromResult(OptimizationResult.Fail("Kein Service-Snapshot vorhanden."));

        var restored = new List<string>();
        foreach (var (name, mode) in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                SetServiceStartMode(name, mode);
                restored.Add($"{name} → {mode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Rollback für Dienst {Name} fehlgeschlagen", name);
            }
        }

        _savedStartupTypes = null;
        if (File.Exists(_snapshotPath))
            File.Delete(_snapshotPath);

        return Task.FromResult(OptimizationResult.Ok(restored.ToArray()));
    }

    private static string GetStartupType(string serviceName)
    {
        try
        {
            return GetStartupTypeEnum(serviceName).ToString();
        }
        catch
        {
            return "Unknown";
        }
    }

    private static ServiceStartMode GetStartupTypeEnum(string serviceName)
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            $@"SYSTEM\CurrentControlSet\Services\{serviceName}");
        if (key is null)
            return ServiceStartMode.Manual;

        var start = key.GetValue("Start");
        return start switch
        {
            2 => ServiceStartMode.Automatic,
            3 => ServiceStartMode.Manual,
            4 => ServiceStartMode.Disabled,
            _ => ServiceStartMode.Manual,
        };
    }

    private static void SetServiceStartMode(string serviceName, ServiceStartMode mode)
    {
        var startValue = mode switch
        {
            ServiceStartMode.Automatic => 2,
            ServiceStartMode.Manual => 3,
            ServiceStartMode.Disabled => 4,
            _ => 3,
        };

        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            $@"SYSTEM\CurrentControlSet\Services\{serviceName}",
            writable: true) ?? throw new InvalidOperationException($"Registry-Schlüssel für {serviceName} nicht gefunden.");

        key.SetValue("Start", startValue, Microsoft.Win32.RegistryValueKind.DWord);
    }

    private Dictionary<string, ServiceStartMode> LoadSnapshot()
    {
        if (!File.Exists(_snapshotPath))
            return new Dictionary<string, ServiceStartMode>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var json = File.ReadAllText(_snapshotPath);
            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonDefaults.Options) ?? new();
            return raw.ToDictionary(
                kvp => kvp.Key,
                kvp => Enum.Parse<ServiceStartMode>(kvp.Value),
                StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, ServiceStartMode>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void SaveSnapshot()
    {
        if (_savedStartupTypes is null || _savedStartupTypes.Count == 0)
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(_snapshotPath)!);
        var raw = _savedStartupTypes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
        File.WriteAllText(_snapshotPath, JsonSerializer.Serialize(raw, JsonDefaults.Options));
    }
}
