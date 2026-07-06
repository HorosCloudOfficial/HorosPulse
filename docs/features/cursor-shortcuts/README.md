# Cursor IDE — Keyboard Shortcuts (Windows)

> **Referenz-Dokumentation** · Stand: 2026-07-06  
> Wichtige nicht-code-navigationsbezogene Tastenkürzel für Cursor IDE auf Windows — fokussiert auf Agent, Chat, Inline Edit, Apply/Reject, Modus- und Modellwechsel sowie Kontext-Eingabe.

---

## Auf einen Blick

Cursor IDE ergänzt VS Code um eine eigene KI-Ebene mit drei Haupt-Interaktionsmodi und einer Reihe dedizierter Tastenkürzel. Diese Dokumentation deckt **ausschließlich** die KI- und Workflow-bezogenen Shortcuts ab — keine Code-Navigations- oder Refactoring-Bindings (F12, Umbenennen, Multi-Cursor).

### Top-Shortcuts

| Tastenkürzel | Funktion |
|---|---|
| `Ctrl+I` | Agent-Panel öffnen / fokussieren |
| `Ctrl+L` | Chat-Panel öffnen / fokussieren |
| `Ctrl+K` | Inline-Edit-Leiste öffnen |
| `Ctrl+Enter` | Nachricht senden / Änderungen übernehmen |
| `Ctrl+Backspace` | Änderungen ablehnen |
| `Ctrl+Shift+J` | Cursor-Einstellungen öffnen |
| `Ctrl+/` | Modell wechseln (Schleife) |
| `Ctrl+.` | KI-Modus wechseln |

---

## Inhaltsverzeichnis

| Datei | Diataxis | Inhalt |
|-------|----------|--------|
| [01-uebersicht.md](01-uebersicht.md) | Erklärung | Was, warum, Abgrenzung der drei Modi |
| [02-benutzer-anleitung.md](02-benutzer-anleitung.md) | Anleitung | Tägliche Workflows Schritt für Schritt |
| [03-einstellungen.md](03-einstellungen.md) | Referenz | Keybinding-Anpassung, `keybindings.json`, Konflikte |
| [04-architektur.md](04-architektur.md) | Erklärung | Agent vs. Chat vs. Inline Edit — Fluss und Abgrenzung |
| [05-api-datenmodell.md](05-api-datenmodell.md) | Referenz | Vollständige Shortcut-Tabellen nach Kategorie |
| [06-faq-troubleshooting.md](06-faq-troubleshooting.md) | Anleitung | Review vs. Apply (UI-Änderung), Rebinding, Konflikte |
| [index.html](index.html) | — | Standalone HTML-Übersicht mit Sidebar + Mermaid |

---

## Modi im Überblick

Cursor kennt drei KI-Interaktionsmodi, wählbar über `Ctrl+.` oder `Shift+Tab`:

| Modus | Beschreibung | Öffnen |
|-------|-------------|--------|
| **Agent** | Autonome, mehrstufige Aufgaben; liest und schreibt Dateien selbstständig | `Ctrl+I` |
| **Ask** (Chat) | Fragen stellen, Code erklären lassen, ohne direkte Änderungen | `Ctrl+L` |
| **Inline Edit** | Gezielte Änderung einer Auswahl oder Zeile im Editor | `Ctrl+K` |

---

## Verlinkungen

- Offizielle Cursor-Docs: [cursor.com/docs/reference/keyboard-shortcuts](https://cursor.com/docs/reference/keyboard-shortcuts)
- Projekt-Dokumentation: [`docs/README.md`](../../README.md)
- Master-TODO: [`TODO.md`](../../TODO.md)
