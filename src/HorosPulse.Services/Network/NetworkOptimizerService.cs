namespace HorosPulse.Services.Network;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class NetworkOptimizerService : INetworkOptimizerService
{
    private const string TcpipParametersPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
    private const string DnsCacheParametersPath = @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters";

    private readonly ILogger<NetworkOptimizerService> _logger;
    private readonly string _snapshotPath;
    private NetworkSettingsState? _snapshot;

    public NetworkOptimizerService(ILogger<NetworkOptimizerService> logger)
    {
        _logger = logger;
        _snapshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "network-settings-snapshot.json");
    }

    public Task<NetworkSettingsState> GetCurrentSettingsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadCurrentSettings());
    }

    public Task<OptimizationResult> ApplyOptimizationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveSnapshotIfNeeded();

        try
        {
            SetRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TcpAckFrequency", 1);
            SetRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TCPNoDelay", 1);
            SetRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TcpDelAckTicks", 0);
            SetRegistryDword(Registry.LocalMachine, DnsCacheParametersPath, "MaxCacheTtl", 300);

            _logger.LogInformation("Netzwerk-Optimierungen angewendet");
            return Task.FromResult(OptimizationResult.Ok(
                "TCP NoDelay aktiviert",
                "DNS-Cache-TTL auf 300s gesetzt"));
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
            return OptimizationResult.Fail("Kein Netzwerk-Snapshot vorhanden.");

        try
        {
            RestoreRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TcpAckFrequency", state.TcpAckFrequency);
            RestoreRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TCPNoDelay", state.TcpNoDelay);
            RestoreRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TcpDelAckTicks", state.TcpDelAckTicks);
            RestoreRegistryDword(Registry.LocalMachine, DnsCacheParametersPath, "MaxCacheTtl", state.DnsMaxCacheTtl);

            _snapshot = null;
            if (File.Exists(_snapshotPath))
                File.Delete(_snapshotPath);

            return OptimizationResult.Ok("Netzwerk-Einstellungen zurückgesetzt");
        }
        catch (Exception ex)
        {
            return OptimizationResult.Fail(ex.Message);
        }
    }

    private NetworkSettingsState ReadCurrentSettings() =>
        new()
        {
            TcpAckFrequency = ReadRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TcpAckFrequency"),
            TcpNoDelay = ReadRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TCPNoDelay"),
            TcpDelAckTicks = ReadRegistryDword(Registry.LocalMachine, TcpipParametersPath, "TcpDelAckTicks"),
            DnsMaxCacheTtl = ReadRegistryDword(Registry.LocalMachine, DnsCacheParametersPath, "MaxCacheTtl"),
        };

    private void SaveSnapshotIfNeeded()
    {
        if (_snapshot is not null || File.Exists(_snapshotPath))
            return;

        _snapshot = ReadCurrentSettings();
        Directory.CreateDirectory(Path.GetDirectoryName(_snapshotPath)!);
        File.WriteAllText(_snapshotPath, JsonSerializer.Serialize(_snapshot));
    }

    private async Task<NetworkSettingsState?> LoadSnapshotAsync()
    {
        if (!File.Exists(_snapshotPath))
            return null;

        var json = await File.ReadAllTextAsync(_snapshotPath);
        return JsonSerializer.Deserialize<NetworkSettingsState>(json);
    }

    private static int? ReadRegistryDword(RegistryKey hive, string subKey, string name)
    {
        using var key = hive.OpenSubKey(subKey);
        return key?.GetValue(name) as int?;
    }

    private static void SetRegistryDword(RegistryKey hive, string subKey, string name, int value)
    {
        using var key = hive.OpenSubKey(subKey, writable: true)
            ?? hive.CreateSubKey(subKey, writable: true)
            ?? throw new InvalidOperationException($"Registry-Schlüssel nicht erstellbar: {subKey}");

        key.SetValue(name, value, RegistryValueKind.DWord);
    }

    private static void RestoreRegistryDword(RegistryKey hive, string subKey, string name, int? value)
    {
        using var key = hive.OpenSubKey(subKey, writable: true);
        if (key is null)
            return;

        if (value is null)
            key.DeleteValue(name, throwOnMissingValue: false);
        else
            key.SetValue(name, value.Value, RegistryValueKind.DWord);
    }
}
