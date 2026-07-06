# API & Datenmodell: Vollständige Repo-Tabelle

> **Typ:** Referenz (Diataxis)  
> **Zielgruppe:** Entwickler, Reviewer, Abhängigkeits-Audits

---

## Empfohlene Repos — Vollständige Tabelle

| # | Paket / Repo | GitHub-URL | NuGet | Verdict | Integrationstyp | Phase | TODO-Referenz |
|---|---|---|---|---|---|---|---|
| 1 | `H.NotifyIcon.Wpf` | [HavenDV/H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon) | [nuget](https://www.nuget.org/packages/H.NotifyIcon.Wpf) | **NEED** | NuGet (Runtime) | MVP | 1.4 P1 |
| 2 | `LiveChartsCore.SkiaSharpView.WPF` | [beto-rodriguez/LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) | [nuget](https://www.nuget.org/packages/LiveChartsCore.SkiaSharpView.WPF) | **NEED** | NuGet (Runtime) | MVP / P2 | 3.6 P1, 5.7 P2 |
| 3 | `coverlet.collector` | [coverlet-coverage/coverlet](https://github.com/coverlet-coverage/coverlet) | [nuget](https://www.nuget.org/packages/coverlet.collector) | **NEED** | NuGet (Dev-Only) | MVP | 4.3 P2 |
| 4 | Komurasoft Elevation Pattern | Kein Paket — Referenzarchitektur | — | **NEED** (bereits impl.) | Architekturmuster | MVP | 2.3 P0 |
| 5 | `Vanara.PInvoke.NtDll` | [dahall/Vanara](https://github.com/dahall/Vanara) | [nuget](https://www.nuget.org/packages/Vanara.PInvoke.NtDll) | **NEED** | NuGet (P/Invoke-Wrapper) | Phase 2 | 5.4 P2 |
| 6 | `Microsoft.Windows.CsWin32` | [microsoft/CsWin32](https://github.com/microsoft/CsWin32) | [nuget](https://www.nuget.org/packages/Microsoft.Windows.CsWin32) | **NICE** | NuGet (SourceGen) | Phase 2 | 5.3, 5.5 |
| 7 | `TaskScheduler` (dahall) | [dahall/TaskScheduler](https://github.com/dahall/TaskScheduler) | [nuget](https://www.nuget.org/packages/TaskScheduler) | **NEED** | NuGet (Runtime) | Phase 2 | 6.2 P2 |
| 8 | `Velopack` | [velopack/velopack](https://github.com/velopack/velopack) | [nuget](https://www.nuget.org/packages/Velopack) | **NEED** | NuGet (Runtime + CLI) | Phase 3 | 6.4 P2 |
| 9 | `Microsoft.ML` + `Microsoft.ML.TimeSeries` | [dotnet/machinelearning](https://github.com/dotnet/machinelearning) | [nuget](https://www.nuget.org/packages/Microsoft.ML) | **NICE** | NuGet (Runtime) | Phase 3 | 6.3 P3 |

---

## SKIP-Liste — Vollständig

| Paket / Repo | Grund für Ablehnung |
|---|---|
| **Windows Performance Toolkit SDK** (wpt.msi) | Systemweites Diagnose-SDK für ETW-Profiling. Kein NuGet-Paket; Installation erfordert Windows ADK. Weit über den Scope einer User-Facing Optimization-App hinaus. |
| **MSO-Scripts** (Microsoft Office Skripte) | Office-Automation. Kein Bezug zu Windows-System-Performance im Cursor-IDE-Kontext. |
| **ETW / Microsoft.Diagnostics.Tracing.TraceEvent** | Event Tracing for Windows für tiefen OS-Kernel-Tracing. `System.Diagnostics.PerformanceCounter` deckt alle Metriken ab, die WindowsPerformance benötigt (CPU, RAM, Disk). TraceEvent würde Elevated-Kernel-Sessions erfordern und die Komplexität unnötig erhöhen. |
| **MaterialDesignInXamlToolkit** | In `TODO.md` als optionale Alternative erwähnt. Tokyo-Night-Custom-Styles sind im MVP bereits vollständig implementiert (`/Themes/TokyoNight.xaml`). Nachträgliche Einführung von Material Design würde Theme-Konflikte, Paket-Overhead (~15 MB) und API-Brüche verursachen. |
| **Squirrel.Windows** | In `TODO.md` als Installer-Option genannt. Das Projekt ist seit 2021 praktisch inaktiv. **Velopack** (Verdict: NEED) ist der direkte, aktiv gewartete Nachfolger des ursprünglichen Squirrel-Teams. |

---

## Verdict-Definitionen

| Verdict | Bedeutung |
|---------|----------|
| **NEED** | Die betroffene TODO-Aufgabe ist ohne dieses Paket nicht sinnvoll implementierbar (Kern-Funktionalität fehlt oder manueller Aufwand wäre unverhältnismäßig hoch). |
| **NICE** | Das Paket vereinfacht die Implementierung erheblich, die Funktion wäre aber auch mit eigenem Code oder nativen APIs machbar. |
| **SKIP** | Explizit abgelehnt — Begründung in der SKIP-Tabelle. Darf **nicht** ohne neue Entscheidung eingeführt werden. |

---

## Detaildaten je Paket

### H.NotifyIcon.Wpf

| Feld | Wert |
|------|------|
| Maintainer | HavenDV (havendv@gmail.com, aktiv) |
| Lizenz | MIT |
| .NET-Targets | net6.0-windows, net7.0-windows, net8.0-windows, net9.0-windows |
| WPF-Support | Nativ (separate WPF-Variante neben WinUI/MAUI) |
| Letzter Commit | 2025 |
| Abhängigkeiten | Keine unerwarteten transitiven Deps |
| Namenskonflikt | Älteres `Hardcodet.NotifyIcon.Wpf` (in TODO erwähnt) — `H.NotifyIcon.Wpf` ist der modernere Fork mit aktiver Pflege |

---

### LiveChartsCore.SkiaSharpView.WPF

| Feld | Wert |
|------|------|
| Maintainer | beto-rodriguez (aktiv, >5k GitHub-Stars) |
| Lizenz | MIT |
| .NET-Targets | net6.0, net7.0, net8.0 (net9.0 via Kompatibilität) |
| WPF-Support | Eigenes WPF-Paket (`SkiaSharpView.WPF`) |
| Letzter Commit | 2025 |
| Aktueller Stand | `2.0.0-rc4` — produktionsreif, API stabil |
| Transitiv | SkiaSharp, HarfBuzzSharp (je ~5 MB nativ) |

---

### coverlet.collector

| Feld | Wert |
|------|------|
| Maintainer | coverlet-coverage (Community, Microsoft-unterstützt) |
| Lizenz | MIT |
| Verwendung | `--collect:"XPlat Code Coverage"` in `dotnet test` |
| Output-Format | Cobertura XML (kompatibel mit GitHub Actions, ReportGenerator) |
| Projekt-Scope | Nur in Test-Projekten (`PrivateAssets="all"`) |

---

### Vanara.PInvoke.NtDll

| Feld | Wert |
|------|------|
| Maintainer | dahall (David Hall, aktiv, sehr umfangreich) |
| Lizenz | MIT |
| Relevante Methode | `NtSetSystemInformation(SYSTEM_INFORMATION_CLASS.SystemMemoryListInformation, ...)` |
| Privilege | `SE_INCREASE_QUOTA_PRIVILEGE` (Elevation erforderlich) |
| Alternative | Manuelles `[DllImport("ntdll.dll")]` — höheres Fehlerrisiko, mehr Boilerplate |

---

### Microsoft.Windows.CsWin32

| Feld | Wert |
|------|------|
| Maintainer | Microsoft (AArnott, aktiv) |
| Lizenz | MIT |
| Funktionsweise | Roslyn Source Generator — liest `NativeMethods.txt`, generiert typsichere Wrapper |
| Runtime-Overhead | Null (compile-time only) |
| Alternative | Manuelle P/Invoke-Definitionen oder Vanara (Vanara hat mehr fertige High-Level-Wrapper; CsWin32 ist generischer) |

---

### TaskScheduler (dahall)

| Feld | Wert |
|------|------|
| Maintainer | dahall (gleicher Autor wie Vanara) |
| Lizenz | MIT |
| COM-Wrapper für | Windows Task Scheduler 2.0 (Vista+) |
| .NET 9 | Kompatibel (Windows-only) |
| Alternative | `Microsoft.Win32.TaskScheduler` (veraltet, selbe Basis) oder direktes COM-Interop (aufwändig) |

---

### Velopack

| Feld | Wert |
|------|------|
| Maintainer | Clowd Software (caesay, ehemals Squirrel-Maintainer) |
| Lizenz | MIT |
| .NET 9 | ✓ |
| Vorteile vs. Squirrel | Aktive Entwicklung, Delta-Updates, bessere Code-Signing-Integration, plattformübergreifend (Windows/Linux/macOS) |
| CLI-Tool | `vpk` (separates Tool-Install: `dotnet tool install -g vpk`) |

---

### Microsoft.ML + Microsoft.ML.TimeSeries

| Feld | Wert |
|------|------|
| Maintainer | Microsoft (.NET-Team) |
| Lizenz | MIT |
| Relevanter Algorithmus | SrCnn (Spectral Residual + CNN) für Zeitreihen-Anomalie-Erkennung |
| Training | Lokal, keine Cloud-API |
| Datenquelle | `performance_metrics`-Tabelle in SQLite (7-Tage-Retention) |
| Paketgröße | ~100 MB (native Deps) — erst Phase 3 relevant |
