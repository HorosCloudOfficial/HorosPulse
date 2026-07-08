namespace HorosPulse.Core.Models;

/// <summary>Einstellungen für das Kompakt-Fenster (Mem-Reduct-Style).</summary>
public sealed class CompactWindowSettings
{
    public bool ShowRamStats { get; set; } = true;

    public bool ShowCpuStats { get; set; } = true;

    public bool ShowDiskStats { get; set; } = true;

    public bool ShowMemoryCleanAction { get; set; } = true;

    public bool ShowCursorDevModeAction { get; set; } = true;

    public bool ShowDiskOptimizeAction { get; set; }

    public bool ShowVisualEffectsAction { get; set; }

    public bool OpenOnStartup { get; set; }

    public bool PurgeWorkingSet { get; set; } = true;

    public bool PurgeSystemFileCache { get; set; } = true;

    public bool PurgeModifiedPageList { get; set; } = true;

    public bool PurgeStandbyList { get; set; } = true;

    public bool PurgeLowPriorityStandby { get; set; } = true;

    public bool PurgeRegistryCache { get; set; }

    public bool PurgeCombineMemoryLists { get; set; } = true;

    public double WindowWidth { get; set; } = 350;

    public double WindowHeight { get; set; } = 640;

    public double WindowLeft { get; set; } = double.NaN;

    public double WindowTop { get; set; } = double.NaN;
}
