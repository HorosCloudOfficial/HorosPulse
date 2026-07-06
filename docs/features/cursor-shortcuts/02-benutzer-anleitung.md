# Cursor Shortcuts — Benutzer-Anleitung

> **Diataxis: How-to** · Stand: 2026-07-06

Praktische Workflows für den täglichen Einsatz — Schritt für Schritt, ohne Maus.

---

## Workflow 1: Feature mit dem Agent implementieren

**Ziel:** Eine neue Funktion in mehreren Dateien implementieren lassen.

1. `Ctrl+I` — Agent-Panel öffnen.
2. Prompt eingeben: z. B. _„Füge eine Retry-Logik in `src/Services/ApiService.cs` ein, max. 3 Versuche mit exponentialem Backoff."_
3. Mit `@` gezielt Dateien referenzieren: `@ApiService.cs @IApiService.cs`.
4. `Ctrl+Enter` — Prompt absenden.
5. Agent zeigt generierte Änderungen als Diff an.
6. Änderungen prüfen → `Ctrl+Enter` zum Übernehmen oder `Ctrl+Backspace` zum Ablehnen.
7. Wenn die Generation zu lange dauert: `Ctrl+Shift+Backspace` — Abbrechen.

> **Neuen Chat starten:** `Ctrl+N` beginnt eine frische Session. Vorherige Sessions bleiben gespeichert und sind über `Ctrl+R` (Recent Chats) wieder zugänglich.

---

## Workflow 2: Code erklären lassen (Chat/Ask)

**Ziel:** Eine komplexe Funktion verstehen, ohne den Code zu verändern.

1. Funktion im Editor markieren.
2. `Ctrl+Shift+L` — Auswahl zum Chat hinzufügen.
3. `Ctrl+L` — Chat-Panel öffnen (falls nicht bereits geöffnet).
4. Frage eingeben: _„Was macht diese Methode? Welche Edge Cases werden nicht behandelt?"_
5. `Ctrl+Enter` — Frage absenden.

Der Chat antwortet ohne Code-Änderungen. Für eine direkte Änderung stattdessen Agent (`Ctrl+I`) verwenden.

---

## Workflow 3: Gezielte Inline-Änderung

**Ziel:** Eine einzelne Funktion umschreiben, ohne einen vollständigen Agent-Context zu öffnen.

1. Zu ändernden Code-Block markieren.
2. `Ctrl+K` — Inline-Edit-Leiste öffnen.
3. Anweisung eingeben: _„Extrahiere die Validierung in eine eigene private Methode."_
4. `Ctrl+Enter` — Änderung anwenden.
5. Diff erscheint direkt im Editor.
6. `Ctrl+Enter` zum Übernehmen oder `Escape` zum Verwerfen.

---

## Workflow 4: Modell situationsbedingt wechseln

**Ziel:** Für einfache Fragen ein schnelleres Modell, für komplexe Aufgaben ein leistungsfähigeres verwenden.

1. Im Agent- oder Chat-Panel: `Ctrl+/` — Modell wechseln (rotiert durch verfügbare Modelle).
2. Alternativ: `Ctrl+.` — Modus-Menü öffnen, Modell direkt auswählen.

> Das gewählte Modell gilt bis zur nächsten manuellen Änderung. Es wird pro Chat-Session gespeichert.

---

## Workflow 5: Modus wechseln (Agent / Ask / Manual)

**Ziel:** Zwischen Agent-Modus (schreibt eigenständig) und Ask-Modus (nur Antworten) wechseln.

1. `Ctrl+.` — Modus-Auswahl-Menü öffnen.
2. Gewünschten Modus wählen.

Alternativ: `Shift+Tab` im geöffneten Panel — rotiert durch Agent → Ask → Manual.

| Modus | Verhalten |
|-------|-----------|
| **Agent** | Liest/schreibt Dateien, führt Terminal-Befehle aus |
| **Ask** | Antwortet nur mit Text, ändert keine Dateien |
| **Manual** | Inlines und Completions deaktiviert |

---

## Workflow 6: Kontext präzise steuern mit `@` und `/`

**Ziel:** Im Prompt gezielt Dateien, Symbole, Dokumentation oder Web-Inhalte referenzieren.

Im Prompt-Eingabefeld:

| Eingabe | Öffnet |
|---------|--------|
| `@` | Kontext-Selektor (Dateien, Symbole, Docs, Web, Git, Codebase) |
| `@DateiName` | Schnellsuche nach einer Datei |
| `@symbol:MethodenName` | Symbol-Referenz |
| `@web` | Web-Suche als Kontext einbeziehen |
| `/` | Befehls-Menü (z. B. `/edit`, `/explain`, `/fix`) |

**Beispiel:**
```
Refactoriere @MemoryOptimizerService.cs so, dass @IMemoryOptimizer.cs als Interface vollständig implementiert wird.
```

---

## Workflow 7: Cursor-Einstellungen anpassen

1. `Ctrl+Shift+J` — Cursor-Einstellungen öffnen (nicht `Ctrl+,`, das öffnet VS-Code-Settings).
2. Hier: Modell-Auswahl, Privacy-Modus, Keybinding-Overrides, Features.

Alternativ: `Ctrl+Shift+P` → _„Cursor: Open Settings"_ eingeben.

---

## Workflow 8: Panel-Layout wechseln

`Ctrl+E` — togglet zwischen Editor-fokussiertem und KI-Panel-fokussiertem Layout. Nützlich auf kleinen Bildschirmen oder beim Wechsel zwischen Coding und Review.

---

## Tastenkürzel-Spickzettel (Daily Use)

| Aktion | Shortcut |
|--------|----------|
| Agent öffnen | `Ctrl+I` |
| Chat öffnen | `Ctrl+L` |
| Inline Edit | `Ctrl+K` |
| Auswahl → Chat | `Ctrl+Shift+L` |
| Auswahl → Agent | `Ctrl+Shift+I` |
| Neuer Chat | `Ctrl+N` |
| Letzte Chats | `Ctrl+R` |
| Senden / Übernehmen | `Ctrl+Enter` |
| Ablehnen | `Ctrl+Backspace` |
| Generation abbrechen | `Ctrl+Shift+Backspace` |
| Modell wechseln | `Ctrl+/` |
| Modus wechseln | `Ctrl+.` oder `Shift+Tab` |
| Cursor-Einstellungen | `Ctrl+Shift+J` |
| Befehls-Palette | `Ctrl+Shift+P` |
