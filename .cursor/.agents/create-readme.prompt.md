# 📝 create-readme.prompt.md

**Pfad:** `.github/prompts/create-readme.prompt.md`  
**Typ:** Prompt Template — README.md Generator  
**Status:** ✅ Sofort verwendbar — keine Anpassungen erforderlich  
**Aktivierung:** `/prompt create-readme` oder im Agent-Modus ausführen  
**Mode:** `agent` (benötigt Codebase-Zugriff)

---

## 🔍 Was ist diese Datei?

Ein **Prompt Template** das automatisch eine hochwertige `README.md` Datei für ein Projekt erstellt. Analysiert das gesamte Projekt und generiert eine professionelle, informative Dokumentation.

---

## 🎯 Was macht dieser Prompt?

1. **Projekt analysieren** — vollständiger Workspace-Review
2. **README strukturieren** — nach bewährten Open-Source-Beispielen
3. **Formatieren** — GitHub Flavored Markdown (GFM) + Admonitions
4. **Qualitätsprüfung** — concise, nicht zu viele Emojis, klar

### Aus dem Prompt stammende Regeln
- ✅ Umfassende, gut strukturierte README
- ✅ GFM Formatierung
- ✅ GitHub Admonitions (`> [!NOTE]`, `> [!WARNING]` etc.)
- ✅ Logo/Icon einbinden wenn vorhanden
- ❌ Keine overuse von Emojis
- ❌ Keine LICENSE/CONTRIBUTING/CHANGELOG Sektionen

---

## ⚡ Parameter-System

### Aktivierungswege

| Syntax | Effekt | Sofort nutzbar |
|--------|--------|----------------|
| `/prompt create-readme` | Prompt direkt ausführen | ✅ Ja |
| `@workspace /prompt create-readme` | Mit Workspace-Kontext | ✅ Ja |

---

## 📋 GitHub Admonitions (in README nutzbar)

```markdown
> [!NOTE]
> Nützliche Information für Nutzer

> [!TIP]
> Hilfreicher Tipp

> [!IMPORTANT]
> Kritische Information

> [!WARNING]
> Warnung — Vorsicht geboten

> [!CAUTION]
> Gefährliche Aktion — kann Daten löschen
```

---

## 📖 README Struktur (typisch)

```markdown
# Projekt-Name
[Logo/Icon wenn vorhanden]
[Kurze Beschreibung — 1-2 Sätze]

## Features
[Key Features als Liste]

## Getting Started
### Prerequisites
### Installation

## Usage
[Wie man es benutzt]

## Architecture
[Wie es aufgebaut ist]

## Contributing
[Kurzer Hinweis auf CONTRIBUTING.md]
```

---

## 💡 Praktische Anwendungsbeispiele

```bash
# README für Zielprojekt erstellen
/prompt create-readme

# README für ein anderes Projekt
@workspace erstelle eine professionelle README.md für das Zielprojekt Projekt

# README mit spezifischem Fokus
erstelle eine README.md die besonders die App-Features und Installation hervorhebt
```

### Beispiel für Zielprojekt README Output:
```markdown
# Zielprojekt
Ein modernes App-Client für Windows, gebaut mit .NET MAUI.

> [!NOTE]
> Dieses Projekt befindet sich in aktiver Entwicklung.

## Features
- Einfache Verbindung mit einem Klick
- Server-Auswahl mit Länderflaggen
- Dark Theme Interface
- Automatische Verbindungswiederherstellung
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Dieser Prompt ist vollständig und sofort ausführbar!

Um den Prompt anzupassen:
- **Andere Beispiel-READMEs:** URLs in der Datei ändern
- **Projekt-spezifische Sektionen:** Neue Anforderungen hinzufügen
- **Sprache:** Deutsche README Ausgabe anfordern: "Create the README in German"

---

*Erstellt von CODEX — Experten Team Auswahl README System*



