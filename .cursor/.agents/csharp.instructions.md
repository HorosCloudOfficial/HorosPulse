# ⚙️ csharp.instructions.md

**Pfad:** `.github/.instructions/csharp.instructions.md`  
**Typ:** Coding Instructions — C# Sprachstandards  
**Status:** ✅ Sofort verwendbar — automatisch auf `**/*.cs` Dateien angewendet  
**Aktivierung:** Automatisch für alle `.cs` Dateien ODER manuell via `/instructions csharp`  
**Gilt für:** `**/*.cs`

---

## 🔍 Was ist diese Datei?

Definiert **verbindliche C# Code-Standards** für alle C# Dateien im Projekt. Wird automatisch aktiv wenn Copilot auf `.cs` Dateien antwortet.

> ⭐ **Gilt für das gesamte Zielprojekt Projekt** — alle C# Dateien folgen diesen Standards.

---

## 📐 Coding Standards — Übersicht

### Naming Conventions
| Element | Convention | Beispiel |
|---------|-----------|---------|
| Klassen, Methoden, Public Members | PascalCase | `Service.ConnectAsync()` |
| Private Felder, Lokale Variablen | camelCase | `_isConnected`, `serverCount` |
| Interfaces | "I" Präfix | `IService` |

### C# Version
> **Immer C# 13** — neueste Features nutzen

### Formatierungs-Regeln
```csharp
// ✅ File-scoped Namespaces (kein extra Einzug)
namespace Zielprojekt.Services;

// ✅ Single-line usings
using System.Net;
using Microsoft.Extensions.Logging;

// ✅ Newline vor öffnender Klammer
if (condition)
{
    // Code
}

// ✅ Letztes Return auf eigener Zeile
public bool IsConnected()
{
    var status = GetStatus();
    
    return status == Status.Connected;
}
```

---

## 🔷 C# 13 Modern Features (Pflicht)

### Pattern Matching
```csharp
// ✅ Switch-Expression statt switch-Statement
var message = status switch
{
    Status.Connected => "Verbunden",
    Status.Connecting => "Verbinde...",
    Status.Disconnected => "Getrennt",
    _ => "Unbekannt"
};

// ✅ Property Pattern
if (server is { IsActive: true, Country: "DE" })
{
    ConnectToServer(server);
}
```

### Nullable Reference Types
```csharp
// ✅ Non-nullable deklarieren, nur an Entry-Points prüfen
public void ConnectToServer(Server server) // server ist nie null
{
    // Kein null-check nötig — Type System garantiert es
}

// ✅ RICHTIG: is null / is not null
if (server is null) throw new ArgumentNullException(nameof(server));
if (result is not null) return result;

// ❌ FALSCH: == null
if (server == null) ...
```

### nameof() Operator
```csharp
// ✅ RICHTIG: nameof für Member-Namen
OnPropertyChanged(nameof(IsConnected));
throw new ArgumentException("Invalid", nameof(serverName));

// ❌ FALSCH: String-Literal
OnPropertyChanged("IsConnected");
```

---

## 📋 XML Dokumentation (Pflicht für Public APIs)

```csharp
/// <summary>
/// Stellt eine Verbindung zum server her.
/// </summary>
/// <param name="server">Der Ziel-server.</param>
/// <returns>True wenn Verbindung erfolgreich.</returns>
/// <example>
/// <code>
/// var success = await Service.ConnectAsync(selectedServer);
/// </code>
/// </example>
public async Task<bool> ConnectAsync(Server server)
```

---

## ⚡ Parameter-System

### Aktivierungswege

| Syntax | Effekt | Sofort nutzbar |
|--------|--------|----------------|
| **Automatisch** | Bei Antworten auf `*.cs` Dateien | ✅ Ja |
| `/instructions csharp` | Explizite Aktivierung | ✅ Ja |
| `/instructions -talk` | Diskussion über C# Best Practices | ✅ Ja |

---

## 🏗️ Projektstruktur (Feature Folders / DDD)

```
Zielprojekt/
├── Models/         → Datenstrukturen (Records bevorzugt)
│   ├── Server.cs
│   └── Status.cs
├── Services/       → Business-Logic (Interfaces + Implementation)
│   ├── IService.cs
│   └── Service.cs
├── ViewModels/     → State + Commands (MVVM)
│   ├── MainViewModel.cs
│   └── ServersViewModel.cs
└── Views/          → UI (XAML + minimal Code-Behind)
    ├── MainPage.xaml
    └── ServersPage.xaml
```

---

## 🛡️ Error Handling Standards

```csharp
// ✅ Global Exception Handler für API-Calls
try
{
    await _vpnService.ConnectAsync(server);
}
catch (ConnectionException ex)
{
    _logger.LogError(ex, "App Connection failed for server {Server}", server.Name);
    StatusMessage = "Verbindung fehlgeschlagen";
}
catch (OperationCanceledException)
{
    _logger.LogInformation("App Connection cancelled by user");
}
```

---

## 🧪 Testing (Pflicht)

| Framework | Verwendung |
|-----------|-----------|
| xUnit / NUnit / MSTest | Test-Framework |
| Moq / NSubstitute | Mocking |
| FluentValidation | Validierung |
| FluentAssertions | Test-Assertions |

---

## 💡 Praktische Anwendungsbeispiele

```bash
# C# Code Review
/instructions csharp überprüfe Service.cs auf Standards

# Nullable Handling prüfen  
/instructions csharp finde alle == null Vergleiche und ersetze durch is null

# Pattern Matching einführen
/instructions csharp refactore switch-Statements in switch-Expressions

# XML Docs hinzufügen
/instructions csharp ergänze XML-Dokumentation für alle public Methoden in IService.cs
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Die Instructions sind aktiv und sofort wirksam!

**Mögliche Erweiterungen:**
- `.editorconfig` Datei erstellen für konsistentes Formatting
- Projektspezifische Exception-Typen definieren
- Performance-Regeln für kritische Pfade ergänzen

---

*Erstellt von CODEX — Experten Team Auswahl README System*



