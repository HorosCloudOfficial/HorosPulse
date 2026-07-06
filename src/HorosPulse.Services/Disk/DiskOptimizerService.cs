namespace HorosPulse.Services.Disk;

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class DiskOptimizerService : IDiskOptimizerService
{
    private const string PrefetchKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters";

    private readonly ILogger<DiskOptimizerService> _logger;
    private readonly string _snapshotPath;
    private DiskOptimizerState? _snapshot;

    public DiskOptimizerService(ILogger<DiskOptimizerService> logger)
    {
        _logger = logger;
        _snapshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "disk-optimizer-snapshot.json");
    }

    public Task<DiskOptimizerState> GetCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadCurrentState());
    }

    public Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveSnapshotIfNeeded();

        try
        {
            SetRegistryDword(Registry.LocalMachine, PrefetchKeyPath, "EnablePrefetcher", 0);
            SetRegistryDword(Registry.LocalMachine, PrefetchKeyPath, "EnableSuperfetch", 0);

            _logger.LogInformation("Festplatten-Optimierungen angewendet");
            return Task.FromResult(OptimizationResult.Ok(
                "Prefetch deaktiviert (SSD/Dev)",
                "Superfetch deaktiviert"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OptimizationResult.Fail(ex.Message));
        }
    }

    public async Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var state = _snapshot ?? await LoadSnapshotAsync();
        if (state is null)
            return OptimizationResult.Fail("Kein Festplatten-Snapshot vorhanden.");

        try
        {
            RestoreRegistryDword(Registry.LocalMachine, PrefetchKeyPath, "EnablePrefetcher", state.PrefetchEnabled);
            RestoreRegistryDword(Registry.LocalMachine, PrefetchKeyPath, "EnableSuperfetch", state.SuperfetchEnabled);

            _snapshot = null;
            if (File.Exists(_snapshotPath))
                File.Delete(_snapshotPath);

            return OptimizationResult.Ok("Festplatten-Einstellungen zurückgesetzt");
        }
        catch (Exception ex)
        {
            return OptimizationResult.Fail(ex.Message);
        }
    }

    private DiskOptimizerState ReadCurrentState()
    {
        var prefetch = ReadRegistryDword(Registry.LocalMachine, PrefetchKeyPath, "EnablePrefetcher");
        var superfetch = ReadRegistryDword(Registry.LocalMachine, PrefetchKeyPath, "EnableSuperfetch");

        return new DiskOptimizerState
        {
            PrefetchEnabled = prefetch is > 0,
            SuperfetchEnabled = superfetch is > 0,
            TrimEnabled = QueryTrimEnabled(),
            WriteCacheEnabled = null,
            DefragStatus = QueryDefragStatus(),
        };
    }

    private static bool? QueryTrimEnabled()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "fsutil.exe",
                Arguments = "behavior query DisableDeleteNotify",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
                return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output.Contains("= 0", StringComparison.Ordinal);
        }
        catch
        {
            return null;
        }
    }

    private static string? QueryDefragStatus()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -Command \"Get-Volume -DriveLetter C | Select-Object -ExpandProperty HealthStatus\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
                return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrWhiteSpace(output) ? "Unbekannt" : output;
        }
        catch
        {
            return "Nicht abrufbar";
        }
    }

    private void SaveSnapshotIfNeeded()
    {
        if (_snapshot is not null || File.Exists(_snapshotPath))
            return;

        _snapshot = ReadCurrentState();
        var dir = Path.GetDirectoryName(_snapshotPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_snapshotPath, JsonSerializer.Serialize(_snapshot, new JsonSerializerOptions { WriteIndented = true }));
    }

    private async Task<DiskOptimizerState?> LoadSnapshotAsync()
    {
        if (!File.Exists(_snapshotPath))
            return null;

        var json = await File.ReadAllTextAsync(_snapshotPath);
        return JsonSerializer.Deserialize<DiskOptimizerState>(json);
    }

    private static int? ReadRegistryDword(RegistryKey hive, string subKey, string valueName)
    {
        using var key = hive.OpenSubKey(subKey);
        return key?.GetValue(valueName) as int?;
    }

    private static void SetRegistryDword(RegistryKey hive, string subKey, string valueName, int value)
    {
        using var key = hive.CreateSubKey(subKey, true)
            ?? throw new InvalidOperationException($"Registry-Schlüssel nicht beschreibbar: {subKey}");
        key.SetValue(valueName, value, RegistryValueKind.DWord);
    }

    private static void RestoreRegistryDword(RegistryKey hive, string subKey, string valueName, bool? enabled)
    {
        if (enabled is null)
            return;

        SetRegistryDword(hive, subKey, valueName, enabled.Value ? 3 : 0);
    }
}
