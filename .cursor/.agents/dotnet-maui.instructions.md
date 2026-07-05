# 📱 dotnet-maui.instructions.md

**Pfad:** `.github/.instructions/dotnet-maui.instructions.md`  
**Typ:** Coding Instructions — .NET MAUI Sprachstandards  
**Status:** ✅ Sofort verwendbar — automatisch auf `**/*.xaml, **/*.cs` Dateien angewendet  
**Aktivierung:** Automatisch für alle XAML/C# Dateien ODER manuell via `/instructions dotnet-maui`  
**Gilt für:** `**/*.xaml`, `**/*.cs`

---

## 🔍 Was ist diese Datei?

Definiert **verbindliche Code-Standards** für .NET MAUI Projekte. Wird automatisch angewendet wenn Copilot auf XAML oder C# Dateien antwortet — keine manuelle Aktivierung nötig.

> ⭐ **Besonders relevant für das Zielprojekt Projekt** — alle MAUI-Dateien profitieren direkt.

---

## 📐 Code-Style Regeln

### Naming Conventions
| Element | Convention | Beispiel |
|---------|-----------|---------|
| Klassen | PascalCase | `Service`, `MainViewModel` |
| Methoden | PascalCase | `ConnectAsync()`, `LoadServers()` |
| Public Properties | PascalCase | `IsConnected`, `ServerList` |
| Private Felder | camelCase | `_vpnService`, `_isLoading` |
| Interfaces | "I" Präfix | `IService`, `ISettingsService` |
| Lokale Variablen | camelCase | `serverCount`, `isValid` |

### C# Version
- **Aktuell: C# 13** — immer neueste Features nutzen:
  - Record Types
  - Pattern Matching
  - Global Usings
  - Nullable Reference Types

---

## ⚡ Parameter-System

> **Hinweis:** Instructions haben kein eigenes Parameter-System. Sie werden aktiviert via:

### Aktivierungswege

| Syntax | Effekt | Sofort nutzbar |
|--------|--------|----------------|
| **Automatisch** | Bei Antworten auf `*.xaml`/`*.cs` Dateien | ✅ Ja |
| `/instructions dotnet-maui` | Explizite Aktivierung | ✅ Ja |
| `/instructions -talk` | Diskussion über MAUI Best Practices | ✅ Ja |

---

## 🏗️ Architektur-Regeln

### Separation of Concerns
```
Views (*.xaml)          → Nur UI, kein Business-Logic
Code-Behind (*.xaml.cs) → Minimal — nur UI-Events
ViewModels              → State + Commands + Business-Logic Interface
Services                → Pure Business-Logic und API-Calls
Models                  → Datenstrukturen (Records bevorzugt)
```

### Component Lifecycle
```csharp
// Pflicht: OnAppearing für Daten-Ladung nutzen
protected override async void OnAppearing()
{
    base.OnAppearing();
    await ViewModel.LoadDataAsync();
}

// OnDisappearing für Cleanup
protected override void OnDisappearing()
{
    base.OnDisappearing();
    ViewModel.Cleanup();
}
```

---

## 🔄 Async/Await Regeln

```csharp
// ✅ RICHTIG: Async für alle UI-blockierenden Operationen
public async Task ConnectAsync()
{
    IsLoading = true;
    try 
    {
        await _vpnService.ConnectAsync();
    }
    finally 
    {
        IsLoading = false;
    }
}

// ❌ FALSCH: Sync auf UI Thread
public void Connect()
{
    _vpnService.Connect(); // Blockiert UI!
}
```

---

## 📡 Data Binding Standards

```xaml
<!-- ✅ RICHTIG: Proper Binding mit StringFormat -->
<Label Text="{Binding ServerName}" />
<Button Command="{Binding ConnectCommand}" 
        IsEnabled="{Binding CanConnect}" />

<!-- ✅ RICHTIG: OneWay für Read-Only -->
<Label Text="{Binding Status, Mode=OneWay}" />
```

---

## ⚡ Performance-Optimierung

| Regel | Warum |
|-------|-------|
| `OnPropertyChanged()` sparsam | Verhindert unnötige Re-Renders |
| `BatchBegin()` / `BatchCommit()` | Mehrere Properties gleichzeitig ändern |
| `IMemoryCache` für häufige Daten | API-Calls reduzieren |
| Async für alle API-Calls | UI nicht blockieren |

---

## 🛡️ Security-Regeln

```csharp
// ✅ PFLICHT: HTTPS für alle Kommunikation
private const string BaseUrl = "https://api.Zielprojekt.com";

// ✅ PFLICHT: OAuth/JWT für Authentication
// ❌ VERBOTEN: Credentials im Code speichern
```

---

## 🧪 Testing-Standards

| Framework | Zweck |
|-----------|-------|
| xUnit / NUnit / MSTest | Unit Tests |
| Moq / NSubstitute | Mocking von Dependencies |
| MAUI Community Toolkit Logger | UI-Level Error Capturing |

---

## 💡 Praktische Anwendungsbeispiele

```bash
# MAUI-spezifische Code-Erstellung
/instructions dotnet-maui erstelle einen neuen ViewModel für Status

# MAUI Best Practice Review
/instructions -talk überprüfe ob MainPage.xaml.cs dem MAUI Pattern folgt

# Performance Check
/instructions dotnet-maui optimiere die Server-Liste im ServersViewModel

# Async Pattern prüfen
/instructions dotnet-maui prüfe alle sync Methoden auf async Potenzial
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN für Basis-Nutzung** — Die Instructions sind aktiv und sofort wirksam!

**Mögliche Erweiterungen für Zielprojekt:**
```markdown
# In dotnet-maui.instructions.md ergänzen:

## Allgemeine Regeln
- Status immer als Enum (Status) darstellen
- Server-Objekte als Records (unveränderlich)
- Alle Operationen müssen CancellationToken unterstützen
- UI zeigt immer Loading-Indicator während Operations
```

---

*Erstellt von CODEX — Experten Team Auswahl README System*



