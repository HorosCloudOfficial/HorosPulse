# Cursor Shortcuts — Vollständige Referenz

> **Diataxis: Reference** · Stand: 2026-07-06  
> Quelle: [cursor.com/docs/reference/keyboard-shortcuts](https://cursor.com/docs/reference/keyboard-shortcuts) — Windows-Ctrl-Mappings

---

## Kategorie 1: Agent & Chat öffnen

| Shortcut | Funktion | Kontext |
|---|---|---|
| `Ctrl+I` | Agent-Panel öffnen / fokussieren | Global |
| `Ctrl+L` | Chat-Panel öffnen / fokussieren | Global |
| `Ctrl+Shift+I` | Aktuelle Auswahl dem Agent hinzufügen | Editor-Selektion |
| `Ctrl+Shift+L` | Aktuelle Auswahl dem Chat hinzufügen | Editor-Selektion |
| `Ctrl+N` | Neuen Chat / neue Agent-Session starten | Im Panel |
| `Ctrl+R` | Letzte Chat-Sessions anzeigen (Recent) | Global |

---

## Kategorie 2: Inline Edit

| Shortcut | Funktion | Kontext |
|---|---|---|
| `Ctrl+K` | Inline-Edit-Leiste öffnen | Editor-Fokus |
| `Ctrl+Enter` | Inline-Änderung übernehmen (Apply) | Inline-Edit aktiv |
| `Escape` | Inline-Edit schließen / Vorschlag verwerfen | Inline-Edit aktiv |

---

## Kategorie 3: Inline-Vervollständigung (Copilot++)

| Shortcut | Funktion | Kontext |
|---|---|---|
| `Tab` | Gesamten Inline-Vorschlag übernehmen | Vorschlag sichtbar |
| `Ctrl+→` | Nächstes Wort des Vorschlags übernehmen | Vorschlag sichtbar |
| `Escape` | Vorschlag verwerfen | Vorschlag sichtbar |

---

## Kategorie 4: Apply / Review / Reject

| Shortcut | Funktion | Kontext |
|---|---|---|
| `Ctrl+Enter` | Änderungen übernehmen (Accept/Apply) | Diff sichtbar, Panel |
| `Ctrl+Backspace` | Änderungen ablehnen (Reject) | Diff sichtbar |
| `Ctrl+Shift+Backspace` | Laufende Generation abbrechen | Während Generation |

> **Hinweis zum Review-Workflow (Cursor 0.45+):** In früheren Cursor-Versionen gab es einen separaten „Review"-Schritt mit Häkchen-Symbol, bevor Änderungen final übernommen wurden. Ab Cursor 0.45 entfällt dieser Zwischenschritt. Änderungen werden direkt als Diff angezeigt; `Ctrl+Enter` nimmt an, `Ctrl+Backspace` lehnt ab. Details in [06-faq-troubleshooting.md](06-faq-troubleshooting.md).

---

## Kategorie 5: Modus & Modell

| Shortcut | Funktion | Kontext |
|---|---|---|
| `Ctrl+.` | KI-Modus-Auswahlmenü öffnen (Agent / Ask / Manual) | Global |
| `Shift+Tab` | Durch Modi rotieren (Agent → Ask → Manual) | Panel aktiv |
| `Ctrl+/` | Modell wechseln (Schleife durch verfügbare Modelle) | Im Chat/Agent-Panel |

---

## Kategorie 6: Einstellungen & Palette

| Shortcut | Funktion | Kontext |
|---|---|---|
| `Ctrl+Shift+P` | Befehls-Palette öffnen | Global |
| `Ctrl+Shift+J` | Cursor-Einstellungen öffnen | Global |
| `Ctrl+E` | KI-Panel-Layout togglen | Global |

---

## Kategorie 7: Kontext im Prompt

Diese Eingaben sind keine Tastenkürzel im klassischen Sinne, sondern Prompt-Syntax-Trigger:

| Eingabe | Öffnet / Funktion |
|---------|------------------|
| `@` | Kontext-Selektor (Dateien, Symbole, Docs, Web, Git, Codebase) |
| `@FileName` | Datei direkt referenzieren |
| `@web` | Web-Suche als Kontext |
| `@docs` | Dokumentations-Kontext |
| `/` | Befehls-Menü im Prompt (z. B. `/edit`, `/explain`) |

---

## Vollständige Shortcut-Matrix (Übersicht)

| # | Shortcut | Kategorie | Funktion |
|---|---|---|---|
| 1 | `Ctrl+I` | Agent | Agent-Panel öffnen |
| 2 | `Ctrl+L` | Chat | Chat-Panel öffnen |
| 3 | `Ctrl+Shift+I` | Agent | Auswahl → Agent |
| 4 | `Ctrl+Shift+L` | Chat | Auswahl → Chat |
| 5 | `Ctrl+N` | Panel | Neuer Chat |
| 6 | `Ctrl+R` | Panel | Recent Chats |
| 7 | `Ctrl+K` | Inline Edit | Inline-Edit öffnen |
| 8 | `Tab` | Completion | Vorschlag übernehmen |
| 9 | `Ctrl+→` | Completion | Nächstes Wort übernehmen |
| 10 | `Escape` | Allgemein | Abbrechen / Schließen |
| 11 | `Ctrl+Enter` | Apply | Änderungen übernehmen |
| 12 | `Ctrl+Backspace` | Apply | Änderungen ablehnen |
| 13 | `Ctrl+Shift+Backspace` | Apply | Generation abbrechen |
| 14 | `Ctrl+.` | Modus | Modus-Menü öffnen |
| 15 | `Shift+Tab` | Modus | Modus rotieren |
| 16 | `Ctrl+/` | Modell | Modell wechseln |
| 17 | `Ctrl+Shift+P` | Einstellungen | Befehls-Palette |
| 18 | `Ctrl+Shift+J` | Einstellungen | Cursor-Einstellungen |
| 19 | `Ctrl+E` | Layout | Panel-Layout togglen |
| 20 | `@` | Prompt | Kontext-Selektor |
| 21 | `/` | Prompt | Befehls-Menü |

---

## Nicht dokumentierte / Beta-Shortcuts

Diese Shortcuts sind in offiziellen Docs nicht garantiert stabil und können sich zwischen Cursor-Releases ändern:

| Shortcut | Gemeldete Funktion | Status |
|---|---|---|
| `Ctrl+Alt+I` | Background Agent öffnen | Beta |
| `Ctrl+Shift+E` | Codebase-Index neu aufbauen | Nicht in Docs |

Quellen: Community-Forum, Cursor Release Notes.
