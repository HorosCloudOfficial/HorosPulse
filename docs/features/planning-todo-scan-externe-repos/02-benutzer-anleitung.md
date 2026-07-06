# Benutzer-Anleitung: Empfohlene Repos integrieren

> **Typ:** Anleitung (Diataxis)  
> **Zielgruppe:** Entwickler / Maintainer (kein End-User-Guide)

---

## Voraussetzungen

- .NET 9 SDK installiert
- Solution unter `D:\WindowsPerformance` lauffähig (`dotnet build` grün)
- `Directory.Packages.props` angelegt (TODO 1.1 P1 — Central Package Management)
- PowerShell 7 (`pwsh.exe`) verfügbar

---

## MVP-Pakete integrieren

### 1. H.NotifyIcon.Wpf — Tray-Icon

**Benötigt für:** TODO 1.4 P1 — „Tray-Icon (NotifyIcon via `Hardcoders.NotifyIcon.Wpf` oder `H.NotifyIcon`) — App in Tray minimieren"

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="H.NotifyIcon.Wpf" Version="2.1.0" />
```

```xml
<!-- src/WindowsPerformance.App/WindowsPerformance.App.csproj -->
<PackageReference Include="H.NotifyIcon.Wpf" />
```

**App.xaml** — TaskbarIcon-Ressource registrieren:

```xml
<tb:TaskbarIcon x:Key="TrayIcon"
                IconSource="/Resources/tray.ico"
                ToolTipText="WindowsPerformance"
                DoubleClickCommand="{Binding ShowWindowCommand}" />
```

**MainWindow.xaml.cs** — Minimize-to-Tray-Pattern:

```csharp
protected override void OnStateChanged(EventArgs e)
{
    if (WindowState == WindowState.Minimized)
        Hide();
    base.OnStateChanged(e);
}
```

> Icon-Datei unter `src/WindowsPerformance.App/Resources/tray.ico` ablegen; Build-Action `Resource`.

---

### 2. LiveChartsCore.SkiaSharpView.WPF — Sparklines & Trends

**Benötigt für:** TODO 3.6 P1 (CPU-Sparkline), TODO 5.7 P2 (Trend-Analyse)

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc4" />
```

```xml
<!-- src/WindowsPerformance.App/WindowsPerformance.App.csproj -->
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" />
```

**XAML — Mini-Sparkline (CartesianChart):**

```xml
<lvc:CartesianChart Series="{Binding CpuSeries}"
                    XAxes="{Binding HiddenAxes}"
                    YAxes="{Binding HiddenAxes}"
                    Height="48" />
```

**ViewModel:**

```csharp
public ISeries[] CpuSeries { get; } = [
    new LineSeries<double>
    {
        Values = new ObservableCollection<double>(),
        Fill = null,
        GeometrySize = 0,
        Stroke = new SolidColorPaint(SKColors.Amber) { StrokeThickness = 1.5f }
    }
];
```

> Polling-Intervall: 2 s über `PerformanceCounterService.MetricUpdated`-Event → ViewModel aktualisiert `Values`.

---

### 3. coverlet.collector — Test-Coverage

**Benötigt für:** TODO 4.3 P2 — „Test-Coverage-Report (Coverlet) als Artefakt"

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="coverlet.collector" Version="6.0.2" />
```

```xml
<!-- tests/WindowsPerformance.Tests.Unit/WindowsPerformance.Tests.Unit.csproj -->
<PackageReference Include="coverlet.collector">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

**CLI-Aufruf:**

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

**GitHub Actions (`.github/workflows/ci.yml`):**

```yaml
- name: Run tests with coverage
  run: dotnet test --collect:"XPlat Code Coverage" --results-directory coverage

- name: Upload coverage
  uses: actions/upload-artifact@v4
  with:
    name: coverage-report
    path: coverage/
```

---

## Phase-2-Pakete vorbereiten

### 4. Vanara.PInvoke.NtDll — Standby-Liste leeren

**Benötigt für:** TODO 5.4 — `NtSetSystemInformation` mit `SystemMemoryListInformation`

```xml
<!-- Directory.Packages.props — erst hinzufügen wenn Phase 2 startet -->
<PackageVersion Include="Vanara.PInvoke.NtDll" Version="4.0.3" />
```

```xml
<!-- src/WindowsPerformance.Services/WindowsPerformance.Services.csproj -->
<PackageReference Include="Vanara.PInvoke.NtDll" />
```

**Verwendung im `MemoryOptimizerService` (Elevation erforderlich):**

```csharp
using static Vanara.PInvoke.NtDll;

public async Task PurgeStandbyListAsync()
{
    var command = SYSTEM_MEMORY_LIST_COMMAND.MemoryPurgeStandbyList;
    var status = NtSetSystemInformation(
        SYSTEM_INFORMATION_CLASS.SystemMemoryListInformation,
        ref command, sizeof(SYSTEM_MEMORY_LIST_COMMAND));

    if (status.Failed)
        throw new InvalidOperationException($"NtSetSystemInformation fehlgeschlagen: {status}");
}
```

> Diese Operation **muss** durch den `WindowsPerformance.Elevation.exe`-Prozess ausgeführt werden (SE_INCREASE_QUOTA_PRIVILEGE erforderlich).

---

### 5. Microsoft.Windows.CsWin32 — Windows-API-Bindings

**Benötigt für:** TODO 5.3 (`SystemParametersInfo`), TODO 5.5 (Nagle-TCP-Registry)

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Microsoft.Windows.CsWin32" Version="0.3.106" />
```

```xml
<!-- Nur in Projekten, die Windows-APIs benötigen (Services, Elevation) -->
<PackageReference Include="Microsoft.Windows.CsWin32" PrivateAssets="all" />
```

**`NativeMethods.txt` im Projekt anlegen:**

```
SystemParametersInfo
RegSetValueEx
RegQueryValueEx
```

CsWin32 generiert daraus typsichere P/Invoke-Wrapper zur Kompilierzeit. Kein manuelles `[DllImport]` mehr nötig.

---

### 6. TaskScheduler (dahall) — Geplante Tasks

**Benötigt für:** TODO 6.2 — `ITaskSchedulerService`

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="TaskScheduler" Version="2.11.1" />
```

```xml
<!-- src/WindowsPerformance.Services/WindowsPerformance.Services.csproj -->
<PackageReference Include="TaskScheduler" />
```

**Verwendung:**

```csharp
using Microsoft.Win32.TaskScheduler;

using var ts = new TaskService();
var task = ts.GetTask(@"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan");
task.Definition.Settings.Enabled = false;
task.RegisterChanges();
```

> Snapshot vor dem Deaktivieren: aktuellen `Enabled`-Status pro Task speichern (JSON in `%LOCALAPPDATA%\WindowsPerformance\task-scheduler-state.json`).

---

## Phase-3-Pakete planen

### 7. Velopack — Auto-Update

**Benötigt für:** TODO 6.4 — Auto-Update-Mechanismus

```xml
<!-- Directory.Packages.props — Phase 3 -->
<PackageVersion Include="Velopack" Version="0.0.1073" />
```

**Minimal-Setup in `App.xaml.cs`:**

```csharp
VelopackApp.Build().Run();
```

**Update-Check beim Start:**

```csharp
var mgr = new UpdateManager("https://releases.example.com/windowsperformance/");
var newVersion = await mgr.CheckForUpdatesAsync();
if (newVersion != null)
    await mgr.DownloadUpdatesAsync(newVersion);
```

> Velopack benötigt ein eigenes Build-Target (`vpk pack`). Erst bei Phase-3-Installer relevant.

---

### 8. Microsoft.ML — ML-Empfehlungen

**Benötigt für:** TODO 6.3 — Anomalie-Erkennung auf historischen Metriken

```xml
<!-- Directory.Packages.props — Phase 3 -->
<PackageVersion Include="Microsoft.ML" Version="4.0.0" />
<PackageVersion Include="Microsoft.ML.TimeSeries" Version="4.0.0" />
```

**Konzept — SrCnn-Anomalie-Erkennung auf CPU-Zeitreihe:**

```csharp
var pipeline = mlContext.Transforms.DetectAnomalyBySrCnn(
    outputColumnName: "Prediction",
    inputColumnName: nameof(PerformanceMetric.CpuPercent),
    windowSize: 64,
    backAddWindowSize: 5,
    lookaheadWindowSize: 5,
    averagingWindowSize: 3,
    judgementWindowSize: 21,
    threshold: 0.3);
```

> Modell lokal trainieren — keine Cloud-Calls. Trainingsdaten aus SQLite (`performance_metrics`-Tabelle, Retention 7 Tage).

---

## Entscheidungsbaum: Wann welches Paket?

```
Neues TODO-Item auftauchen?
│
├─ Betrifft UI-Feedback (Charts, Graphs)?  → LiveChartsCore
├─ Betrifft System-Tray?                   → H.NotifyIcon.Wpf
├─ Betrifft Windows-API (P/Invoke)?        → CsWin32 (Codegen) oder Vanara (fertige Wrapper)
│    └─ Ist es NtDll/undokumentiert?       → Vanara.PInvoke.NtDll
├─ Betrifft Task-Scheduler?                → TaskScheduler (dahall)
├─ Betrifft Installer/Update?              → Velopack
├─ Betrifft ML/Anomalie?                   → Microsoft.ML.TimeSeries
└─ Betrifft Test-Coverage?                 → coverlet.collector
```
