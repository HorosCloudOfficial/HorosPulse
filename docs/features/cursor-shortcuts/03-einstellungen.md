# Cursor Shortcuts — Einstellungen & Konfiguration

> **Diataxis: Reference** · Stand: 2026-07-06

---

## Keybindings anpassen

Cursor verwendet VS Codes Standard-Keybinding-System. Eigene Überschreibungen werden in `keybindings.json` eingetragen.

**Öffnen:**
- `Ctrl+Shift+P` → _„Preferences: Open Keyboard Shortcuts (JSON)"_
- Oder: `Ctrl+Shift+P` → _„Preferences: Open Keyboard Shortcuts"_ (grafische UI)

**Pfad:**  
`%APPDATA%\Cursor\User\keybindings.json`

---

## Cursor-eigene Befehls-IDs

Diese Command-IDs werden für Keybinding-Overrides benötigt:

| Command-ID | Standard-Shortcut | Funktion |
|---|---|---|
| `aichat.newchataction` | `Ctrl+L` | Chat-Panel öffnen |
| `composer.startComposerPrompt` | `Ctrl+I` | Agent-Panel öffnen |
| `aichat.insertselectionintochat` | `Ctrl+Shift+L` | Auswahl → Chat |
| `composer.addFileToComposer` | `Ctrl+Shift+I` | Auswahl → Agent |
| `composer.newAgentChat` | `Ctrl+N` | Neuer Chat (im Panel) |
| `editor.action.inlineSuggest.trigger` | automatisch | Inline-Vorschlag auslösen |
| `inlineChat.start` | `Ctrl+K` | Inline-Edit-Leiste |
| `cursor.toggleCursorSettings` | `Ctrl+Shift+J` | Cursor-Einstellungen |

> **Hinweis:** Cursor-spezifische Command-IDs können sich zwischen Releases ändern. Die aktuelle Liste ist über `Ctrl+Shift+P` → _„Preferences: Open Keyboard Shortcuts"_ → Suchfeld einsehbar.

---

## Beispiel: `Ctrl+L` auf `Ctrl+Alt+C` umlegen

```json
[
  {
    "key": "ctrl+alt+c",
    "command": "aichat.newchataction"
  },
  {
    "key": "ctrl+l",
    "command": "-aichat.newchataction"
  }
]
```

Das `-` vor dem Command-Namen entfernt die bestehende Binding.

---

## Beispiel: `Ctrl+/` Konflikt lösen (Zeile auskommentieren vs. Modell wechseln)

`Ctrl+/` ist in VS Code standardmäßig „Zeile auskommentieren" (`editor.action.commentLine`). Cursor übernimmt diesen Slot in KI-Kontexten für den Modellwechsel. Um beide Funktionen zu trennen:

```json
[
  {
    "key": "ctrl+/",
    "command": "editor.action.commentLine",
    "when": "editorTextFocus && !inCursorChat"
  }
]
```

Der `when`-Kontext `inCursorChat` stellt sicher, dass `Ctrl+/` im Editor-Fokus weiterhin auskommentiert, während es im Chat/Agent-Panel das Modell wechselt.

---

## Cursor-Einstellungen (`Ctrl+Shift+J`)

Die Cursor-eigene Einstellungs-UI (nicht VS Code Settings) enthält:

| Bereich | Einstellung |
|---------|-------------|
| **Models** | Standard-Modell festlegen, API-Keys (für BYOK) |
| **Features** | Copilot++-Modus, Auto-Import-Vorschläge, Shadow Workspace |
| **Privacy** | Privacy Mode (kein Code-Sharing mit Cursor-Servern) |
| **Rules** | `.cursor/rules/` — projektweite KI-Anweisungen |
| **Beta** | Experimentelle Features aktivieren |

---

## `settings.json` — Relevante Cursor-Einträge

```json
{
  "cursor.general.enableShadowWorkspace": true,
  "cursor.cpp.disabledLanguages": [],
  "cursor.chat.smoothStreaming": true,
  "cursor.aipreview.enabled": true
}
```

**Pfad:**  
`%APPDATA%\Cursor\User\settings.json`

---

## Keybinding-Konflikte: Bekannte Überschneidungen

| Shortcut | Cursor-Funktion | VS-Code-Standard | Konflikt? |
|---|---|---|---|
| `Ctrl+K` | Inline Edit öffnen | Prefix-Sequenz (z. B. `Ctrl+K Ctrl+S`) | Teilweise — `Ctrl+K` als Prefix-Key verliert Funktion |
| `Ctrl+/` | Modell wechseln (in Panel) | Zeile auskommentieren | Nur in Panel-Kontext |
| `Ctrl+L` | Chat öffnen | — (kein VS-Code-Default) | Keiner |
| `Ctrl+I` | Agent öffnen | — | Keiner |
| `Ctrl+Shift+J` | Cursor-Einstellungen | — | Keiner |

Vollständige Konfliktliste und Lösungen: [06-faq-troubleshooting.md](06-faq-troubleshooting.md).

---

## `.cursor/rules/` — Projektweite KI-Anweisungen

Nicht direkt ein Shortcut, aber Einstellungs-relevant: Cursor liest `.cursor/rules/*.mdc`-Dateien als persistente Anweisungen für den Agent. Diese steuern das Verhalten des Agents projektübergreifend.

**Öffnen:** `Ctrl+Shift+P` → _„Cursor: Open Rules"_

Für dieses Projekt relevant: `D:\WindowsPerformance\.cursor\rules\`
