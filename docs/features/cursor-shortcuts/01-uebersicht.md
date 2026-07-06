# Cursor Shortcuts — Übersicht

> **Diataxis: Explanation** · Stand: 2026-07-06

---

## Was sind Cursor-eigene Shortcuts?

Cursor IDE baut auf VS Code auf und fügt eine eigene KI-Interaktionsebene hinzu. Diese Ebene bringt dedizierte Tastenkürzel mit, die **nicht** Teil des Standard-VS-Code-Keybinding-Sets sind. Die Shortcuts steuern:

- das Öffnen und Wechseln zwischen den drei KI-Panels (Agent, Chat, Inline Edit)
- das Senden von Prompts und Übernehmen oder Ablehnen von Vorschlägen
- den Wechsel zwischen KI-Modi und -Modellen
- die Eingabe von Kontext-Referenzen (`@`, `/`) im Prompt

Diese Dokumentation klammert Code-Navigations-Shortcuts (F12, Gehe zu Definition, Umbenennen, Multi-Cursor) bewusst aus. Diese sind VS-Code-Standard und im VS Code Keyboard Reference dokumentiert.

---

## Warum eigene Shortcuts lernen?

Cursor-Shortcuts sind darauf ausgelegt, den KI-gestützten Entwicklungsfluss zu beschleunigen:

1. **Kontextwechsel ohne Maus** — zwischen Agent, Chat und Editor wechseln, ohne den Tastaturfluss zu unterbrechen.
2. **Schnelles Apply/Reject** — generierte Änderungen per Tastatur übernehmen oder verwerfen, ohne zur Maus zu greifen.
3. **Modell- und Modusauswahl** — Modell und Interaktionsmodus situationsbedingt wechseln, ohne ein Menü aufzurufen.
4. **Kontext präzise steuern** — mit `@` gezielt Dateien oder Symbole referenzieren, mit `/` Prompt-Befehle auslösen.

---

## Die drei Haupt-Modi

### Agent (`Ctrl+I`)

Der Agent-Modus ist für **autonome, mehrstufige Aufgaben** gedacht. Der Agent liest Dateien, erstellt neue, führt Terminal-Befehle aus und schlägt Änderungen vor, die der Nutzer anschließend überprüft und übernimmt oder ablehnt. Agent-Sessions sind persistent und können mit `Ctrl+R` wieder aufgerufen werden.

**Einsatzbereich:** Neue Features implementieren, Refactoring über mehrere Dateien, Fehler debuggen, Tests schreiben.

### Chat / Ask (`Ctrl+L`)

Der Chat-Modus (Ask) erlaubt **Fragen ohne direkte Code-Änderungen**. Er ist ideal für Erklärungen, Architekturüberlegungen und Code-Reviews. Der Chat kennt den Kontext des geöffneten Projekts und kann mit `@` gezielt auf Dateien, Symbole oder Dokumentation verwiesen werden.

**Einsatzbereich:** Code erklären lassen, Alternativen diskutieren, Dokumentation suchen.

### Inline Edit (`Ctrl+K`)

Inline Edit öffnet eine Eingabeleiste direkt im Editor-Kontext. Eine Auswahl (oder der gesamte Cursor-Kontext) wird als Basis übergeben. Die Änderung erscheint sofort als Diff im Editor und kann mit `Ctrl+Enter` oder `Ctrl+Backspace` angenommen oder abgelehnt werden.

**Einsatzbereich:** Gezielte Änderung einzelner Funktionen oder Blöcke ohne vollständige Agent-Session.

---

## Abgrenzung zu VS-Code-Shortcuts

| Cursor-Shortcut | Funktion | VS-Code-Äquivalent |
|---|---|---|
| `Ctrl+I` | Agent-Panel | — (Cursor-spezifisch) |
| `Ctrl+L` | Chat-Panel | — (Cursor-spezifisch) |
| `Ctrl+K` | Inline Edit | `Ctrl+K` (Prefix-Sequenzen in VS Code) |
| `Ctrl+Shift+J` | Cursor-Einstellungen | `Ctrl+,` (VS Code Settings) |
| `Ctrl+/` | Modell wechseln | `Ctrl+/` (Toggle Line Comment in VS Code) |

> **Hinweis zu `Ctrl+/`:** Cursor überschreibt in KI-Kontexten die VS-Code-Binding für `Ctrl+/`. Im Editor (ohne aktives KI-Panel) funktioniert `Ctrl+/` weiterhin als „Zeile auskommentieren". Konflikte und Lösungen sind in [06-faq-troubleshooting.md](06-faq-troubleshooting.md) beschrieben.

---

## Quellen

- [Cursor Keyboard Shortcuts Reference](https://cursor.com/docs/reference/keyboard-shortcuts)
- [Cursor Agent Documentation](https://cursor.com/docs/agent)
- [Cursor Chat Documentation](https://cursor.com/docs/chat)
