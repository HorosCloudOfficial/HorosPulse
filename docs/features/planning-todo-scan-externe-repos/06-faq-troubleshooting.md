# FAQ & Troubleshooting

> **Typ:** Anleitung (Diataxis)  
> **Zielgruppe:** Entwickler, die auf Integrationsprobleme oder Designfragen stoßen

---

## Allgemeine Fragen

### Warum Central Package Management und keine einzelnen `<PackageReference Version="…">`?

`Directory.Packages.props` (Central Package Management) stellt sicher, dass alle Projekte in der Solution dieselbe Paketversion verwenden. Bei einer Solution mit 8 Projekten und mehreren geteilten Abhängigkeiten verhindert das Versions-Drift (z. B. `Serilog 3.x` in App, `Serilog 4.x` in Services). Einmal aktualisieren — überall konsistent.

Offizielle Doku: [Central Package Management (NuGet)](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)

---

### Wann ist ein Paket "Dev-Only" und was bedeutet das konkret?

`PrivateAssets="all"` verhindert, dass das Paket in der `*.deps.json`-Datei des Publish-Outputs erscheint und nicht mit der App ausgeliefert wird. Für `coverlet.collector` ist das korrekt — Coverage-Collection läuft nur in der Testumgebung, nie in der produzierten App.

```xml
<PackageReference Include="coverlet.collector">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

---

### Ich sehe `LiveChartsCore.SkiaSharpView.WPF` Version `2.0.0-rc4` — ist das stabil genug?

Ja. LiveCharts2 ist seit Ende 2024 de facto API-stabil. Die `rc`-Bezeichnung ist historisch gewachsen und wird nicht mehr aktiv gepflegt, die Library aber schon. Für einen Produktionseinsatz mit WPF .NET 9 ist `2.0.0-rc4` die empfohlene Version (kein stabiler `2.0.0` ohne rc-Suffix existiert stand 2026-07). Vor einem Phase-2-Start mit `dotnet package search LiveChartsCore.SkiaSharpView.WPF` auf eine eventuell erschienene `2.x` stable prüfen.

---

### Brauche ich `Vanara.Core` zusätzlich zu `Vanara.PInvoke.NtDll`?

`Vanara.PInvoke.NtDll` deklariert `Vanara.Core` als transitive Abhängigkeit — NuGet löst das automatisch auf. Kein expliziter `<PackageReference>` für `Vanara.Core` nötig.

---

## SKIP-Begründungen im Detail

### Warum kein MaterialDesignInXamlToolkit?

`TODO.md` nennt es als Option: `MaterialDesignInXamlToolkit oder eigene Styles`. Die Entscheidung fiel für eigene Styles, weil:

1. **Tokyo-Night ist bereits implementiert** (`/Themes/TokyoNight.xaml` mit allen Basis-Controls).
2. **Paket-Overhead:** MaterialDesign bringt ~15 MB Icons + Styles mit. Bei einer App, die nur ~10 Controls benötigt, ist das unverhältnismäßig.
3. **Theme-Konflikte:** MaterialDesign überschreibt Control-Templates pauschal. Nachträgliche Einführung würde alle bestehenden Styles invalidieren.
4. **Keine Rechtfertigung:** Es gibt kein TODO-Item, das MaterialDesign spezifisch benötigt.

**Fazit:** SKIP endgültig. Bei einer zukünftigen kompletten UI-Neuentwicklung kann die Entscheidung neu getroffen werden.

---

### Warum kein Squirrel.Windows?

`TODO.md` 6.4 nennt `Squirrel.Windows oder MSIX Auto-Update`. Squirrel.Windows wird seit 2021 nicht mehr aktiv gepflegt. Velopack ist der direkte Nachfolger:

- Entwickelt von denselben Autoren (caesay)
- API sehr ähnlich zu Squirrel (Migration straightforward)
- Aktiv gepflegt (2025–2026)
- Bessere .NET 9 / WPF-Unterstützung
- Delta-Updates out of the box

Migration von Squirrel zu Velopack: [velopack.io/docs/migrating](https://velopack.io/docs/migrating)

---

### Warum kein ETW / TraceEvent?

ETW (Event Tracing for Windows) via `Microsoft.Diagnostics.Tracing.TraceEvent` ermöglicht Kernel-Level-Tracing, was weit über die Anforderungen von WindowsPerformance hinausgeht:

- `System.Diagnostics.PerformanceCounter` liefert CPU%, RAM, Disk% mit ausreichender Granularität (2s-Polling).
- ETW-Sessions erfordern Admin-Rechte und sind komplex einzurichten.
- Der App-Scope ist Optimierung, nicht Diagnose-/Profiling-Tooling.

Wenn zukünftig Kernel-Events benötigt werden (z. B. detailliertes I/O-Tracing), kann ETW neu evaluiert werden.

---

### Warum kein Windows Performance Toolkit SDK?

Das WPT-SDK (Teil des Windows Assessment and Deployment Kit) ist ein Diagnose-Framework für Systemadministratoren und Performance-Ingenieure:

- Kein NuGet-Paket — erfordert ADK-Installation auf dem Zielrechner
- Kein API-Einstiegspunkt für eingebettete Desktop-Apps
- Scope: Offline-Analyse von ETL-Traces (WPA/Xperf), nicht Live-Optimierung
- Für WindowsPerformance irrelevant

---

### Warum keine MSO-Scripts?

MSO (Microsoft Office) Skripte sind für Office-Automatisierung via JavaScript/TypeScript in Excel/Word-Kontext. Kein Bezug zu Windows-System-Performance.

---

## Integrationsprobleme

### `H.NotifyIcon.Wpf` — Tray-Icon erscheint nicht

**Symptom:** `<tb:TaskbarIcon>` im XAML definiert, aber kein Icon in der Taskleiste sichtbar.

**Ursache 1:** Icon-Datei nicht als `Resource` markiert.
```xml
<!-- WindowsPerformance.App.csproj -->
<Resource Include="Resources\tray.ico" />
```

**Ursache 2:** `TaskbarIcon` als Application-Ressource, aber `FormsNotifyIcon` gleichzeitig aktiv (Konflikt).
→ Nur eine NotifyIcon-Implementierung verwenden.

**Ursache 3:** App wird ohne `STAThread` gestartet (unwahrscheinlich bei WPF).

---

### `LiveChartsCore` — Chart wird nicht gerendert (leere Fläche)

**Symptom:** `CartesianChart` im Layout sichtbar, aber keine Linie/Balken.

**Ursache 1:** `Series`-Collection ist `null` statt leer.
→ Immer mit `ISeries[] Series { get; } = Array.Empty<ISeries>();` initialisieren.

**Ursache 2:** `Values` der Serie ist nicht `ObservableCollection<T>`.
→ LiveCharts benötigt eine Collection, die `INotifyCollectionChanged` implementiert.

**Ursache 3:** Fehlende Namespace-Deklaration im XAML.
```xml
xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.WPF;assembly=LiveChartsCore.SkiaSharpView.WPF"
```

---

### `Vanara.PInvoke.NtDll` — `NtSetSystemInformation` gibt `STATUS_PRIVILEGE_NOT_HELD` zurück

**Symptom:** Methode wirft Exception oder gibt Fehler-NTSTATUS zurück.

**Ursache:** Die Operation wird nicht durch den `WindowsPerformance.Elevation.exe` ausgeführt, sondern direkt im App-Prozess (ohne Admin-Rechte).

**Lösung:** `MemoryOptimizerService.PurgeStandbyListAsync()` muss über Named-Pipe-IPC an `WindowsPerformance.Elevation.exe` delegieren — nicht direkt aufgerufen werden. Das Elevation-Helper-Projekt bekommt die Vanara-Abhängigkeit.

---

### `Microsoft.Windows.CsWin32` — Generierter Code fehlt nach `dotnet restore`

**Symptom:** `NativeMethods.g.cs` existiert nicht, Kompilierung schlägt fehl.

**Ursache:** `NativeMethods.txt` fehlt oder ist leer, oder die Datei ist nicht im Projektverzeichnis (nicht im Solution-Root).

**Lösung:**
1. `NativeMethods.txt` muss im **gleichen Verzeichnis** wie die `.csproj`-Datei liegen.
2. Eintrag im Csproj prüfen: `<AdditionalFiles Include="NativeMethods.txt" />` ist optional — CsWin32 sucht die Datei automatisch.
3. `dotnet build` erzwingt Source-Generator-Ausführung; `dotnet restore` allein reicht nicht.

---

### `TaskScheduler` (dahall) — COM-Exception auf Windows 10 Home

**Symptom:** `COMException: Access denied` beim Lesen von Tasks.

**Ursache:** Einige Tasks unter `\Microsoft\Windows\` erfordern Admin-Rechte, auch zum Lesen.

**Lösung:** Nur Tasks unter `\` (Root) oder benutzereigene Tasks lesen ohne Elevation. Für HKLM-System-Tasks über ElevationHelper delegieren. Das `ITaskSchedulerService` muss diese Unterscheidung treffen.
