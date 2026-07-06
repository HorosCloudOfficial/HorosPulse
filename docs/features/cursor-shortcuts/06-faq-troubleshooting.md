# Cursor Shortcuts — FAQ & Troubleshooting

> **Diataxis: How-to** · Stand: 2026-07-06

---

## F: Das Häkchen / „Review"-Symbol fehlt — was hat sich geändert?

**Symptom:** In älteren Cursor-Versionen (vor 0.45) gab es nach einer Agent-Generation einen expliziten „Review"-Schritt mit einem Häkchen-Icon (✓), bevor Änderungen final übernommen wurden. Dieser Schritt ist verschwunden.

**Ursache:** Cursor 0.45 hat den Review-Workflow vereinfacht. Der explizite Review-Checkpoint wurde entfernt. Änderungen erscheinen jetzt direkt als Diff im Editor und können sofort angenommen oder abgelehnt werden.

**Neuer Workflow (Cursor 0.45+):**

| Schritt | Shortcut | Verhalten |
|---------|----------|-----------|
| 1. Generation läuft | — | Fortschritt im Panel sichtbar |
| 2. Diff erscheint im Editor | — | Rote/grüne Markierungen direkt im Code |
| 3. Änderungen übernehmen | `Ctrl+Enter` | Alle vorgeschlagenen Änderungen anwenden (Accept) |
| 4. Änderungen ablehnen | `Ctrl+Backspace` | Alle Änderungen verwerfen (Reject) |
| 5. Generation stoppen | `Ctrl+Shift+Backspace` | Laufende KI-Anfrage abbrechen |

**Was ist weggefallen:**
- Separater „Review"-Button in der Toolbar
- Häkchen-Icon zum Bestätigen
- Zweistufiger Accept-Flow (Review → Apply)

**Was bleibt:** Die Diff-Anzeige selbst ist geblieben. Einzelne Hunks können weiterhin per Mausklick auf das `+`/`-`-Icon im Gutter akzeptiert oder abgelehnt werden. Der Shortcut-basierte Accept/Reject (`Ctrl+Enter` / `Ctrl+Backspace`) wirkt auf **alle** vorgeschlagenen Änderungen gleichzeitig.

---

## F: `Ctrl+I` öffnet nicht das Agent-Panel, sondern etwas anderes

**Ursache A:** Eine Extension überschreibt `Ctrl+I`. Häufige Täter: GitLens, GitHub Copilot Chat, oder andere KI-Assistenten.

**Lösung:**
1. `Ctrl+Shift+P` → _„Preferences: Open Keyboard Shortcuts"_
2. In der Suchleiste `Ctrl+I` eingeben.
3. Konflikte identifizieren und die fremde Binding entfernen (Rechtsklick → „Remove Keybinding").

**Ursache B:** Cursor ist nicht als Standard-Editor für das aktuelle Fenster aktiv.

**Lösung:** Sicherstellen, dass das Fenster in Cursor (nicht VS Code) geöffnet ist.

---

## F: `Ctrl+K` bricht Prefix-Sequenzen wie `Ctrl+K Ctrl+S` (Alle Shortcuts anzeigen)

**Ursache:** Cursor bindet `Ctrl+K` als eigenständige Aktion (Inline Edit öffnen). In VS Code ist `Ctrl+K` ein Prefix-Key für Sequenzen.

**Konflikt:** `Ctrl+K` → sofort Inline Edit öffnen; VS Code kann die Sequenz nicht mehr vollenden.

**Lösung A:** VS-Code-Sequenzen auf andere Keys legen:
```json
[
  {
    "key": "ctrl+alt+k ctrl+s",
    "command": "workbench.action.openGlobalKeybindings"
  }
]
```

**Lösung B:** Inline Edit auf anderen Key legen und `Ctrl+K` als Prefix freigeben:
```json
[
  {
    "key": "ctrl+alt+k",
    "command": "inlineChat.start",
    "when": "editorTextFocus"
  },
  {
    "key": "ctrl+k",
    "command": "-inlineChat.start"
  }
]
```

---

## F: `Ctrl+/` kommentiert keine Zeilen mehr aus

**Ursache:** `Ctrl+/` ist für den Modellwechsel im Chat/Agent-Panel belegt. Im Editor-Fokus sollte es weiterhin als „Zeile auskommentieren" funktionieren — wenn nicht, liegt ein `when`-Kontext-Problem vor.

**Diagnose:**
1. `Ctrl+Shift+P` → _„Developer: Toggle Keyboard Shortcuts Troubleshooter"_
2. `Ctrl+/` drücken → prüfen, welcher Command ausgeführt wird und warum.

**Lösung:**
```json
[
  {
    "key": "ctrl+/",
    "command": "editor.action.commentLine",
    "when": "editorTextFocus && !inCursorChat"
  }
]
```

---

## F: `Ctrl+L` öffnet den Browser-Adressleisten-Fokus (Edge/Chrome-Konflikt)

Dieser Konflikt tritt auf, wenn ein Browser-Fenster im Vordergrund ist. Cursor-Shortcuts gelten nur innerhalb des Cursor-IDE-Fensters. Kein Handlungsbedarf — die Bindung ist OS-kontextbezogen.

---

## F: Wie wechsle ich das Modell dauerhaft, ohne `Ctrl+/` jedes Mal drücken zu müssen?

**Lösung:**
1. `Ctrl+Shift+J` → Cursor-Einstellungen.
2. Unter **Models** → Standard-Modell auswählen.

Das gewählte Modell bleibt als Default, bis es manuell geändert wird. `Ctrl+/` wechselt nur für die aktuelle Session.

---

## F: `Ctrl+N` öffnet eine neue Datei statt einem neuen Chat

**Ursache:** `Ctrl+N` öffnet in VS Code eine neue unbenannte Datei. Cursor überschreibt diesen Shortcut **nur im KI-Panel-Kontext** für „Neuer Chat". Wenn der Fokus im Editor liegt, öffnet `Ctrl+N` weiterhin eine neue Datei.

**Verhalten:**
- Fokus im **Editor**: `Ctrl+N` → neue Datei
- Fokus im **Chat/Agent-Panel**: `Ctrl+N` → neuer Chat

**Lösung (wenn neuer Chat global gewünscht):**
```json
[
  {
    "key": "ctrl+shift+n",
    "command": "composer.newAgentChat"
  }
]
```

---

## F: `Ctrl+R` zeigt Recent Chats nicht, sondern öffnet zuletzt verwendete Dateien

Gleiche Kontextlogik wie `Ctrl+N`: `Ctrl+R` öffnet Recent Chats nur wenn der Fokus im Chat- oder Agent-Panel liegt.

---

## F: Inline-Vorschläge erscheinen nicht automatisch

**Ursache A:** Copilot++-Modus ist deaktiviert.

**Lösung:** `Ctrl+Shift+J` → Features → „Copilot++" aktivieren.

**Ursache B:** Privacy Mode ist aktiviert — sendet keinen Code an externe Server.

**Lösung:** Nur deaktivieren, wenn der Anwendungsfall es erlaubt. Bei sensiblen Projekten (wie `WindowsPerformance`) ist Privacy Mode empfohlen.

---

## F: `Ctrl+Shift+J` vs. `Ctrl+,` — was ist der Unterschied?

| Shortcut | Öffnet | Inhalt |
|----------|--------|--------|
| `Ctrl+,` | VS Code Settings | Editor-Einstellungen, Extensions, UI-Theme, Font |
| `Ctrl+Shift+J` | Cursor Settings | KI-Modelle, Privacy, Rules, Beta-Features |

Beide sind separate UIs. Cursor-spezifische Einstellungen sind **ausschließlich** in `Ctrl+Shift+J` zu finden.

---

## F: Wo melde ich fehlende oder falsche Shortcuts?

- Community: [forum.cursor.com](https://forum.cursor.com)
- GitHub Issues: [github.com/getcursor/cursor](https://github.com/getcursor/cursor)
- Offizielle Docs: [cursor.com/docs/reference/keyboard-shortcuts](https://cursor.com/docs/reference/keyboard-shortcuts)
