# ⚙️ agent-configs.md — Advanced Agent Konfigurationen

**Pfad:** `.github/agent-configs.md`  
**Typ:** Systemkonfiguration — Erweiterte Agent-Modi  
**Status:** ✅ Sofort verwendbar — Referenzdokument für Aktivierungsbefehle  
**Zweck:** Definiert spezialisierte CODEX Modi und deren Verhalten

---

## 🔍 Was ist diese Datei?

Erweiterte Konfigurationsdokument für spezielle CODEX Agent-Modi. Definiert Quick-Mode-Aktivierungsbefehle, Verhaltensmodifikatoren und Qualitäts-Gates.

---

## 🚀 Quick Mode Aktivierungsbefehle

| Befehl | Modus | Fokus |
|--------|-------|-------|
| `CODEX Security Mode` | Sicherheitsanalyse | Vulnerability-Analyse, Defensive Security |
| `CODEX Performance Mode` | Performance-Optimierung | Geschwindigkeit und Ressourceneffizienz |
| `CODEX Integration Mode` | Integration-Spezialist | API-Verbindungen und Drittanbieter-Services |
| `CODEX Testing Mode` | Test-Spezialist | Vollständige Test-Abdeckung und Validierung |

### Verwendung
```bash
# Direkt in Chat eingeben
CODEX Security Mode: analysiere Service.cs

CODEX Performance Mode: optimiere die Server-Listen Ladezeit

CODEX Testing Mode: erstelle vollständige Unit Tests für MainViewModel

CODEX Integration Mode: verbinde Zielprojekt mit OpenVPN API
```

---

## 🎛️ Agent Verhaltensmodifikatoren

### Ultra-Autonomous Mode
```
Eigenschaften:
├── Keine Bestätigungsanfragen
├── Maximale Selbstständigkeit
├── Vollständige Aufgabenlösung
└── Production-ready Output

Aktivierung: Direkt CODEX bitten autonom zu arbeiten
```

### Research-Heavy Mode
```
Eigenschaften:
├── Umfassende Internet-Recherche
├── Best Practice Analyse
├── Mehrere Lösungen evaluieren
└── Cutting-edge Implementierung

Wann nutzen: Unbekannte Technologien, neue Frameworks
```

### Legacy Code Mode
```
Eigenschaften:
├── Bestehende Patterns sorgfältig erhalten
├── Minimale Disruptions-Upgrades
├── Rückwärts-Kompatibilitäts-Fokus
└── Änderungen dokumentieren

Wann nutzen: Alte Codebase-Teile modernisieren
```

---

## 🖥️ Projekttyp-Erkennung

| Typ | Erkannte Technologien | Optimierungen |
|-----|----------------------|---------------|
| **Web Apps** | React, Vue, Angular | Component Patterns, Bundle-Size |
| **Desktop Apps** | WPF, WinUI, Electron, MAUI | MVVM, UI-Performance |
| **Backend** | Node.js, .NET, Python | API-Design, Async Patterns |
| **Mobile** | React Native, Flutter, MAUI | State Management, Offline |
| **CLI Tools** | PowerShell, Python, Go | Pipeline Efficiency |
| **Libraries** | Beliebig | Public API Design, Docs |

---

## 🔧 Sprachspezifische Optimierungen

| Sprache | Optimierungs-Fokus |
|---------|-------------------|
| **C#/.NET** | Modern Async Patterns, LINQ, Memory Management |
| **JavaScript/TS** | Modern ES Features, Type Safety, Performance |
| **Python** | Pythonic Patterns, Performance, Libraries |
| **PowerShell** | Module Design, Pipeline, Error Handling |
| **SQL** | Query Optimierung, Security, Best Practices |

---

## ✅ Quality Gates

### Automatische Validierung
```
1. Syntax Validation   → Code compiliert ohne Fehler
2. Style Compliance    → Projekt-Konventionen eingehalten
3. Security Scan       → Keine offensichtlichen Vulnerabilities
4. Performance Check   → Effiziente Algorithmen und Patterns
5. Documentation       → Ausreichende Kommentare und Erklärungen
```

### Manuelle Review-Punkte
```
1. Logic Verification  → Korrekte Business-Logic Implementation
2. Edge Case Coverage  → Umfassendes Error Handling
3. Integration Testing → Funktioniert mit bestehenden Systemen
4. User Experience     → Intuitiv und funktional
5. Maintainability     → Leicht zu verstehen und modifizieren
```

---

## 💡 Praktische Anwendungsbeispiele für Zielprojekt

```bash
# Security Audit der Verbindungslogik
CODEX Security Mode: prüfe Service.cs auf alle OWASP Top 10 Risiken

# Performance-Optimierung der Server-Liste
CODEX Performance Mode: die ServersPage lädt bei 50+ Servern langsam

# Test-Coverage erhöhen
CODEX Testing Mode: erstelle Tests für alle Services mit 80% Coverage

# OpenVPN Integration
CODEX Integration Mode: implementiere echte OpenVPN CLI Integration

# Legacy Code modernisieren
Legacy Code Mode: modernisiere alte sync Methoden auf async/await ohne Breaking Changes
```

---

## 🚨 Emergency Protokolle

```
Bei Stuck-Detection:
├── Alternativen Lösungsweg suchen
├── Kleineres Teilproblem zuerst lösen
└── Eskalations-Protokoll aktivieren

Bei Quality Gate Failure:
├── Sofort stoppen
├── Problem beheben
├── Re-validieren
└── Dann weiterfahren
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Diese Datei ist Dokumentation und sofort als Referenz nutzbar!

Um eigene Agent-Modi hinzuzufügen:
1. Neuen Mode-Block in `agent-configs.md` eintragen
2. In `copilot-instructions.md` als Parameter registrieren
3. Optional: Eigene `.chatmode.md` Datei erstellen

---

*Erstellt von CODEX — Experten Team Auswahl README System*



