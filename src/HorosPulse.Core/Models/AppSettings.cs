namespace HorosPulse.Core.Models;

public sealed class AppSettings
{
    public bool DefenderOptIn { get; set; }
    public IReadOnlyList<string> DefaultDevFolders { get; set; } =
    [
        "node_modules",
        ".git",
        "dist",
        "build",
    ];

    public int SnapshotRetentionLimit { get; set; } = 50;
    public int MetricsPollingIntervalMs { get; set; } = 2000;

    /// <summary>UI-Theme: Dark, TokyoNight oder Light.</summary>
    public string Theme { get; set; } = "TokyoNight";

    /// <summary>Serilog-Minimum-Level: Verbose, Debug, Information, Warning, Error.</summary>
    public string MinimumLogLevel { get; set; } = "Information";

    /// <summary>Autostart mit Windows (HKCU Run).</summary>
    public bool AutoStartWithWindows { get; set; }

    /// <summary>PowerShell-Standard-Timeout in Sekunden.</summary>
    public int PowerShellTimeoutSeconds { get; set; } = 30;

    /// <summary>Legacy-Flag; wird mit Theme=Light synchron gehalten.</summary>
    public bool UseLightMode { get; set; }

    /// <summary>Fensterbreite beim letzten Schließen.</summary>
    public double WindowWidth { get; set; } = 1100;

    /// <summary>Fensterhöhe beim letzten Schließen.</summary>
    public double WindowHeight { get; set; } = 720;

    /// <summary>Fenster-Left-Position (NaN = zentriert).</summary>
    public double WindowLeft { get; set; } = double.NaN;

    /// <summary>Fenster-Top-Position (NaN = zentriert).</summary>
    public double WindowTop { get; set; } = double.NaN;

    /// <summary>Fensterzustand: Normal, Maximized, Minimized.</summary>
    public string WindowState { get; set; } = "Normal";

    /// <summary>Prozess-Monitor Polling-Intervall in ms (Standard 5s).</summary>
    public int ProcessMonitorRefreshIntervalMs { get; set; } = 5000;

    /// <summary>Nur Cursor-relevante Prozesse im Monitor anzeigen.</summary>
    public bool ProcessMonitorCursorFilterOnly { get; set; } = true;

    /// <summary>Windows-Suchdienst nach Indexer-Änderung neu starten (Opt-in).</summary>
    public bool RestartSearchServiceAfterIndexerChange { get; set; }

    /// <summary>Cursor-Prioritäten bei neuem cursor.exe-Prozess automatisch anwenden.</summary>
    public bool ReapplyCursorPrioritiesOnRestart { get; set; }

    /// <summary>Kompakt-Fenster (Mem-Reduct-Style Mini-UI).</summary>
    public CompactWindowSettings CompactWindow { get; set; } = new();
}
