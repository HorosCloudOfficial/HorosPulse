# Einstellungen: NuGet-Versionen & Integration-Typen

> **Typ:** Referenz (Diataxis)  
> **Zielgruppe:** Entwickler, die Central Package Management befüllen

---

## Central Package Management (`Directory.Packages.props`)

Alle empfohlenen Pakete als fertige `<PackageVersion>`-Einträge, nach Phase geordnet. Einfach in `D:\HorosPulse\Directory.Packages.props` einfügen:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup Label="MVP-Pakete">
    <!-- Tray-Icon -->
    <PackageVersion Include="H.NotifyIcon.Wpf" Version="2.1.0" />

    <!-- Charts & Sparklines -->
    <PackageVersion Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc4" />

    <!-- Test-Coverage (Dev-Only) -->
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>

  <ItemGroup Label="Phase-2-Pakete (noch nicht aktiv)">
    <!-- P/Invoke-Wrapper für NtDll (Memory Optimizer) -->
    <PackageVersion Include="Vanara.PInvoke.NtDll" Version="4.0.3" />
    <!-- Windows-API-Source-Generator -->
    <PackageVersion Include="Microsoft.Windows.CsWin32" Version="0.3.106" />
    <!-- Task-Scheduler-Verwaltung -->
    <PackageVersion Include="TaskScheduler" Version="2.11.1" />
  </ItemGroup>

  <ItemGroup Label="Phase-3-Pakete (noch nicht aktiv)">
    <!-- Auto-Updater -->
    <PackageVersion Include="Velopack" Version="0.0.1073" />
    <!-- Machine Learning -->
    <PackageVersion Include="Microsoft.ML" Version="4.0.0" />
    <PackageVersion Include="Microsoft.ML.TimeSeries" Version="4.0.0" />
  </ItemGroup>
</Project>
```

> Pakete unter `Label="Phase-2/3-Pakete"` sind **noch nicht in `<PackageReference>` eingetragen** — sie stehen nur als Versions-Platzhalter. Erst beim jeweiligen Phase-Start die `<PackageReference>` in den zugehörigen Projekten hinzufügen.

---

## Versionstabelle

| Paket | Version | .NET 9 | WPF | Lizenz | NuGet-Link |
|-------|---------|--------|-----|--------|------------|
| `H.NotifyIcon.Wpf` | 2.1.0 | ✓ | ✓ | MIT | [nuget.org](https://www.nuget.org/packages/H.NotifyIcon.Wpf) |
| `LiveChartsCore.SkiaSharpView.WPF` | 2.0.0-rc4 | ✓ | ✓ | MIT | [nuget.org](https://www.nuget.org/packages/LiveChartsCore.SkiaSharpView.WPF) |
| `coverlet.collector` | 6.0.2 | ✓ | n/a | MIT | [nuget.org](https://www.nuget.org/packages/coverlet.collector) |
| `Vanara.PInvoke.NtDll` | 4.0.3 | ✓ | n/a | MIT | [nuget.org](https://www.nuget.org/packages/Vanara.PInvoke.NtDll) |
| `Microsoft.Windows.CsWin32` | 0.3.106 | ✓ | n/a | MIT | [nuget.org](https://www.nuget.org/packages/Microsoft.Windows.CsWin32) |
| `TaskScheduler` | 2.11.1 | ✓ | n/a | MIT | [nuget.org](https://www.nuget.org/packages/TaskScheduler) |
| `Velopack` | 0.0.1073 | ✓ | ✓ | MIT | [nuget.org](https://www.nuget.org/packages/Velopack) |
| `Microsoft.ML` | 4.0.0 | ✓ | n/a | MIT | [nuget.org](https://www.nuget.org/packages/Microsoft.ML) |
| `Microsoft.ML.TimeSeries` | 4.0.0 | ✓ | n/a | MIT | [nuget.org](https://www.nuget.org/packages/Microsoft.ML.TimeSeries) |

> Versionen auf Stand 2026-07-06. Vor dem Eintragen `dotnet package search <paket>` ausführen um neueste stabile Version zu prüfen.

---

## Integration-Typen

| Typ | Beschreibung | Beispiel |
|-----|-------------|---------|
| **NuGet (Runtime)** | Normales Laufzeit-Paket; landet im Output-Verzeichnis | `H.NotifyIcon.Wpf`, `LiveChartsCore` |
| **NuGet (Dev-Only)** | Nur für Build/Test; `PrivateAssets="all"` setzt das durch | `coverlet.collector` |
| **SourceGen (Compile-Time)** | Generiert C#-Code bei der Kompilierung; kein Runtime-Overhead | `Microsoft.Windows.CsWin32` |
| **NuGet (P/Invoke-Wrapper)** | Fertige typsichere Win32-Bindings; ersetzt manuelles `[DllImport]` | `Vanara.PInvoke.NtDll` |

---

## Projekt-Zuordnung

Welches Paket kommt in welches `.csproj`-Projekt?

| Paket | Projekt(e) |
|-------|-----------|
| `H.NotifyIcon.Wpf` | `HorosPulse.App` |
| `LiveChartsCore.SkiaSharpView.WPF` | `HorosPulse.App` |
| `coverlet.collector` | `HorosPulse.Tests.Unit`, `HorosPulse.Tests.Integration` |
| `Vanara.PInvoke.NtDll` | `HorosPulse.Services` (oder `HorosPulse.Elevation`) |
| `Microsoft.Windows.CsWin32` | `HorosPulse.Services`, `HorosPulse.Elevation` |
| `TaskScheduler` | `HorosPulse.Services` |
| `Velopack` | `HorosPulse.App` |
| `Microsoft.ML` / `Microsoft.ML.TimeSeries` | `HorosPulse.Services` (neues `AnalyticsService`) |

---

## Komurasoft-Elevation-Pattern

Kein NuGet-Paket — ein **Referenz-Architekturmuster** für UAC-Elevation in WPF-Anwendungen:

- Hauptprozess läuft **ohne** `requireAdministrator`-Manifest (Standard-Benutzerrechte).
- Elevation-Operationen werden an ein separates `HorosPulse.Elevation.exe` delegiert (eigenes Manifest: `requireAdministrator`).
- Kommunikation via **Named Pipe** (IPC): JSON-kodierte Befehle, JSON-Antworten.
- Helper beendet sich nach jeder UAC-Session selbst (kein dauerhafter Admin-Prozess).

HorosPulse implementiert dieses Muster bereits vollständig in `HorosPulse.Elevation` (TODO 2.3). Die Referenz dient als Qualitäts-Benchmark — die Implementierung ist konform.

GitHub-Referenz: Komurasoft ist ein japanischer WPF-Entwickler, bekannt für saubere Separation von Elevation in Desktop-Apps. Das Muster ist Community-Best-Practice und kein offizielles Framework.
