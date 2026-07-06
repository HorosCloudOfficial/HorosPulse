# Übersicht: TODO-Scan → Externe Repos

> **Typ:** Erklärung (Diataxis)  
> **Zielgruppe:** Entwickler und Maintainer von HorosPulse

---

## Was ist dieser Scan?

Der TODO-Scan ist eine systematische Analyse des Master-TODOs (`TODO.md`), bei der jede offene Aufgabe darauf geprüft wurde, ob eine externe Abhängigkeit die Implementierung vereinfacht, absichert oder beschleunigt. Das Ergebnis ist eine phasengerechte Liste von NuGet-Paketen und GitHub-Repos mit einem klaren **NEED / NICE / SKIP**-Urteil.

### Methodik

1. Alle `- [ ]`-Einträge aus `TODO.md` gesammelt (MVP + Phase 2 + Phase 3).
2. Je Eintrag geprüft: Gibt es ein etabliertes Paket, das genau dieses Problem löst?
3. Pakete bewertet nach: Aktivität (letzter Commit), .NET 9 / WPF-Kompatibilität, Lizenz (MIT/Apache bevorzugt), Download-Zahlen, offizieller Support.
4. Verdict vergeben: **NEED** (ohne diese Abhängigkeit ist die Funktion kaum vertretbar zu bauen), **NICE** (erleichtert Implementierung, eigener Code wäre aber möglich), **SKIP** (explizit abgelehnt mit Begründung).

---

## Scope

Dieser Scan deckt **externe NuGet-Pakete und GitHub-Repos** ab. Er deckt **nicht** ab:

- Interne Bibliotheken oder lokale Projekte innerhalb der Solution
- PowerShell-Module (getrennte Analyse)
- CI/CD-Actions (gehören in `.github/workflows/`)
- Betriebssystem-APIs (P/Invoke-Wrapper sind Teil des Scans, aber die OS-APIs selbst nicht)

---

## Phasen-Übersicht

### MVP-Lücken (Sprint 1–3)

Fünf Abhängigkeiten fehlen im aktuellen Setup und blockieren oder erschweren konkrete MVP-Aufgaben:

| TODO-Aufgabe | Fehlende Abhängigkeit | Aufgaben-ID |
|---|---|---|
| Tray-Icon (App in Tray minimieren) | `H.NotifyIcon.Wpf` | TODO 1.4 P1 |
| CPU/RAM-Sparklines auf Dashboard | `LiveChartsCore.SkiaSharpView.WPF` | TODO 3.6 P1 |
| Test-Coverage-Reports (CI) | `coverlet.collector` | TODO 4.3 P2 |
| Startup-Programme verwalten | Registry-Zugriff via `Microsoft.Win32.Registry` (bereits im SDK) oder Startup-Explorer-Muster | TODO 5.2 |
| Elevation-Pattern (robuste IPC) | Komurasoft-Elevation-Pattern als Referenzarchitektur | TODO 2.3 |

### Phase 2 (W7–14)

Drei Abhängigkeiten werden erst ab Phase 2 benötigt, sollten aber jetzt schon evaluiert werden, damit keine Architekturentscheidung das später blockiert:

| TODO-Aufgabe | Abhängigkeit | Aufgaben-ID |
|---|---|---|
| Standby-Liste leeren (Memory Optimizer) | `Vanara.PInvoke.NtDll` | TODO 5.4 |
| Windows-API-Bindings (Visual Effects, Network) | `Microsoft.Windows.CsWin32` | TODO 5.3, 5.5 |
| Geplante Tasks lesen/steuern | `TaskScheduler` (dahall) | TODO 6.2 |

> `PurgeStandbyList` ist kein eigenes Paket, sondern der Methodenname aus `Vanara.PInvoke.NtDll` — `NtSetSystemInformation(SYSTEM_MEMORY_LIST_COMMAND.MemoryPurgeStandbyList, ...)`.

### Phase 3 (W15+)

Zwei strategische Abhängigkeiten für den langen Horizont:

| TODO-Aufgabe | Abhängigkeit | Aufgaben-ID |
|---|---|---|
| Auto-Update (statt Squirrel) | `Velopack` | TODO 6.4 |
| ML-Anomalie-Erkennung (lokal) | `Microsoft.ML` | TODO 6.3 |

---

## Abgrenzung zu verwandten Entscheidungen

- **MaterialDesignInXamlToolkit** ist in `TODO.md` als Option erwähnt (`Tokyo Night Farb-Palette als ResourceDictionary definieren (…) MaterialDesignInXamlToolkit oder eigene Styles`). Da eigene Styles im MVP bereits etabliert sind, wird Material Design nicht mehr eingeführt. Weitere Begründung in [06-faq-troubleshooting.md](06-faq-troubleshooting.md).
- **Squirrel.Windows** ist in `TODO.md` als Option für den Installer genannt. Es wurde durch **Velopack** ersetzt (Squirrel ist der Vorgänger, Velopack der aktiv gewartete Nachfolger desselben Teams).
- **ETW / TraceEvent** (Microsoft.Diagnostics.Tracing.TraceEvent) wäre für tiefen OS-Tracing geeignet, überschreitet aber den definierten Scope einer User-Facing Optimization-App erheblich.
