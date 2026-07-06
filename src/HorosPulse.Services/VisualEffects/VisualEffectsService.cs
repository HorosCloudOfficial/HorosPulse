namespace HorosPulse.Services.VisualEffects;

using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using HorosPulse.Core.Enums;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;
using HorosPulse.Data;

public sealed class VisualEffectsService : IVisualEffectsService
{
    private const string DesktopKeyPath = @"Control Panel\Desktop";
    private const string UserPreferencesMaskValue = "UserPreferencesMask";

    private const uint SpiSetUiEffects = 0x103F;
    private const uint SpiGetUiEffects = 0x103E;
    private const uint SpiSetDragFullWindows = 0x0025;
    private const uint SpiGetDragFullWindows = 0x0026;
    private const uint SpiSetMenuAnimation = 0x1003;
    private const uint SpiGetMenuAnimation = 0x1002;
    private const uint SpiSetComboBoxAnimation = 0x1005;
    private const uint SpiGetComboBoxAnimation = 0x1004;
    private const uint SpiSetListBoxSmoothScrolling = 0x1007;
    private const uint SpiGetListBoxSmoothScrolling = 0x1006;
    private const uint SpiSetCursorShadow = 0x101B;
    private const uint SpiGetCursorShadow = 0x101A;
    private const uint SpiSetDropShadow = 0x1025;
    private const uint SpifUpdateIniFile = 0x01;
    private const uint SpifSendChange = 0x02;

    private readonly ILogger<VisualEffectsService> _logger;
    private readonly string _snapshotPath;
    private VisualEffectsState? _snapshot;

    public VisualEffectsService(ILogger<VisualEffectsService> logger)
    {
        _logger = logger;
        _snapshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosPulse", "visual-effects-snapshot.json");
    }

    public Task<VisualEffectsState> GetCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadCurrentState());
    }

    public Task<OptimizationResult> ApplyPresetAsync(VisualEffectsPreset preset, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveSnapshotIfNeeded();

        var (animations, dragFull, menu, combo, listbox, cursorShadow, dropShadow) = preset switch
        {
            VisualEffectsPreset.Performance => (false, false, false, false, false, false, false),
            VisualEffectsPreset.Balanced => (true, false, true, false, true, false, false),
            VisualEffectsPreset.BestAppearance => (true, true, true, true, true, true, true),
            _ => (false, false, false, false, false, false, false),
        };

        try
        {
            SetSystemParameter(SpiSetUiEffects, animations);
            SetSystemParameter(SpiSetDragFullWindows, dragFull);
            SetSystemParameter(SpiSetMenuAnimation, menu);
            SetSystemParameter(SpiSetComboBoxAnimation, combo);
            SetSystemParameter(SpiSetListBoxSmoothScrolling, listbox);
            SetSystemParameter(SpiSetCursorShadow, cursorShadow);
            SetSystemParameter(SpiSetDropShadow, dropShadow);

            _logger.LogInformation("Visuelle Effekte angewendet: {Preset}", preset);
            return Task.FromResult(OptimizationResult.Ok($"Preset {preset} angewendet"));
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
            return OptimizationResult.Fail("Kein Visual-Effects-Snapshot vorhanden.");

        try
        {
            SetSystemParameter(SpiSetUiEffects, state.AnimationsEnabled);
            SetSystemParameter(SpiSetDragFullWindows, state.DragFullWindows);
            SetSystemParameter(SpiSetMenuAnimation, state.MenuAnimation);
            SetSystemParameter(SpiSetComboBoxAnimation, state.ComboBoxAnimation);
            SetSystemParameter(SpiSetListBoxSmoothScrolling, state.ListBoxSmoothScrolling);
            SetSystemParameter(SpiSetCursorShadow, state.CursorShadow);

            if (!string.IsNullOrEmpty(state.UserPreferencesMaskHex))
                RestoreUserPreferencesMask(state.UserPreferencesMaskHex);

            _snapshot = null;
            if (File.Exists(_snapshotPath))
                File.Delete(_snapshotPath);

            return OptimizationResult.Ok("Visuelle Effekte zurückgesetzt");
        }
        catch (Exception ex)
        {
            return OptimizationResult.Fail(ex.Message);
        }
    }

    private VisualEffectsState ReadCurrentState() =>
        new()
        {
            AnimationsEnabled = GetSystemParameter(SpiGetUiEffects),
            DragFullWindows = GetSystemParameter(SpiGetDragFullWindows),
            MenuAnimation = GetSystemParameter(SpiGetMenuAnimation),
            ComboBoxAnimation = GetSystemParameter(SpiGetComboBoxAnimation),
            ListBoxSmoothScrolling = GetSystemParameter(SpiGetListBoxSmoothScrolling),
            CursorShadow = GetSystemParameter(SpiGetCursorShadow),
            UserPreferencesMaskHex = ReadUserPreferencesMaskHex(),
        };

    private void SaveSnapshotIfNeeded()
    {
        if (_snapshot is not null || File.Exists(_snapshotPath))
            return;

        _snapshot = ReadCurrentState();
        Directory.CreateDirectory(Path.GetDirectoryName(_snapshotPath)!);
        File.WriteAllText(_snapshotPath, JsonSerializer.Serialize(_snapshot, JsonDefaults.Options));
    }

    private async Task<VisualEffectsState?> LoadSnapshotAsync()
    {
        if (!File.Exists(_snapshotPath))
            return null;

        var json = await File.ReadAllTextAsync(_snapshotPath);
        return JsonSerializer.Deserialize<VisualEffectsState>(json, JsonDefaults.Options);
    }

    private static void SetSystemParameter(uint action, bool enabled)
    {
        var value = enabled ? 1 : 0;
        if (!SystemParametersInfo(action, 0, ref value, SpifUpdateIniFile | SpifSendChange))
            throw new InvalidOperationException($"SystemParametersInfo(0x{action:X}) fehlgeschlagen.");
    }

    private static bool GetSystemParameter(uint action)
    {
        var value = 0;
        return SystemParametersInfo(action, 0, ref value, 0) && value != 0;
    }

    private static string? ReadUserPreferencesMaskHex()
    {
        using var key = Registry.CurrentUser.OpenSubKey(DesktopKeyPath);
        if (key?.GetValue(UserPreferencesMaskValue) is not byte[] bytes)
            return null;

        return Convert.ToHexString(bytes);
    }

    private static void RestoreUserPreferencesMask(string hex)
    {
        var bytes = Convert.FromHexString(hex);
        using var key = Registry.CurrentUser.OpenSubKey(DesktopKeyPath, writable: true);
        key?.SetValue(UserPreferencesMaskValue, bytes, RegistryValueKind.Binary);
    }

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfoW")]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int pvParam, uint fWinIni);
}
