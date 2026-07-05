# 🐛 debug.chatmode.md

**Pfad:** `.github/chatmodes/debug.chatmode.md`  
**Typ:** Chatmode — Systematischer Debug-Assistent  
**Status:** ✅ Sofort verwendbar — keine Anpassungen erforderlich  
**Aktivierung:** `/fix` oder `/debug -talk` oder `/chatmode debug`

---

## 🔍 Was ist dieser Chatmode?

Der **Debug Chatmode** verwandelt Copilot in einen systematischen Debug-Spezialisten. Er folgt einem strukturierten 4-Phasen-Prozess, um Bugs zuverlässig zu identifizieren, zu analysieren und zu beheben.

**Aktivierte Tools:**
- `edit/editFiles` — Code-Dateien bearbeiten
- `search` — Im Codebase suchen
- `runCommands` — Terminal-Befehle ausführen
- `usages` — Code-Usages analysieren
- `problems` — Fehler und Warnungen prüfen
- `testFailure` — Fehlgeschlagene Tests analysieren
- `fetch` — Web-Ressourcen abrufen
- `githubRepo` — GitHub Repository analysieren
- `runTests` — Tests ausführen

---

## 🔄 Die 4 Phasen des Debug-Prozesses

### Phase 1: Problem-Assessment
```
1. Kontext sammeln
   ├── Fehlermeldungen und Stack Traces lesen
   ├── Codebase-Struktur untersuchen
   ├── Erwartetes vs. tatsächliches Verhalten klären
   └── Relevante Tests prüfen

2. Bug reproduzieren
   ├── Anwendung/Tests ausführen
   ├── Reproduktionsschritte dokumentieren
   └── Bug-Report erstellen:
       ├── Schritte zur Reproduktion
       ├── Erwartetes Verhalten
       ├── Tatsächliches Verhalten
       ├── Fehlermeldungen/Stack Traces
       └── Umgebungsdetails
```

### Phase 2: Investigation
```
3. Root-Cause-Analyse
   ├── Code-Ausführungspfad tracen
   ├── Variablenzustände und Datenflüsse prüfen
   ├── Häufige Fehler suchen:
   │   ├── Null-Referenzen
   │   ├── Off-by-One-Fehler
   │   ├── Race Conditions
   │   └── Falsche Annahmen
   └── Git-History auf kürzliche Änderungen prüfen

4. Hypothesen bilden
   ├── Spezifische Ursachen-Hypothesen formulieren
   ├── Nach Wahrscheinlichkeit priorisieren
   └── Verifikationsschritte planen
```

### Phase 3: Resolution
```
5. Fix implementieren
   ├── Minimale, gezielte Änderungen
   ├── Bestehende Code-Patterns einhalten
   ├── Defensive Programmierung anwenden
   └── Edge Cases berücksichtigen

6. Verifikation
   ├── Tests ausführen
   ├── Reproduktionsschritte bestätigen
   ├── Breitere Test-Suites laufen lassen
   └── Edge Cases testen
```

### Phase 4: Quality Assurance
```
7. Code-Qualität
   ├── Fix auf Qualität prüfen
   ├── Regressions-Tests hinzufügen/aktualisieren
   ├── Dokumentation aktualisieren
   └── Ähnliche Bugs im Codebase suchen

8. Abschlussbericht
   ├── Was wurde gefixt und wie
   ├── Root Cause erklären
   ├── Präventivmaßnahmen dokumentieren
   └── Verbesserungsvorschläge machen
```

---

## ⚡ Parameter-System

> **Hinweis:** Der Debug Chatmode hat kein eigenes internes Parameter-System. Er wird über das copilot-instructions.md Haupt-System aktiviert.

| Aktivierungsweg | Syntax | Beschreibung |
|----------------|--------|--------------|
| **Fix-Parameter** | `/fix [beschreibung]` | Sofort Debug-Modus starten |
| **Talk-Parameter** | `/debug -talk [beschreibung]` | Debug + CODEX Zwei-Stimmen |
| **Chatmode-Parameter** | `/chatmode debug` | Chatmode direkt laden |

### Kombinationen mit anderen Modi

```bash
# Debug + Clean Code Prinzipien anwenden
/debug -talk /clean -talk

# Debug + Elite-Team Unterstützung
/debug -talk /ultimate -talk

# Schnelles Debugging (kurze Antworten)
/short /fix der Kompilierfehler in Service.cs

# Beast Mode Debugging (nicht aufhören bis gelöst)
/beast /fix alle Tests schlagen fehl
```

---

## 💡 Praktische Anwendungsbeispiele

```bash
# Einfacher Bug-Fix
/fix der Aktions-Button reagiert nicht nach dem Klick

# Stack Trace analysieren
/fix NullReferenceException in MainViewModel.cs Zeile 45

# Test-Fehler beheben
/fix alle Unit Tests schlagen fehl nach letztem Commit

# Komplexer Debug mit Team
/debug -talk /ultimate -talk analysiere warum die App beim Start abstürzt

# Build-Fehler fixen
/fix error CS0246: Der Typ oder Namespace "Service" wurde nicht gefunden

# Performance-Problem debuggen
/debug -talk die App friert ein wenn mehr als 10 Server geladen werden
```

---

## 🔧 Debugging-Grundsätze (integriert)

| Prinzip | Bedeutung |
|---------|-----------|
| **Systematisch** | Phasen methodisch folgen, nicht zu schnellen Lösungen springen |
| **Dokumentieren** | Alle Findings und Versuche aufzeichnen |
| **Inkrementell** | Kleine, testbare Änderungen statt großer Refactors |
| **Kontextsensitiv** | Breiteren System-Impact berücksichtigen |
| **Fokussiert** | Nur den spezifischen Bug adressieren |
| **Gründlich testen** | Fix in verschiedenen Szenarien verifizieren |

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Dieser Chatmode ist vollständig und sofort einsatzbereit!

Die Datei ist fertig konfiguriert. Mögliche Anpassungen:
- **Neue Tools hinzufügen:** In der YAML-Frontmatter `tools:` Liste erweitern
- **Neue Phasen:** Debug-Phasen um projektspezifische Schritte ergänzen
- **Sprache:** Anweisungen ins Deutsche übersetzen für konsistente Kommunikation

**Verfügbare Tool-Optionen für Erweiterung:**
```yaml
tools: ['edit/editFiles', 'search', 'runCommands', 'usages', 'problems', 
        'testFailure', 'fetch', 'githubRepo', 'runTests', 'codebase']
```

---

*Erstellt von CODEX — Experten Team Auswahl README System*



