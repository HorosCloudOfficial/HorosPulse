# 📋 CODEX — copilot-instructions.md

**Pfad:** `.github/copilot-instructions.md`  
**Typ:** Haupt-Systemkonfiguration (Copilot Instructions)  
**Status:** ✅ Sofort verwendbar — keine Anpassungen erforderlich  
**Zweck:** Definiert das Verhalten von GitHub Copilot für das gesamte Workspace

---

## 🔍 Was ist diese Datei?

Die `copilot-instructions.md` ist die **zentrale Steuerungsdatei** für CODEX — den personalisierten KI-Assistenten. Sie definiert:
- Die **Persönlichkeit** und den **Kommunikationsstil** von Copilot
- Ein vollständiges **Parameter-Kommandosystem** (/short, /beast, /boost etc.)
- **Spezielle Modi** für verschiedene Arbeitsszenarien
- **Sicherheits- und Fehlerbehandlungsregeln**

Diese Datei wird von GitHub Copilot **automatisch geladen** und ist sofort wirksam.

---

## ⚡ Parameter-System — Vollständige Übersicht

### 🚀 Grundlegende Parameter

| Parameter | Funktion | Sofort nutzbar |
|-----------|----------|----------------|
| `/short` | Ultra-kurze Antworten (2-3 Sätze) | ✅ Ja |
| `/clean` | Workspace bereinigen (außer .github) | ✅ Ja |
| `/clean -full` | ALLES löschen inkl. .github | ⚠️ Vorsicht! |
| `/clean -save` | Backup vor Bereinigung erstellen | ✅ Ja |
| `/jailbreak` | Shadow Architect Modus aktivieren | ✅ Ja |
| `/fix` | Debug Chat Mode aktivieren | ✅ Ja |

### 💬 Talk-Modi (Zwei-Stimmen-System)

| Parameter | Aktiviert | Benötigt Datei |
|-----------|-----------|----------------|
| `/critical -talk` | Kritischer Denker | `.github/chatmodes/critical-thinking.chatmode.md` |
| `/ultimate -talk` | 15-köpfiges Expertenteam | `.github/agents/ULTIMATE-PROJECT-ANALYSIS-TEAM.agent.md` |
| `/debug -talk` | Debug-Spezialist | `.github/chatmodes/debug.chatmode.md` |
| `/claude -talk` | Claude Code System | `.github/chatmodes/claude-code-system.chatmode.md` |
| `/clean -talk` | Clean Code Experte | `.github/chatmodes/clean-code.chatmode.md` |

> **ℹ️ Hinweis:** Alle Talk-Modi sind sofort nutzbar — die referenzierten MD-Dateien existieren bereits im Workspace.

### 🎯 Spezialisierte MD-Talk-Modi

| Parameter | Zielordner | Funktion |
|-----------|-----------|----------|
| `/agents -talk` | `.github/agents/` | Agent-Experten-Diskussion |
| `/prompts -talk` | `.github/prompts/` | Prompt-Engineering-Dialog |
| `/instructions -talk` | `.github/.instructions/` | Programmiersprachen-Experten |
| `/chatmodes -talk` | `.github/chatmodes/` | Chatmode-Designer-Dialog |
| `/md -talk` | Alle MD-Dateien | Markdown-Master-Dialog |

### 📁 Ressourcen-Navigation

| Parameter | Syntax | Funktion | Sofort nutzbar |
|-----------|--------|----------|----------------|
| `/agent` | `/agent CSharpExpert` | Spezifischen Agent laden | ✅ Ja |
| `/chatmode` | `/chatmode debug` | Chatmode aktivieren | ✅ Ja |
| `/prompt` | `/prompt csharp-docs` | Prompt-Template laden | ✅ Ja |
| `/instructions` | `/instructions dotnet-maui` | Sprachregeln laden | ✅ Ja |
| `/translate -ger` | `/translate -ger [text/datei]` | Ins Deutsche übersetzen | ✅ Ja |
| `/team` | `/team` | Projekt-spezifischen Agent erstellen | ✅ Ja |

### 🛠️ Verwaltungs-Parameter

| Parameter | Funktion | Sofort nutzbar |
|-----------|----------|----------------|
| `/index` | Index-Dateien für alle MD-Sammlungen erstellen | ✅ Ja |
| `/search [term]` | Durch alle MD-Dateien suchen | ✅ Ja |
| `/list [type]` | Ressourcen auflisten (agents/chatmodes/prompts/instructions/all) | ✅ Ja |
| `/combine [files]` | Mehrere MD-Dateien zu Super-Agent kombinieren | ✅ Ja |
| `/template [type]` | Neue MD-Datei aus Template erstellen | ✅ Ja |
| `/backup` | Timestamped Backup von .github erstellen | ✅ Ja |
| `/validate` | Alle MD-Dateien auf Syntax prüfen | ✅ Ja |

### 🧠 Arbeits-Modi

| Parameter | Syntax | Funktion | Sofort nutzbar |
|-----------|--------|----------|----------------|
| `/idee` | `/idee [problem]` | Brainstorming-Modus | ✅ Ja |
| `/beast` | `/beast` | Autonomes Problem-Solving | ✅ Ja |
| `/remember` | `/remember` | Memory-System aktivieren | ✅ Ja |
| `/surgical` | `/surgical` | Präzise Code-Änderungen | ✅ Ja |
| `/shadow` | `/shadow` | Reverse Engineering Modus | ✅ Ja |
| `/focus` | `/focus [bereich]` | Fokus auf Code-Bereich | ✅ Ja |
| `/focus -off` | `/focus -off` | Fokus deaktivieren | ✅ Ja |
| `/unfocus` | `/unfocus` | Alias für /focus -off | ✅ Ja |

### UI/UX-Parameter

| Parameter | Funktion | Sofort nutzbar |
|-----------|----------|----------------|
| `/ui` | Vollständiger UI/UX Review | ✅ Ja |
| `/ui -theme` | Dark/Light Theme Validierung | ✅ Ja |
| `/ui -accessibility` | WCAG Accessibility Audit | ✅ Ja |
| `/ui -responsive` | Responsive Design Check | ✅ Ja |
| `/ui -performance` | UI Performance Analyse | ✅ Ja |
| `/ui -consistency` | Design-Konsistenz Check | ✅ Ja |
| `/ui -forms` | Formular-Element Validierung | ✅ Ja |

### ⚡ Boost-System

| Parameter | Level | Funktion |
|-----------|-------|----------|
| `/boost 1` | Basic | Leicht schnellere Antworten |
| `/boost 2` | Moderate | Verbesserte Code-Qualität |
| `/boost 3` | Advanced | Tiefe Analyse, komplexe Lösungen |
| `/boost 4` | Expert | Maximum Intelligence |
| `/boost 5` | BEAST MODE | Keine Grenzen, experimentell |

### 📚 Dokumentations-Parameter

| Parameter | Funktion |
|-----------|----------|
| `/para` | Alle Parameter mit Beschreibungen anzeigen |
| `/para -quick` | Kompaktes Format mit Kurzbeschreibungen |
| `/para -full` | Vollständige Dokumentation mit Beispielen |

---

## 🔧 Kombinierte Modi — Beispiele

```bash
# Debugging mit Clean Code Prinzipien
/debug -talk /clean -talk

# Kritische Architektur-Analyse
/critical -talk /claude -talk

# Elite-Team Problemlösung
/ultimate -talk /debug -talk

# Beast Mode + Fokus auf eine Datei
/beast /focus MainViewModel.cs

# Kurze Antwort + UI Review
/short /ui -theme
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Diese Datei ist vollständig und sofort einsatzbereit!

Alle Parameter funktionieren **ohne weitere Anpassungen**. Die referenzierten Agent- und Chatmode-Dateien existieren bereits. Um neue Parameter hinzuzufügen:

1. Parameter-Block in `copilot-instructions.md` eintragen
2. Entsprechende MD-Datei in `.github/agents/` oder `.github/chatmodes/` erstellen
3. Im Parameter `/[name]` auf die neue Datei verweisen

---

## 💡 Praktische Anwendungsbeispiele

```bash
# Schnelle Antwort auf eine Frage
/short Was ist der Unterschied zwischen ICommand und RelayCommand?

# Vollständiges Team für Projekt-Analyse
/ultimate -talk analysiere die MAUI App Architektur

# Bug fixen mit Debug-Experte
/fix der Button in MainPage.xaml reagiert nicht

# UI überprüfen
/ui -theme überprüfe alle Seiten auf Dark Theme Konsistenz

# Agent für App-Projekt erstellen
/team

# Alle verfügbaren Agents auflisten
/list agents
```

---

## 📌 Wichtige Hinweise

- Die Datei wird **automatisch** von Copilot Chat geladen
- Parameter sind **case-sensitive** — immer Kleinbuchstaben
- Kombinierte Parameter werden durch **Leerzeichen** getrennt
- `/clean -full` und `/clean -save` erfordern **explizite Bestätigung**
- Der `/jailbreak` Parameter schaltet erweiterte Fähigkeiten frei

---

*Erstellt von CODEX — Experten Team Auswahl README System*



