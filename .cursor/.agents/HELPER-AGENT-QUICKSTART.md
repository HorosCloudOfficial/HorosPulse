# 🆘 HorosCloudV5 Helper Agent - Quick Start

**Erstellt:** 2026-05-28  
**Agent:** HorosCloudV5 Helper Agent  
**Pfad:** `.github/agents/HorosCloudV5-Helper-Agent.agent.md`

---

## ⚡ WAS IST DER HELPER AGENT?

**Deine zentrale Anlaufstelle für ALLES rund um HorosCloudV5!**

Der Helper Agent ist wie ein **intelligenter Concierge**:
- ✅ Analysiert dein Problem automatisch
- ✅ Gibt dir sofortige Quick-Fixes wenn möglich
- ✅ Leitet dich zum richtigen spezialisierten Agent weiter
- ✅ Hat Zugriff auf alle Memory-Ressourcen
- ✅ Kennt alle Quick Commands

---

## 🚀 SCHNELLSTART

### Einfachste Nutzung:
```bash
/help
```
**Das war's!** Der Helper Agent wird aktiviert und fragt dich, was du brauchst.

---

## 💡 QUICK COMMANDS

### 🚨 Notfall:
```bash
/help server-crash    # Server crashed? Sofortige Diagnose!
/help emergency       # Aktiviert Emergency Debug Agent
```

### 🔍 Diagnose:
```bash
/help status          # Vollständiger System-Status-Check
/help logs            # Zeigt letzte Logs (server.log + server.err)
/help errors          # Zeigt alle aktuellen Fehler
/help ports           # Prüft ob Ports 3000/5173 belegt
```

### 🛠️ Entwicklung:
```bash
/help start           # Startet HorosCloud Server + Web korrekt
/help restart         # Restart mit .env loading
/help feature <name>  # Startet Feature-Entwicklung Workflow
/help bug <area>      # Startet Bug-Fixing Workflow
/help build           # Build Server + Web
```

### 📚 Info:
```bash
/help agents          # Zeigt alle verfügbaren HorosCloud-Agents
/help resources       # Zeigt Memory-Ressourcen
/help teams           # Team-Auswahl Decision Tree
/help workflow <type> # Zeigt Workflow für: feature|bug|security|cleanup
```

---

## 🎯 BEISPIELE

### Beispiel 1: Server crashed
```
DU: /help server-crash

HELPER: 
🚨 SERVER-CRASH ERKANNT!

📊 STATUS-CHECK:
✗ Server Terminal: Exit Code 1
✗ Vite Dev Terminal: Exit Code 1

📝 LOGS:
Error: Cannot find module 'dotenv'
at Server startup...

🔍 ROOT CAUSE:
Fehlende Dependencies!

💡 QUICK-FIX:
cd I:\Cloud\HorosCloudV5\server
npm install
npm run build

Soll ich das für dich ausführen? [Y/n]
```

---

### Beispiel 2: Feature entwickeln
```
DU: /help feature storage

HELPER:
✨ FEATURE-ENTWICKLUNG: Storage

📋 EMPFOHLENER WORKFLOW:
1. Planning Phase
   → Fusion Elite /plan /focus=storage

2. Implementation Phase
   → Fusion Elite /implement /focus=storage

3. Security Check
   → Fusion Elite /security /focus=storage

4. Documentation
   → Fusion Elite /docs -api

🎯 SOLL ICH FUSION ELITE AKTIVIEREN?
/agent HorosCloudV5 Fusion Elite /plan /focus=storage

[Y/n]
```

---

### Beispiel 3: Unsicher was tun
```
DU: Irgendwas funktioniert nicht

HELPER:
🤔 DIAGNOSE-MODUS AKTIVIERT

Lass mich schauen...

✓ Terminal Status: Server Exit Code 1, Vite Exit Code 1
✓ Logs: TypeError in server.log
✓ Build: dist/ Ordner fehlt!

📊 PROBLEM GEFUNDEN:
Server wurde nicht gebaut!

💡 LÖSUNG:
1. Build Server:
   cd I:\Cloud\HorosCloudV5\server
   npm run build

2. Server starten:
   node dist/index.js

Soll ich das für dich machen? [Y/n]
```

---

## 🎨 INTERAKTIVE NUTZUNG

Du kannst auch einfach beschreiben, was du brauchst:

```bash
"Server crashed"
→ Helper analysiert und startet Notfall-Protokoll

"Ich möchte ein Feature entwickeln"
→ Helper zeigt Feature-Workflow und aktiviert Fusion Elite

"Wo sind die Logs?"
→ Helper zeigt Log-Pfade und letzte Einträge

"Wie starte ich HorosCloud?"
→ Helper zeigt korrekte Start-Befehle
```

---

## 🔄 AGENT-WEITERLEITUNG

Der Helper Agent leitet dich automatisch zum richtigen Agent weiter:

| Dein Problem | Helper leitet zu |
|--------------|------------------|
| Server crashed (komplex) | Emergency Debug Agent |
| Feature entwickeln | Fusion Elite `/plan` |
| Bug fixen | Fusion Elite `/debug -talk` |
| Security-Review | Fusion Elite `/security` |
| Code-Cleanup | Fusion Elite `/cleanup-preview` |
| Vollanalyse | Fusion Elite `/deep` |

---

## 💡 WANN HELPER NUTZEN?

### ✅ IMMER nutzen wenn:
- Du unsicher bist, was du brauchst
- Du einen Quick-Fix suchst
- Du nicht weißt, welcher Agent der richtige ist
- Du einen System-Status-Check brauchst
- Du schnelle Hilfe brauchst

### ⚠️ DIREKT zum spezialisierten Agent wenn:
- Du GENAU weißt, was du brauchst
- Du bereits einen Feature-Plan hast
- Du komplexe Architektur-Änderungen machst

---

## 🚀 TYPISCHE WORKFLOWS

### Workflow 1: "Ich will loslegen, weiß aber nicht wie"
```bash
1. /help
2. Helper zeigt System-Status
3. Helper zeigt verfügbare Agents
4. Du wählst aus oder beschreibst dein Ziel
5. Helper aktiviert richtigen Agent
```

### Workflow 2: "Server crashed, was nun?"
```bash
1. /help server-crash
2. Helper diagnostiziert (< 30 Sekunden)
3. Helper gibt Quick-Fix ODER aktiviert Emergency Debug
4. Problem gelöst!
```

### Workflow 3: "Ich möchte Feature X entwickeln"
```bash
1. /help feature <name>
2. Helper zeigt Workflow
3. Helper aktiviert Fusion Elite /plan
4. Du folgst dem Workflow
```

---

## 🎯 VORTEILE

**Warum Helper Agent nutzen?**
- ⚡ **Schnell** - Quick-Fixes in Sekunden
- 🧠 **Intelligent** - Analysiert automatisch
- 🎯 **Präzise** - Leitet zum richtigen Agent
- 📚 **Informiert** - Hat alle Memory-Ressourcen
- 💬 **Interaktiv** - Einfach beschreiben, was du brauchst

---

## 📝 ZUSAMMENFASSUNG

```
GOLDENE REGEL:

❓ Unsicher? → /help
🚨 Notfall? → /help server-crash oder /emergency
✨ Feature? → /help feature <name>
🐛 Bug? → /help bug <area>
📊 Status? → /help status

Der Helper Agent ist DEINE ERSTE ANLAUFSTELLE! 🎯
```

---

**Quick Access:**
- **Aktivierung:** `/help`
- **Quick Commands:** `/help <command>`
- **Interaktiv:** Beschreibe einfach dein Problem!
- **Agent-Datei:** `.github/agents/HorosCloudV5-Helper-Agent.agent.md`

---

**Erstellt:** 2026-05-28  
**Version:** 1.0.0  
**Maintained by:** CODEX
