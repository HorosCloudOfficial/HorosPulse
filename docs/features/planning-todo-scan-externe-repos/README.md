# TODO-Scan → Externe Repos

> **Planungsartefakt** · Stand: 2026-07-06  
> Analysiert welche externen NuGet-Pakete und GitHub-Repos für WindowsPerformance sinnvoll sind — abgeleitet aus dem Master-TODO (`TODO.md`), phasenweise priorisiert.

---

## Auf einen Blick

WindowsPerformance ist eine WPF .NET 9 App für Windows-Systemoptimierung im Cursor-IDE-Workflow. Die TODO-Analyse ergab **acht empfehlenswerte externe Abhängigkeiten** in drei Phasen sowie **fünf bewusste Ablehnungen (SKIP)**.

### Top-5 Shortlist

| Priorität | Paket | Begründung | Phase |
|-----------|-------|-----------|-------|
| 1 | `H.NotifyIcon.Wpf` | Tray-Icon fehlt im MVP (TODO 1.4 P1) | MVP |
| 2 | `LiveChartsCore.SkiaSharpView.WPF` | CPU/RAM-Sparklines, Trend-Ansicht | MVP/Phase 2 |
| 3 | `coverlet.collector` | Test-Coverage-Reports für CI (TODO 4.3) | MVP |
| 4 | `Vanara.PInvoke.NtDll` | Standby-Liste leeren (Memory Optimizer, TODO 5.4) | Phase 2 |
| 5 | `Velopack` | Auto-Update-Mechanismus statt Squirrel (TODO 6.4) | Phase 3 |

---

## Inhaltsverzeichnis

| Datei | Diataxis | Inhalt |
|-------|----------|--------|
| [01-uebersicht.md](01-uebersicht.md) | Erklärung | Warum dieser Scan, Methodik, Abgrenzung |
| [02-benutzer-anleitung.md](02-benutzer-anleitung.md) | Anleitung | Integration Schritt für Schritt (für Entwickler) |
| [03-einstellungen.md](03-einstellungen.md) | Referenz | NuGet-Versionen, `Directory.Packages.props`-Einträge |
| [04-architektur.md](04-architektur.md) | Erklärung | Phasen-Mapping, Abhängigkeitsgraph, Mermaid-Diagramm |
| [05-api-datenmodell.md](05-api-datenmodell.md) | Referenz | Vollständige Repo-Tabelle: URL, Verdict, Typ |
| [06-faq-troubleshooting.md](06-faq-troubleshooting.md) | Anleitung | Häufige Fragen, SKIP-Begründungen, NuGet-Konflikte |
| [index.html](index.html) | — | Standalone HTML-Übersicht mit Sidebar + Mermaid |

---

## SKIP-Liste (Kurzfassung)

Diese Repos wurden geprüft und **bewusst abgelehnt**:

- **Performance Toolkit SDK** — SDK-Overhead, nicht für eingebettete WPF-App geeignet
- **MSO-Scripts** — Office-Automation außerhalb des definierten Scopes
- **ETW / TraceEvent** — zu komplex für App-internen Einsatz; `PerformanceCounter` reicht
- **MaterialDesignInXamlToolkit** — TODO nennt es als Option, Tokyo-Night-Custom-Styles sind bereits etabliert
- **Squirrel.Windows** — veraltet und nicht mehr gewartet; Velopack ist der moderne Nachfolger

Vollständige Begründungen in [06-faq-troubleshooting.md](06-faq-troubleshooting.md).

---

## Verlinkungen

- Master-TODO: [`TODO.md`](../../TODO.md)
- Hauptdokumentation: [`README.md`](../../README.md)
- Branch-Workflow: [`docs/BRANCHING.md`](../BRANCHING.md)
