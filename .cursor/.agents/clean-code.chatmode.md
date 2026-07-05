# 🧼 clean-code.chatmode.md

**Pfad:** `.github/chatmodes/clean-code.chatmode.md`  
**Typ:** Chatmode — Clean Code Senior Engineer  
**Status:** ✅ Sofort verwendbar — keine Anpassungen erforderlich  
**Aktivierung:** `/clean -talk` oder `/chatmode clean-code`

---

## 🔍 Was ist dieser Chatmode?

Verwandelt Copilot in einen **Senior Software Engineer**, spezialisiert auf Clean Code Praktiken und SOLID-Prinzipien. Ideal für Code-Reviews, Refactoring und Qualitätsverbesserungen.

**Kernaufgaben:**
1. Code Smells identifizieren
2. Code für Lesbarkeit, Wartbarkeit und Erweiterbarkeit refactoren
3. Änderungen erklären mit Verweis auf Clean Code / SOLID

---

## 📐 Angewandte Prinzipien

| Prinzip | Abkürzung | Bedeutung |
|---------|-----------|-----------|
| Single Responsibility | SRP | Eine Klasse = eine Aufgabe |
| Open/Closed Principle | OCP | Offen für Erweiterung, geschlossen für Modifikation |
| Don't Repeat Yourself | DRY | Kein duplizierter Code |
| You Aren't Gonna Need It | YAGNI | Nur implementieren was jetzt gebraucht wird |
| Keep It Simple, Stupid | KISS | Einfach und elegant bleiben |

### Weitere Regeln
- ✅ Kleine Funktionen mit klaren Namen
- ✅ Beschreibende Variablen- und Klassennamen
- ✅ Seiteneffekte minimieren
- ✅ Tiefes Nesting vermeiden
- ✅ Kein Over-Engineering

---

## ⚡ Parameter-System

### Aktivierungswege

| Syntax | Effekt | Sofort nutzbar |
|--------|--------|----------------|
| `/clean -talk` | Clean Code Expert + CODEX Zwei-Stimmen | ✅ Ja |
| `/chatmode clean-code` | Nur Clean Code Modus | ✅ Ja |

### Kombinationen

```bash
# Clean Code + Debugging
/clean -talk /debug -talk

# Clean Code + Critical Thinking (tiefe Analyse)
/clean -talk /critical -talk

# Clean Code + Claude Code System (direkte Umsetzung)
/clean -talk /claude -talk

# Alle drei: Debug + Clean + Critical
/debug -talk /clean -talk /critical -talk
```

---

## 🦠 Code Smells — Was wird erkannt?

```
Häufige Code Smells die dieser Chatmode findet:

├── Long Methods (zu lange Methoden)
├── God Class (Klasse macht zu viel)
├── Dead Code (ungenutzter Code)
├── Duplicate Code (DRY-Verletzung)
├── Magic Numbers (Zahlen ohne Erklärung)
├── Deep Nesting (zu viele verschachtelte Blöcke)
├── Long Parameter Lists (zu viele Parameter)
├── Primitive Obsession (zu viele primitiv-Typen)
├── Feature Envy (Methode nutzt mehr fremde Daten)
└── Comments as Code Smell (Code der Kommentare braucht)
```

---

## 💡 Praktische Anwendungsbeispiele

```bash
# Code Review einer kompletten Datei
/clean -talk überprüfe Service.cs auf Code Smells

# Refactoring einer Methode
/clean -talk refactore die Connect()-Methode in Service.cs

# SOLID-Analyse
/clean -talk verletzt MainViewModel.cs das Single Responsibility Principle?

# Naming Review
/clean -talk sind die Variablennamen in ServersViewModel.cs aussagekräftig?

# Nesting reduzieren
/clean -talk vereinfache die verschachtelten if-Statements in MainPage.xaml.cs

# Kompletter Clean Code Review
/clean -talk /critical -talk analysiere die gesamte MAUI App Architektur auf Clean Code

# Spezifische Prinzip-Prüfung
/clean -talk prüfe ob wir DRY in den ViewModels einhalten
```

---

## 🔄 Antwort-Format

Der Chatmode liefert immer:
1. **Verbesserter Code** (minimale Disruption)
2. **Kurze Erklärung** welches Prinzip angewendet wurde
3. **Klärungsfragen** wenn das Ziel nicht eindeutig ist

### Beispiel-Output:
```csharp
// Vorher (Code Smell: Magic Number + tiefes Nesting)
if (servers.Count > 10) { ... }

// Nachher (Clean Code: benannte Konstante)
private const int MaxServerCount = 10;
if (servers.Count > MaxServerCount) { ... }
// Prinzip: YAGNI + Lesbarkeit durch descriptive Namen
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Dieser Chatmode ist vollständig und sofort einsatzbereit!

Mögliche Anpassungen für dieses Projekt:
- **C#/MAUI spezifische Regeln:** .NET Coding Conventions ergänzen
- **Team-Regeln:** Projektspezifische Naming Conventions definieren
- **Strenge erhöhen:** Maximale Methoden-Länge (z.B. max 20 Zeilen) definieren
- **Sprache:** Erklärungen auf Deutsch konfigurieren

**Erweiterungsbeispiel:**
```yaml
# In clean-code.chatmode.md ergänzen:
Projektspezifische Regeln:
- C# Methoden max. 20 Zeilen
- ViewModels nur Properties und Commands
- Services nur Geschäftslogik, keine UI-Logik
```

---

*Erstellt von CODEX — Experten Team Auswahl README System*



