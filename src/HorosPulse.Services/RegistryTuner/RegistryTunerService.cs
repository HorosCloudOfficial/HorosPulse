namespace HorosPulse.Services.RegistryTuner;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class RegistryTunerService : IRegistryTunerService
{
    private static readonly IReadOnlyList<RegistryTweakDefinition> Tweaks =
    [
        new()
        {
            Id = "menu-show-delay",
            Name = "Schnellere Menüs",
            Description = "Reduziert MenuShowDelay für schnellere Kontextmenüs (Microsoft-Dokumentation: Desktop-Verhalten).",
            Hive = "HKCU",
            KeyPath = @"Control Panel\Desktop",
            ValueName = "MenuShowDelay",
            RecommendedValue = 0,
            DocumentationUrl = "https://learn.microsoft.com/windows/win32/uxguide/winenv-ui",
        },
        new()
        {
            Id = "mouse-hover-time",
            Name = "Schnellere Tooltip-Hover",
            Description = "Verkürzt MouseHoverTime für schnellere Hover-Reaktion.",
            Hive = "HKCU",
            KeyPath = @"Control Panel\Mouse",
            ValueName = "MouseHoverTime",
            RecommendedValue = 100,
            DocumentationUrl = "https://learn.microsoft.com/windows/win32/uxguide/winenv-ui",
        },
    ];

    private readonly ILogger<RegistryTunerService> _logger;
    private readonly string _snapshotPath;
    private Dictionary<string, int?>? _snapshot;

    public RegistryTunerService(ILogger<RegistryTunerService> logger)
    {
        _logger = logger;
        _snapshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "registry-tuner-snapshot.json");
    }

    public IReadOnlyList<RegistryTweakDefinition> GetAvailableTweaks() => Tweaks;

    public Task<OptimizationResult> ApplyTweakAsync(string tweakId, bool userConfirmed, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!userConfirmed)
            return Task.FromResult(OptimizationResult.Fail("Registry-Tweak erfordert ausdrückliche Zustimmung."));

        var tweak = Tweaks.FirstOrDefault(t => t.Id == tweakId);
        if (tweak is null)
            return Task.FromResult(OptimizationResult.Fail($"Unbekannter Tweak: {tweakId}"));

        try
        {
            SaveSnapshotIfNeeded();
            var hive = tweak.Hive.Equals("HKCU", StringComparison.OrdinalIgnoreCase)
                ? Registry.CurrentUser
                : Registry.LocalMachine;

            var current = ReadRegistryDword(hive, tweak.KeyPath, tweak.ValueName);
            AppendSnapshotEntry(tweak.Id, current);
            SetRegistryDword(hive, tweak.KeyPath, tweak.ValueName, tweak.RecommendedValue);

            _logger.LogInformation("Registry-Tweak angewendet: {Id}", tweakId);
            return Task.FromResult(OptimizationResult.Ok($"{tweak.Name} angewendet"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OptimizationResult.Fail(ex.Message));
        }
    }

    public async Task<OptimizationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _snapshot ?? await LoadSnapshotAsync();
        if (snapshot is null || snapshot.Count == 0)
            return OptimizationResult.Fail("Kein Registry-Tuner-Snapshot vorhanden.");

        var restored = new List<string>();
        foreach (var tweak in Tweaks)
        {
            if (!snapshot.TryGetValue(tweak.Id, out var previous))
                continue;

            var hive = tweak.Hive.Equals("HKCU", StringComparison.OrdinalIgnoreCase)
                ? Registry.CurrentUser
                : Registry.LocalMachine;

            if (previous is null)
            {
                using var key = hive.OpenSubKey(tweak.KeyPath, writable: true);
                key?.DeleteValue(tweak.ValueName, throwOnMissingValue: false);
            }
            else
            {
                SetRegistryDword(hive, tweak.KeyPath, tweak.ValueName, previous.Value);
            }

            restored.Add(tweak.Name);
        }

        _snapshot = null;
        if (File.Exists(_snapshotPath))
            File.Delete(_snapshotPath);

        return OptimizationResult.Ok(restored.Select(n => $"Zurückgesetzt: {n}").ToArray());
    }

    private void SaveSnapshotIfNeeded()
    {
        if (_snapshot is not null)
            return;

        _snapshot = LoadSnapshotAsync().GetAwaiter().GetResult() ?? new Dictionary<string, int?>();
    }

    private void AppendSnapshotEntry(string tweakId, int? previous)
    {
        _snapshot ??= new Dictionary<string, int?>();
        if (!_snapshot.ContainsKey(tweakId))
            _snapshot[tweakId] = previous;

        var dir = Path.GetDirectoryName(_snapshotPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_snapshotPath, JsonSerializer.Serialize(_snapshot, JsonDefaults.Options));
    }

    private async Task<Dictionary<string, int?>?> LoadSnapshotAsync()
    {
        if (!File.Exists(_snapshotPath))
            return null;

        var json = await File.ReadAllTextAsync(_snapshotPath);
        return JsonSerializer.Deserialize<Dictionary<string, int?>>(json, JsonDefaults.Options);
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
}
