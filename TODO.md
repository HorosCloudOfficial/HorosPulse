# WindowsPerformance — Master Task List

> **Projekt:** WPF .NET 9 Desktop-App zur Windows-Systemoptimierung für Cursor IDE Programmier-Workflows
> **Stand:** 2026-07-05 · Greenfield · Workspace: `D:\WindowsPerformance`

---

## Statuslegende

| Symbol | Bedeutung |
|--------|-----------|
| `- [ ]` | Offen |
| `- [x]` | Erledigt |
| `- [~]` | In Arbeit |
| `- [!]` | Blockiert (Abhängigkeit offen) |
| **P0** | Kritisch — MVP bricht ohne diesen Task |
| **P1** | Wichtig — MVP unvollständig ohne diesen Task |
| **P2** | Nice-to-have für MVP, Pflicht ab Phase 2 |

---

## Projektüberblick

WindowsPerformance ist eine WPF .NET 9 Desktop-Applikation mit einem modernen Tokyo-Night-Interface, die Windows-Systemeinstellungen gezielt für Cursor-IDE-Entwicklungsworkflows optimiert. Sie arbeitet mit on-demand-Elevation (separates `ElevationHelper.exe`), persistiert Snapshots in SQLite, führt PowerShell-Skripte als externer Prozess (`pwsh.exe`) aus und stellt jederzeit Rollback-Sicherheit bereit.

---

## Tech Stack

| Schicht | Technologie |
|---------|-------------|
| UI-Framework | WPF, .NET 9, C# 13 |
| MVVM | CommunityToolkit.Mvvm 8.x |
| Design-System | Tokyo Night Farb-Palette, MaterialDesignInXamlToolkit oder eigene Styles |
| PowerShell | `pwsh.exe` externer Prozess (keine In-Process-Ausführung) |
| Persistenz | System.Text.Json (Profiles/Snapshots) + SQLite via EF Core 9 (Trends/Audit) |
| Elevation | Separates `ElevationHelper.exe` (UAC on-demand, App läuft **nie** als Admin) |
| Logging | Microsoft.Extensions.Logging + Serilog-File-Sink |
| Tests | xUnit, Moq, FluentAssertions |
| Build/CI | `dotnet build`, `dotnet test` (lokal); GitHub Actions optional |

---

## Solution-Struktur

```
WindowsPerformance.sln
├── src/
│   ├── WindowsPerformance.App           # WPF Entry-Point, Styles, Navigation
│   ├── WindowsPerformance.Core          # Domain-Modelle, Interfaces, Enums
│   ├── WindowsPerformance.ViewModels    # CommunityToolkit.Mvvm ViewModels
│   ├── WindowsPerformance.Services      # Business-Logik, OS-Interaktion
│   ├── WindowsPerformance.Data          # EF Core DbContext, Repositories, JSON-IO
│   ├── WindowsPerformance.Elevation     # ElevationHelper.exe (separates Projekt)
│   └── WindowsPerformance.PowerShell   # Wrapper für pwsh.exe-Ausführung
└── tests/
    ├── WindowsPerformance.Unit          # xUnit Unit-Tests
    └── WindowsPerformance.Integration   # Integrations-Tests (mit echtem FS/Registry)
```

---

## Phasen-Roadmap

| Phase | Zeitraum | Ziel |
|-------|----------|------|
| **MVP** | Sprint 1–3 (6 Wochen) | Shell, Dashboard, Kern-Optimierer, Rollback, Presets |
| **Phase 2** | W7–14 | Services, Memory, Network, HealthScorer, Trends |
| **Phase 3** | W15+ | Disk, Scheduler, ML, Installer, Registry-Tuner |

---

---

# MVP — Sprint 1–3

---

## Sprint 1 (W1–2): Solution Foundation & Shell

### 1.1 Solution Setup (P0)

- [~] **P0** Git-Repository initialisieren (`git init`) und initialer Commit nach Solution-Setup *(ergänzt)* — `git init` erledigt, Commit offen
- [x] **P0** Branch-Strategie festlegen: `main` (stable), Feature-Branches `feature/<name>`, Squash-Merge in `main` *(ergänzt)*
- [x] **P0** `dotnet new sln -n WindowsPerformance` in `D:\WindowsPerformance` ausführen
- [x] **P0** Projektordner `src/` und `tests/` anlegen
- [x] **P0** `WindowsPerformance.App` (WPF, .NET 9) erstellen: `dotnet new wpf -n WindowsPerformance.App -o src/WindowsPerformance.App`
- [x] **P0** `WindowsPerformance.Core` (classlib) erstellen
- [x] **P0** `WindowsPerformance.ViewModels` (classlib) erstellen
- [x] **P0** `WindowsPerformance.Services` (classlib) erstellen
- [x] **P0** `WindowsPerformance.Data` (classlib) erstellen
- [x] **P0** `WindowsPerformance.Elevation` (console app, separates EXE-Projekt) erstellen
- [x] **P0** `WindowsPerformance.PowerShell` (classlib) erstellen — Platzhalter-Ordner mit `.psm1`
- [x] **P0** `WindowsPerformance.Unit` (xUnit-Testprojekt) erstellen — als `tests/WindowsPerformance.Tests.Unit`
- [x] **P0** `WindowsPerformance.Integration` (xUnit-Testprojekt) erstellen — als `tests/WindowsPerformance.Tests.Integration`
- [x] **P0** Alle Projekte zur Solution hinzufügen (`dotnet sln add …`)
- [x] **P0** Projekt-Referenzen verdrahten (App → ViewModels, Services, Data; ViewModels → Core; Services → Core, Data; Tests.Unit → Core, Services)
- [x] **P0** `global.json` mit `"sdk": { "version": "9.x.x" }` anlegen
- [x] **P0** `.editorconfig` mit C#-Konventionen anlegen
- [x] **P0** `.gitignore` (Visual Studio Template + `*.user`, `bin/`, `obj/`, `*.db`) anlegen
- [x] **P1** `Directory.Build.props` für gemeinsame MSBuild-Properties anlegen (Nullable, LangVersion, TreatWarningsAsErrors)
- [ ] **P1** `Directory.Packages.props` für Central Package Management anlegen
- [ ] **P1** NuGet-Pakete zentral eintragen: CommunityToolkit.Mvvm, Microsoft.EntityFrameworkCore.Sqlite, Serilog, FluentAssertions, xUnit, Moq

### 1.2 NuGet-Abhängigkeiten (P0)

- [x] **P0** `CommunityToolkit.Mvvm` in ViewModels + App referenzieren
- [ ] **P0** `Microsoft.EntityFrameworkCore.Sqlite` in Data referenzieren — Sprint 1: `Microsoft.Data.Sqlite` stattdessen
- [ ] **P0** `Microsoft.EntityFrameworkCore.Design` (Dev-Dependency) in Data referenzieren
- [x] **P0** `Serilog.Extensions.Logging` + `Serilog.Sinks.File` in App + Services referenzieren — App only (Sprint 1)
- [x] **P1** `Microsoft.Extensions.Hosting` in App für DI-Container
- [x] **P1** `Microsoft.Extensions.DependencyInjection` in App
- [x] **P1** `System.Management` in Services (WMI-Zugriffe)

### 1.3 DI / App-Bootstrap (P0)

- [x] **P0** `App.xaml.cs` — `IHost`-basierter Bootstrap: `CreateHostBuilder()`, `ConfigureServices()`, `Build().Start()`
- [x] **P0** `ServiceLocator` / Extension-Methoden `AddCoreServices()`, `AddViewModels()`, `AddDataServices()` in je eigenem Projekt
- [x] **P0** `IServiceProvider` in `App` halten; kein `static ServiceLocator`
- [x] **P0** Serilog in `CreateHostBuilder` als `ILogger`-Provider konfigurieren (File-Sink: `logs\app-.log`, RollingInterval.Day)
- [x] **P1** `appsettings.json` in App.App (LogLevel, DB-Pfad, Profil-Pfad konfigurierbar)
- [ ] **P1** `IOptions<AppSettings>` pattern für Konfiguration
- [x] **P0** `IOptimizationModule`-Implementierungen in DI registrieren (`AddSingleton<IOptimizationModule, XModule>()` pro Modul; `IEnumerable<IOptimizationModule>` für RollbackEngine) *(ergänzt)*
- [x] **P0** Globaler Exception-Handler: `DispatcherUnhandledException` + `AppDomain.UnhandledException` + `TaskScheduler.UnobservedTaskException` → loggen + benutzerfreundliche Fehler-UI *(ergänzt)*
- [x] **P0** `pwsh.exe`-Verfügbarkeit beim App-Start prüfen; Warnung loggen wenn fehlend *(ergänzt)*

### 1.4 GUI-Shell / Haupt-Window (P0)

**Abhängigkeit:** 1.3 muss abgeschlossen sein

- [x] **P0** `MainWindow.xaml` — Window-Chrome entfernen (`WindowStyle="None"`, `AllowsTransparency="True"`)
- [x] **P0** Tokyo-Night Farb-Palette als `ResourceDictionary` definieren (`Accent`, `Background`, `Surface`, `Border`, `ForegroundPrimary`, `ForegroundMuted`, `Success`, `Warning`, `Danger`)
- [x] **P0** `MainWindow` Basis-Layout: Sidebar (links, 220px fix), ContentArea (rechts, `Frame` oder `ContentControl`)
- [x] **P0** Sidebar: App-Logo/Name oben, vertikale Nav-Items, Version unten
- [x] **P0** Custom Title-Bar mit Drag-Region, Minimize/Maximize/Close-Buttons
- [ ] **P0** `NavigationService` (Interface `INavigationService`) — `Navigate(Type viewModelType)`, `GoBack()`
- [x] **P0** `MainViewModel` — aktive View per `CurrentView`-Property, `NavigateCommand`
- [x] **P0** `ViewModelLocator` oder DataTemplate-basiertes View-ViewModel-Mapping in `App.xaml`
- [x] **P1** Sidebar Nav-Items: Dashboard, Power, Cursor, Prozesse, Snapshots, Einstellungen — DE: Dashboard, Energie, Cursor, Monitor, Presets, Einstellungen
- [x] **P1** Nav-Items für spätere Sprints: Platzhalter-ViewModels/Views (`ComingSoonView`) statt leerer Navigation — Sidebar bleibt vollständig klickbar *(ergänzt)*
- [x] **P1** SemVer + Assembly-Version synchron halten (`Directory.Build.props` oder `<Version>` in csproj); Version in Sidebar-Footer und About-Dialog anzeigen *(ergänzt)* — Sidebar v0.1.0
- [x] **P1** Aktiver Nav-Item visuell hervorheben (Tokyo Night Accent)
- [ ] **P1** Smooth-Transition beim View-Wechsel
- [ ] **P1** Tray-Icon (NotifyIcon via `Hardcoders.NotifyIcon.Wpf` oder `H.NotifyIcon`) — App in Tray minimieren
- [ ] **P2** Fenster-Größe und -Position in `appsettings.json` / `UserPreferences.json` persistieren

### 1.5 Tokyo-Night Design-System (P1)

- [x] **P1** Basis-Styles in `/Themes/TokyoNight.xaml`: `Window`, `Button`, `TextBlock`, `TextBox`, `CheckBox`, `ToggleButton`, `Separator`
- [x] **P1** `CardBorder`-Style (abgerundete Surface-Karte, Elevation-Schatten)
- [ ] **P1** `StatusBadge`-Style (farbiger Chip: Active/Warning/Error/Neutral)
- [ ] **P1** Custom `ScrollBar`-Style (schmal, Tokyo-Night-Farben)
- [ ] **P1** `ProgressRing` oder `LoadingSpinner` für async Operationen
- [ ] **P2** Light-Mode Toggle (optionales Future-Feature, jetzt nur Skeleton)

---

## Sprint 2 (W3–4): Kern-Optimierungsmodule

### 2.1 Core-Domänenmodelle (P0)

**Abhängigkeit:** 1.1 abgeschlossen

- [x] **P0** `OptimizationResult` (record: `bool Success`, `string? ErrorMessage`, `IReadOnlyList<string> Changes`)
- [x] **P0** `SnapshotEntry` (class: Id, CreatedAt, Label, Module, StateJson, CanRollback) — Sprint 3
- [x] **P0** `ProfileDefinition` (class: Id, Name, Description, ModuleStates Dictionary) — Sprint 3
- [x] **P0** `ModuleState` (abstract record: ModuleName, IsEnabled, Settings Dictionary) — Sprint 3
- [x] **P0** `AuditEntry` (class: Timestamp, Operation, Module, Actor, Details)
- [x] **P0** `PerformanceMetric` (record: Timestamp, CpuPercent, RamUsedMB, DiskActivePercent) — Sprint 3
- [x] **P1** Enums: `PowerPlanType`, `ProcessPriorityLevel`, `OptimizationStatus` *(ProcessPriority + RollbackStatus → Sprint 3)*
- [x] **P1** `IOptimizationModule` Interface: `ModuleName`, `CanApply`, `ApplyAsync`, `RollbackAsync` *(vereinfacht; SnapshotEntry-Integration → Sprint 3)*
- [x] **P0** Zusätzliche Modelle: `PowerPlanInfo`, `CursorSettingsProfile`, `DefenderExclusionSet`, `IndexerExcludeEntry`, `ProcessPriorityRule` *(Sprint 2)*

### 2.2 PowerShell-Wrapper (P0)

**Abhängigkeit:** 1.1, 1.3

- [x] **P0** `IPowerShellBridge` Interface: `RunAsync(string script, bool elevated, CancellationToken ct)` *(als IPowerShellRunner)*
- [x] **P0** `PowerShellResult` (record: int ExitCode, string StdOut, string StdErr, bool Success)
- [x] **P0** `PowerShellBridge` — startet `pwsh.exe -NonInteractive -NoProfile -Command "…"` via `Process.Start`, captured stdout/stderr, Timeout (30s default)
- [x] **P0** Elevated-Variante: Named-Pipe-IPC an `ElevationHelper.exe --server` (UAC via `runas` beim Start)
- [x] **P0** Skript-Sanitisierung: verbotene Muster blocken (z. B. `rm -rf`, `Format-`, `Remove-Item C:\Windows`)
- [x] **P0** Startup-Check: `pwsh.exe`-Verfügbarkeit beim App-Start prüfen; Fallback auf `powershell.exe` 5.1 *(ergänzt)*
- [ ] **P1** `PowerShellScriptLibrary` — statische Klasse mit vorkompilierten Skript-Strings (keine `.ps1`-Dateien auf Disk im Produktionsbetrieb)
- [x] **P1** Timeout konfigurierbar via `PowerShellOptions` *(ohne IOptions-Pattern)*
- [ ] **P1** Logging jedes PS-Aufrufs (Script-Hash, ExitCode, Dauer) via `IAuditLogger` — Sprint 3

### 2.3 ElevationHelper.exe (P0)

**Abhängigkeit:** 1.1

- [x] **P0** Separates `WindowsPerformance.Elevation`-Projekt (Console App, .NET 9, `<ApplicationManifest>` mit `requireAdministrator`)
- [x] **P0** Eingabe: Base64-kodiertes PowerShell-Skript als `args[0]`, optional `--timeout <ms>` *(+ Named-Pipe-Server-Modus)*
- [x] **P0** Ausgabe: JSON auf stdout `{ "exitCode": 0, "stdout": "…", "stderr": "…" }`
- [x] **P0** Fehlerfall: Exit-Code ≠ 0, JSON mit `stderr`-Inhalt
- [x] **P0** Keine dauerhafte Elevation: ElevationHelper.exe `--server` läuft pro UAC-Session, Einzelaufruf-Modus beendet sich
- [ ] **P1** Whitelist der erlaubten Skript-Hashes (SHA-256) im ElevationHelper — lehnt unbekannte Skripte ab
- [ ] **P1** Signierung des ElevationHelper.exe (selbstsigniertes Zertifikat für Dev; TODO: echt signieren für Dist.)
- [ ] **P1** Test: ElevationHelper gibt korrektes JSON zurück für bekanntes Dummy-Skript
- [x] **P0** Build-Output: `ElevationHelper.exe` bei Build neben `WindowsPerformance.App.exe` kopieren (MSBuild Post-Build-Target) *(ergänzt)*
- [x] **P0** Runtime-Pfad-Auflösung: `ElevationHelperPathResolver` findet EXE relativ zum App-Basisverzeichnis (Dev-Build + portable Deploy) *(ergänzt)*

### 2.4 PowerPlan-Modul (P1)

**Abhängigkeit:** 2.2, 2.3

- [x] **P1** `IPowerPlanService` Interface: `GetAvailablePlansAsync`, `GetActivePlanAsync`, `SetActivePlanAsync`, `EnsureHighPerformancePlanAsync`
- [x] **P1** `PowerPlanService` — nutzt `powercfg /list` + `powercfg /setactive <GUID>` via Process
- [x] **P1** GUIDs für Balanced, High Performance ermitteln; Duplicate bei Bedarf
- [x] **P1** Snapshot: aktiver Plan-GUID vor Änderung sichern
- [x] **P1** Rollback: gespeicherten Plan-GUID wieder aktivieren
- [x] **P1** `EnergieViewModel` — zeigt aktiven Plan, ListBox für Plan-Auswahl, Apply-Button
- [x] **P1** `EnergieView.xaml` — Tokyo-Night-Karte mit Plan-Selector
- [ ] **P2** Ultimate Performance Plan bei Bedarf erst aktivieren (`powercfg -duplicatescheme GUID`)

### 2.5 CursorOptimizer-Modul (P0)

**Abhängigkeit:** 2.2, 2.3

- [x] **P0** `ICursorOptimizer` Interface + `CursorOptimizerService`
- [x] **P0** settings.json lesen/schreiben mit Merge (Performance-Template: maxTsServerMemory 8192, watcherExclude, search.exclude, minimap off, telemetry off)
- [x] **P0** Backup vor Schreiben (`settings.json.windowsperformance.bak`)
- [x] **P0** Rollback: settings.json aus Backup wiederherstellen
- [x] **P1** `CursorViewModel` + `CursorView.xaml` — Apply/Restore, Vorschau der Änderungen
- [x] **P1** Live-Status: Prozessprioritäten-Status in Cursor-Ansicht
- [x] **P1** "Cursor nicht gefunden"-Zustand mit Hinweis
- [ ] **P2** Automatische Re-Anwendung wenn `cursor.exe` neu startet (FileSystemWatcher auf Process-Start — optional)

### 2.6 Process Priority Modul (P1)

**Abhängigkeit:** 2.2

- [x] **P1** `IProcessPriorityService` Interface: `ApplyCursorPrioritiesAsync`, `SetPriority`, `GetDefaultRules`
- [ ] **P1** Prozessliste laden (Name, PID, Priority, CPU%, RAM-MB) via `System.Diagnostics.Process` — Monitor-Sprint
- [x] **P1** Konfigurierbare Prioritäts-Regeln (`ProcessPriorityRule`: Cursor=High, Cursor Helper=BelowNormal)
- [x] **P1** Regeln in JSON persistieren (`process-priority-state.json`)
- [x] **P1** Snapshot: aktuelle Prioritäten der verwalteten Prozesse
- [x] **P1** Rollback: Prioritäten auf Snapshot-Stand zurücksetzen
- [x] **P1** UI in `CursorView` — Apply-Button für Cursor-Prioritäten *(kein separates ProcessPriorityView)*
- [ ] **P2** Refresh-Intervall konfigurierbar (default: 5s)
- [ ] **P2** Filter: nur Cursor-relevante Prozesse anzeigen (Vorauswahl)

### 2.7 DefenderExclusions-Modul (P1, Opt-in)

**Abhängigkeit:** 2.3 (Elevation erforderlich)

- [x] **P1** `IDefenderExclusionService` Interface: `GetExclusionSetAsync`, `ApplyExclusionsAsync`, `RollbackExclusionsAsync`
- [x] **P1** Exclusion hinzufügen: `Add-MpPreference -ExclusionPath "…"` via ElevationHelper
- [x] **P1** Exclusion entfernen: `Remove-MpPreference -ExclusionPath "…"` via ElevationHelper
- [x] **P1** Vordefinierte Pfade: `%APPDATA%\Cursor`, `%LOCALAPPDATA%\cursor-updater`, `%LOCALAPPDATA%\Programs\cursor` *(kein node_modules)*
- [x] **P1** Tracking: hinzugefügte Exclusions in JSON für Rollback
- [x] **P1** Rollback: hinzugefügte Exclusions wieder entfernen
- [x] **P1** Opt-in-Checkbox prominent im UI — Benutzer muss aktiv zustimmen
- [x] **P1** Warndialog: erklärt Sicherheits-Implikation, erfordert Bestätigung
- [x] **P1** UI in `CursorView` *(kein separates DefenderExclusionsView)*
- [ ] **P2** Pfad-Validierung: existiert der Pfad? Laufwerk korrekt?

### 2.8 Search Indexer Excludes (P1)

**Abhängigkeit:** 2.3 (Registry-Schreibzugriff)

- [x] **P1** `IIndexerExclusionService` Interface: `GetAvailableEntriesAsync`, `ApplyExclusionsAsync`, `RollbackExclusionsAsync`
- [x] **P1** Registry-Pfad: `HKLM\...\Gather\Windows\SystemIndex\ExcludePaths` via ElevationHelper
- [x] **P1** PowerShell-Skript via ElevationHelper: Registry-Keys für Exclusions
- [x] **P1** Vordefinierte Exclusions: Cursor AppData, optionale Dev-Ordner (user-selectable)
- [x] **P1** Snapshot: angewendete Pfade in JSON persistiert
- [x] **P1** Rollback: ExcludePaths-Keys entfernen
- [x] **P1** UI in `CursorView` — Checkbox-Liste für Ordner *(kein separates SearchIndexerView)*
- [ ] **P2** Windows-Suchdienst nach Änderung neu starten (mit Opt-in)

---

## Sprint 3 (W5–6): Snapshot/Rollback, Dashboard, Presets, Monitoring

> **Reihenfolge *(ergänzt)*:** Abschnitt **3.3 (Data Layer)** vor **3.1 (SnapshotManager)** implementieren — Snapshots benötigen SQLite/Repositories.

### 3.1 SnapshotManager (P0)

**Abhängigkeit:** 2.1, **3.3 zuerst** (Data Layer muss vor SnapshotManager fertig sein)

- [x] **P0** `ISnapshotManager` Interface: `CreateBaselineAsync`, `GetSnapshotsAsync`, `GetSnapshotAsync`, `DeleteSnapshotAsync`
- [x] **P0** `SnapshotManager` — serialisiert `BaselineState` als JSON (gzip), speichert in SQLite via `ISnapshotRepository`
- [x] **P0** `SnapshotEntry` enthält: Id (GUID), CreatedAt (UTC), Label, Module, StateJson (compressed), Checksum (SHA-256)
- [x] **P0** Checksum-Validierung beim Laden — korrumpierte Snapshots als invalid markieren
- [x] **P0** Max-Snapshot-Retention konfigurierbar (default: 50) — älteste löschen bei Überschreitung
- [ ] **P1** `SnapshotViewModel` + `SnapshotView.xaml` — Liste aller Snapshots, sortiert nach Datum
- [ ] **P1** Snapshot manuell benennen (Label-Eingabe vor Capture)
- [ ] **P1** Snapshot-Details expandierbar (StateJson pretty-printed)
- [ ] **P1** "Snapshot jetzt erstellen"-Button (manueller Full-System-Snapshot)

### 3.2 RollbackEngine (P0)

**Abhängigkeit:** 3.1, 2.1

- [x] **P0** `IRollbackEngine` Interface: `RollbackSnapshotAsync`, `RollbackModuleAsync`, `RollbackLatestAsync`
- [x] **P0** `RollbackEngine` — findet zuständiges `IOptimizationModule` anhand `snapshot.Module`, ruft `RollbackAsync` auf
- [x] **P0** Modul-Registry: Dictionary `string moduleName → IOptimizationModule` (via DI registriert)
- [x] **P0** Rollback-Ergebnis in `AuditLogger` protokollieren
- [x] **P0** Fehlerfall: partial rollback — so viel wie möglich rückgängig machen, alle Fehler aggregieren
- [ ] **P1** Bestätigungs-Dialog vor Rollback (zeigt: welches Modul, welcher Snapshot-Zeitpunkt)
- [ ] **P1** Progress-Anzeige während Rollback (async, mit CancellationToken)
- [ ] **P1** Nach erfolgreichem Rollback: neuer Snapshot "post-rollback" automatisch erfassen

### 3.3 Data Layer (P0)

**Abhängigkeit:** 1.1, 1.3

- [x] **P0** `DatabaseBootstrap` (Microsoft.Data.Sqlite) in `WindowsPerformance.Data` — statt EF Core
- [x] **P0** Tabellen: `snapshots`, `audit_entries`, `performance_metrics`
- [x] **P0** `ISnapshotRepository`: CRUD für Snapshots
- [x] **P0** `IAuditRepository`: Insert + Query für AuditEntries
- [x] **P0** `IPerformanceMetricRepository`: Insert + Query (Time-Series, Paginierung)
- [x] **P0** DB-Schema-Bootstrap beim ersten Start (`DatabaseBootstrap.InitializeAsync`)
- [x] **P0** DB-Datei-Pfad: `%LOCALAPPDATA%\WindowsPerformance\data.db`
- [x] **P1** `ProfileRepository` — liest/schreibt `ProfileDefinition`-JSON aus `%LOCALAPPDATA%\WindowsPerformance\profiles\`
- [x] **P1** `IProfileRepository` Interface: `GetAllAsync()`, `GetByIdAsync()`, `SaveAsync()`, `DeleteAsync()`
- [x] **P1** JSON-Serialisierung mit `System.Text.Json`, `JsonSerializerOptions` (Indented, camelCase)
- [x] **P1** DB-Initialisierung beim App-Start automatisch (`BootstrapApplicationAsync`)
- [ ] **P2** DB-Backup vor Migrationen (Datei kopieren mit Timestamp-Suffix)

### 3.4 AuditLogger (P1)

**Abhängigkeit:** 3.3

- [x] **P1** `IAuditLogger` Interface: `Task LogAsync(string operation, string module, string details)`
- [x] **P1** `AuditLogger` — schreibt `AuditEntry` in SQLite via `IAuditRepository`
- [x] **P1** Jede Apply/Rollback-Operation wird geloggt (Modul, Zeitpunkt, Erfolg/Fehler, Details)
- [x] **P1** Audit-Log in UI aufrufbar (Einstellungen → Audit-Protokoll)
- [ ] **P2** Audit-Log als CSV exportieren

### 3.5 PerformanceCounterService (P1)

**Abhängigkeit:** 1.3

- [x] **P1** `IMetricsCollector` / `PerformanceCounterService`: `GetCurrentMetricAsync()`, `MetricUpdated` event
- [x] **P1** CPU: `System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total")`
- [x] **P1** RAM: `GlobalMemoryStatusEx` via P/Invoke (`kernel32.dll`)
- [x] **P1** Disk: `PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total")`
- [x] **P1** Polling-Intervall: 2s (konfigurierbar via `AppSettings.MetricsPollingIntervalMs`)
- [x] **P1** Metriken in SQLite schreiben (via `IPerformanceMetricRepository`) — Retention: 7 Tage
- [ ] **P2** `IObservable<PerformanceMetric>` mit `System.Reactive` oder eigenem polling timer

### 3.6 Dashboard (P1)

**Abhängigkeit:** 3.5, 2.1, 2.5

- [x] **P1** `DashboardViewModel` — aggregiert Live-Metriken, Health-Hint, letzter Snapshot
- [x] **P1** `DashboardView.xaml` — Übersichtskarten: CPU, RAM, Disk (Live-Badges)
- [ ] **P1** Modul-Status-Karten: je Modul ein StatusBadge (Active / Inactive / Unknown)
- [x] **P1** Quick-Apply-Button (Cursor Dev Mode)
- [x] **P1** Letzter Snapshot-Zeitpunkt anzeigen
- [ ] **P1** CPU-Sparkline (Mini-Chart, letzten 60 Datenpunkte) — einfach mit `Path`/`Polyline` oder OxyPlot
- [ ] **P2** RAM-Sparkline
- [x] **P2** "Quick Actions"-Panel: Ein-Klick Cursor Dev Mode aktivieren + Rollback

### 3.7 Presets / Cursor Dev Mode (P0)

**Abhängigkeit:** 2.4, 2.5, 2.7, 2.8, 3.1

- [x] **P0** `IPresetOrchestrator` Interface: `GetPresetsAsync()`, `ApplyPresetAsync(string presetId, CancellationToken ct)`
- [x] **P0** `PresetOrchestrator` — führt Modul-Applies in definierter Reihenfolge aus, erstellt vorher Full-Snapshot
- [x] **P0** Built-in Preset: **"Cursor Dev Mode"** — orchestrierte Checklist:
  - [x] **P0** Schritt 1: Full-System-Snapshot erstellen (Label: `before_cursor_dev_mode`)
  - [x] **P0** Schritt 2: PowerPlan auf "High Performance" setzen
  - [x] **P0** Schritt 3: Prozessprioritäten (Cursor/Helper)
  - [ ] **P0** Schritt 4: `node.exe` Priorität auf `Normal` sicherstellen
  - [x] **P0** Schritt 5: Search Indexer Excludes anwenden (node_modules, .git, dist, build)
  - [x] **P0** Schritt 6 (opt-in): Defender-Exclusions anwenden (nur wenn user bereits zugestimmt hat)
  - [x] **P0** Schritt 7: AuditLog-Eintrag für Preset-Aktivierung
  - [x] **P0** Schritt 8: Erfolgs-/Fehler-Zusammenfassung anzeigen
- [x] **P0** Built-in Preset: **"Balanced"** — Cursor zurücksetzen + Ausgewogener Plan
- [x] **P0** Built-in Preset: **"Gaming"** — High Performance Plan
- [ ] **P1** User-definierte Presets: speichern, laden, löschen
- [x] **P1** `PresetsViewModel` + `PresetsView.xaml` — Liste der Presets, Apply-Button, Rollback
- [x] **P1** Preset-Apply zeigt Step-by-Step Progress (jeder Schritt als Listeneintrag mit ✓/✗)
- [ ] **P2** Preset-Import/Export (JSON-Datei)

### 3.8 Einstellungen-View (P2)

**Abhängigkeit:** 1.4

- [x] **P2** `EinstellungenViewModel` + `EinstellungenView.xaml`
- [ ] **P2** App-Theme (nur Dark/Tokyo Night vorerst)
- [ ] **P2** Log-Level konfigurieren (Verbose / Debug / Information / Warning)
- [x] **P2** Snapshot-Retention-Limit konfigurieren
- [ ] **P2** Auto-Start mit Windows Toggle (Registry `HKCU\…\Run`)
- [ ] **P2** PowerShell-Timeout konfigurieren
- [x] **P2** Audit-Protokoll-View integriert in Einstellungen
- [x] **P2** Defender Opt-in Toggle + Default Dev-Ordner (Indexer)
- [x] **P2** About-Dialog (Version aus Assembly)

---

## Testing — MVP

### 4.1 Unit Tests (P1)

**Abhängigkeit:** alle Services implementiert

- [ ] **P1** `PowerShellRunner` Tests: Mock-Prozess, korrekte Argumente, Timeout-Handling
- [x] **P1** `RollbackEngine` Tests: Modul-Dispatch, Fehler-Aggregierung, Partial-Rollback-Verhalten
- [x] **P1** `SnapshotManager` Tests: Checksum-Validierung, Retention-Limit-Enforcement
- [x] **P1** `PresetOrchestrator` Tests: korrekte Reihenfolge der Modul-Applies, Snapshot-Capture vor Apply
- [x] **P1** `ProfileRepository` Tests: JSON-Serialisierung/Deserialisierung round-trip
- [ ] **P2** ViewModel-Tests: `DashboardViewModel`, `PresetsViewModel` (Commands, Properties)

### 4.2 Integrations-Tests (P2)

**Abhängigkeit:** 4.1 Unit Tests grün

- [ ] **P2** `AppDbContext` gegen echte SQLite In-Memory-DB testen
- [ ] **P2** `ElevationHelper.exe` Integration: Build + Start + JSON-Output parsen
- [ ] **P2** `SnapshotManager` ↔ `RollbackEngine` End-to-End mit Fake-Modul
- [ ] **P2** `PowerShellRunner` gegen `pwsh.exe` auf CI-Maschine (wenn verfügbar)

### 4.3 CI Pipeline (P2)

- [ ] **P2** `.github/workflows/ci.yml` anlegen (dotnet restore → build → test)
- [ ] **P2** GitHub Actions Runner: `windows-latest`
- [ ] **P2** Test-Coverage-Report (Coverlet) als Artefakt
- [ ] **P2** Build-Badge in README

### 4.4 MVP Portable Distribution (P1)

**Abhängigkeit:** 2.3 (ElevationHelper-Deploy), 1.4 (Shell lauffähig)

- [ ] **P1** MVP portable ZIP-Distribution: `dotnet publish` (win-x64, self-contained optional) → Ordner mit App + ElevationHelper + README bündeln; interim bis Phase-3-Installer *(ergänzt)*
- [ ] **P1** Publish-Skript oder MSBuild-Target: ZIP-Artefakt `WindowsPerformance-<version>-win-x64.zip` erzeugen *(ergänzt)*

---

## Dokumentation — MVP

- [ ] **P1** `README.md` anlegen: Projekt-Überblick, Voraussetzungen, Build-Anleitung, Quick-Start
- [ ] **P1** Inline-XML-Dokumentation für alle `interface`-Mitglieder in Core + Services
- [ ] **P2** `CHANGELOG.md` initialisieren (Keep a Changelog Format)
- [ ] **P2** `docs/architecture.md` — Solution-Struktur, Datenfluss-Diagramm, Elevation-Flow
- [ ] **P2** `docs/presets.md` — Dokumentation aller Built-in-Presets + Konfiguration

---

---

# Phase 2 (W7–14): Erweiterte Module

> **Voraussetzung:** MVP vollständig und stabil. Alle MVP-Tests grün.

### 5.1 Services-Manager (P1 in Phase 2)

- [ ] `IWindowsServiceManager` Interface: `GetServicesAsync()`, `SetStartupTypeAsync(string name, ServiceStartMode mode)`
- [ ] Liste der Dienste laden (Name, DisplayName, Status, StartupType) via `ServiceController`
- [ ] Preset-Regeln für Dienste (z. B. SysMain/Superfetch auf Demand für Dev-Maschinen)
- [ ] Snapshot + Rollback für Service-Starttypen
- [ ] Opt-in UI mit ausführlicher Warnung (Dienste-Änderungen können System destabilisieren)
- [ ] `ServicesViewModel` + `ServicesView.xaml`

### 5.2 Startup-Programme (P1 in Phase 2)

- [ ] `IStartupManagerService`: Startup-Einträge aus Registry + `shell:startup`-Ordner lesen
- [ ] Einträge deaktivieren/aktivieren (HKCU + HKLM)
- [ ] Snapshot + Rollback
- [ ] `StartupViewModel` + `StartupView.xaml` — DataGrid mit Enable/Disable Toggle

### 5.3 Visual Effects Optimizer (P2 in Phase 2)

- [ ] `IVisualEffectsService`: `SystemParametersInfo` P/Invoke für Animations-Flags
- [ ] Preset: "Performance" (alle Effekte deaktivieren), "Balanced", "Best Appearance"
- [ ] Registry-Einträge: `HKCU\Control Panel\Desktop\UserPreferencesMask` etc.
- [ ] Snapshot + Rollback
- [ ] UI: drei RadioButtons + Apply

### 5.4 Memory Optimizer (P2 in Phase 2)

- [ ] `IMemoryOptimizerService`: Standby-Liste via `NtSetSystemInformation` leeren (undokumentierte API, nur mit Elevation)
- [ ] Klare Warnung: temporärer Effekt, kann Performance kurzzeitig verschlechtern
- [ ] Kein Scheduler für Auto-Memory-Flush (Out of Scope MVP, optional Phase 2)
- [ ] `MemoryViewModel` + `MemoryView.xaml` — manueller "Flush"-Button + RAM vor/nach Anzeige

### 5.5 Network Optimizer (P2 in Phase 2)

- [ ] `INetworkOptimizerService`: Nagle-Algorithmus deaktivieren (TCP `NoDelay`), DNS-Cache-TTL anpassen
- [ ] Registry: `HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters`
- [ ] Snapshot + Rollback (Registry-Snapshot)
- [ ] `NetworkViewModel` + `NetworkView.xaml`

### 5.6 HealthScorer (P1 in Phase 2)

- [ ] `IHealthScorerService`: berechnet 0–100 Score aus: aktiver PowerPlan, Cursor-Priority, Search-Exclusions, Defender-Exclusions (falls opted-in), Dienste-Status
- [ ] Score-Berechnung regel-basiert (gewichtete Summe)
- [ ] Score auf Dashboard prominent anzeigen (Gauge oder numerisch mit Farbcode)
- [ ] Score-Breakdown: welche Faktoren ziehen ab?

### 5.7 Trend-Analyse (P2 in Phase 2)

- [ ] `ITrendAnalysisService`: Zeitreihen aus SQLite aggregieren (5-Min-Buckets, 1h, 24h)
- [ ] `TrendViewModel` + `TrendView.xaml` — Linien-Charts (OxyPlot oder LiveCharts2)
- [ ] CPU-, RAM-, Disk-Trends über Zeit anzeigen
- [ ] Annotations: Zeitpunkte von Optimierungs-Applies markieren

### 5.8 Process Inspector (P2 in Phase 2)

- [ ] `IProcessInspectorService`: detaillierte Prozess-Infos (Handle-Count, Thread-Count, IO-Bytes, Working Set)
- [ ] Filter nach Cursor-relevanten Prozessen (cursor, node, git, eslint, tsc)
- [ ] `ProcessInspectorViewModel` + `ProcessInspectorView.xaml`
- [ ] Kill-Prozess-Funktion (mit Bestätigung)

---

# Phase 3 (W15+): Erweiterte Features

> **Voraussetzung:** Phase 2 vollständig und stabil.

### 6.1 Disk Optimizer (P2)

- [ ] `IDiskOptimizerService`: Prefetch/Superfetch-Einstellungen, TRIM für SSDs, Write-Caching
- [ ] Defragmentierungs-Status anzeigen (nicht triggern — zu lang)
- [ ] Snapshot + Rollback
- [ ] `DiskOptimizerViewModel` + View

### 6.2 Task Scheduler Integration (P2)

- [ ] `ITaskSchedulerService`: Geplante Tasks lesen/deaktivieren (z. B. Windows Update Delivery Optimization während Dev)
- [ ] Preset: "Dev-Zeit-Schutz" — störende scheduled Tasks temporär deaktivieren
- [ ] Snapshot + Rollback
- [ ] Warnung: Tasks werden nach Ablauf automatisch re-enabled (konfigurierbare Dauer)

### 6.3 ML-basierte Empfehlungen (P3)

- [ ] Historische Metriken analysieren (SQLite → ML.NET Modell)
- [ ] Anomalie-Erkennung: ungewöhnliche CPU-Spikes korreliert mit aktiven Prozessen
- [ ] Empfehlungs-Engine: "Gestern um 14 Uhr war CPU bei 95% — `antimalware_service_executable` war schuld. Empfehle Defender-Exclusion für Workspace."
- [ ] Modell lokal trainieren (keine Cloud-Daten)

### 6.4 Installer (P2)

- [ ] MSIX-Paket oder WiX-Installer erstellen
- [ ] Code-Signierung (EV-Zertifikat für Produktion)
- [ ] Auto-Update-Mechanismus (Squirrel.Windows oder MSIX Auto-Update)
- [ ] Uninstaller: alle App-Daten entfernen (opt-in), alle Optimierungen rollbacken

### 6.5 Registry Tuner — opt-in (P3)

- [ ] Opt-in, explizite Benutzer-Zustimmung pro Tweak erforderlich
- [ ] Nur bewährte, dokumentierte Registry-Tweaks (keine "Placebo"-Keys)
- [ ] Jede Änderung einzeln snapshot-gesichert
- [ ] Rollback-Garantie 100% vor Aktivierung

---

---

## Out-of-Scope (explizit, MVP und darüber hinaus)

Diese Items werden bewusst **nicht** implementiert:

- [ ] ~~Registry-Placebo-Tweaks~~ — Keys ohne nachweisbaren Effekt werden nicht angefasst
- [ ] ~~`node_modules`-Verzeichnis als Defender-Exclusion~~ — Sicherheitsrisiko, nicht recommended
- [ ] ~~`EmptyWorkingSet` für `cursor.exe` / `node.exe`~~ — führt zu Working-Set-Thrashing, verschlechtert Performance
- [ ] ~~Automatisches Deaktivieren von Windows-Diensten~~ — zu destruktiv; manuelle Opt-in-Kontrolle in Phase 2
- [ ] ~~App läuft dauerhaft als Administrator~~ — Elevation nur on-demand via ElevationHelper.exe
- [ ] ~~Cloud-Sync von Profilen/Metriken~~ — alle Daten lokal, keine externen API-Calls
- [ ] ~~Browser-Performance-Tweaks~~ — außerhalb des definierten Scopes
- [ ] ~~Linux/macOS-Support~~ — Windows-only by design
- [ ] ~~In-Process PowerShell (System.Management.Automation NuGet)~~ — externer pwsh.exe-Prozess ist Sicherheitsprinzip

---

## Risiken & Mitigationen

| Risiko | Wahrscheinlichkeit | Impact | Mitigation |
|--------|--------------------|--------|------------|
| ElevationHelper UAC-Prompt erschreckt Nutzer | Mittel | Hoch | UI erklärt klar warum Elevation nötig; Aktion erst nach User-Bestätigung |
| Rollback schlägt fehl (Prozess hält Handle) | Niedrig | Hoch | Partial-Rollback-Muster; Fehler aggregieren; Nutzer informieren; manueller Fallback-Guide |
| Defender-Exclusion schafft Sicherheitslücke | Mittel | Mittel | Opt-in + Warndialog; nur workspace-spezifische Pfade; nie system-weite Exclusions |
| PowerShell-Timeout bei langsamen Systemen | Niedrig | Niedrig | Timeout konfigurierbar (default 30s → 60s für ältere Hardware) |
| EF-Core-Migration bricht DB-Schema | Niedrig | Mittel | DB-Backup vor Migration; Migrations-Script reviewed vor Release |
| Process.GetProcessesByName liefert falsches cursor.exe | Niedrig | Niedrig | Prozess-Pfad zusätzlich validieren (MainModule.FileName) |
| SQLite-Datei korrumpiert | Sehr niedrig | Mittel | WAL-Modus aktivieren; periodisches Backup; Rebuild-Option im UI |
| `pwsh.exe` nicht installiert | Niedrig | Hoch | Startup-Check: Fallback auf `powershell.exe` (Windows PowerShell 5.1) mit Warnung |

---

## Sicherheit & Elevation-Architektur

- [ ] **P0** App läuft **niemals** mit `requireAdministrator`-Manifest
- [ ] **P0** Nur `ElevationHelper.exe` hat `requireAdministrator`; wird als separater Prozess gestartet
- [ ] **P0** Alle elevated Operationen gehen durch Whitelist im ElevationHelper (Script-Hash-Check)
- [ ] **P0** Keine Credentials, Tokens oder Passwörter werden gespeichert
- [ ] **P1** Eingaben für PowerShell-Skripte werden sanitized (keine User-kontrollierten Skript-Inhalte)
- [ ] **P1** Snapshot-Checksums verhindern Tampering mit gespeicherten States
- [ ] **P1** SQLite-Datei liegt in `%LOCALAPPDATA%` — kein Netzwerk-Zugriff
- [ ] **P2** ElevationHelper.exe signiert (SHA-256 Authenticode)
- [ ] **P2** Audit-Log ist append-only (keine Update/Delete-Operationen auf AuditEntries)

---

*Zuletzt aktualisiert: 2026-07-05 — Audit-Ergänzungen (Git, Deploy, DI, Fehler-UI, Reihenfolge 3.3→3.1); Sprint 1 noch nicht begonnen.*
