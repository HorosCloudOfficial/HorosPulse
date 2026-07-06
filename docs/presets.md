# Presets

## Built-in Presets

### Cursor Dev Mode (`cursor-dev-mode`)

Orchestrierte Dev-Optimierung mit Full-Snapshot vor Anwendung:

1. Snapshot (`before_cursor_dev_mode`)
2. High-Performance Energieplan
3. Cursor/Helper Prozessprioritäten
4. `node.exe` auf Normal
5. Search-Indexer-Ausschlüsse (node_modules, .git, dist, build)
6. Defender-Ausschlüsse (nur wenn Opt-in in Einstellungen)
7. Audit-Log-Eintrag

### Balanced (`balanced`)

- Cursor-Einstellungen aus Backup wiederherstellen
- Ausgewogener Energieplan

### Gaming (`gaming`)

- High-Performance Energieplan

## Benutzerdefinierte Presets

In **Presets** → Schritte auswählen → Name/Beschreibung → Speichern.

Schritte-IDs (`PresetStepIds`):

| ID | Beschreibung |
|----|--------------|
| `snapshot` | Full-System-Snapshot |
| `power-plan` | Energieplan |
| `cursor-priorities` | Prozessprioritäten |
| `indexer-exclusions` | Indexer-Ausschlüsse |
| `defender-exclusions` | Defender (Opt-in) |

## Import / Export

- **Export JSON:** gewähltes Preset als `ProfileDefinition`-JSON
- **Import JSON:** Datei wählen → neues user Preset mit neuer ID

Dateiformat (`ProfileDefinition`):

```json
{
  "id": "abc123",
  "name": "Mein Preset",
  "description": "Optional",
  "isBuiltIn": false,
  "steps": ["snapshot", "power-plan", "cursor-priorities"]
}
```

Gespeichert unter `%LOCALAPPDATA%\HorosPulse\profiles\`.

## Task Scheduler Preset (Phase 3)

**Dev-Zeit-Schutz** deaktiviert temporär Windows-Update-Tasks — Rollback stellt den vorherigen Zustand wieder her.

## Rollback

Jedes Preset-Apply erstellt zuerst einen Snapshot. **Rollback letzter Snapshot** in Presets oder Dashboard nutzt `RollbackEngine`.
