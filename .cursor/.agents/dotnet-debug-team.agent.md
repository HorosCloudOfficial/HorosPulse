# 🛠️ .NET Debug & Fix Team

> **Quelldatei:** `.github/agents/dotnet-debug-team.agent.md`
> **Typ:** Multi-Rollen-Team (5 Rollen) für .NET/WPF
> **Status:** ✅ Sofort verwendbar

---

## 🎯 Was es ist

Kombiniertes Expertenteam für **Debugging + Cleanup + Code-Qualität** in .NET/WPF/MAUI Projekten. Wechselt dynamisch zwischen 5 Rollen je nach Aufgabe.

## 👥 5 Rollen im Workflow

```
PROBLEM → 🔍 Detective (Analyse)
              ↓
         👨‍💼 Architekt (Design)
              ↓
         🧹 C# Spezialist (Implementierung)
              ↓
         🗑️ Entrümpler (Aufräumen)
              ↓
         🧼 Code-Polizist (Qualitätsprüfung)
              ↓
         ✅ LÖSUNG
```

| Rolle | Aufgabe |
|-------|---------|
| 🔍 **DETECTIVE** | 4-Phasen Fehleranalyse: Assessment → Investigation → Resolution → QA |
| 🧹 **C# SPEZIALIST** | C# 13 Modernisierung, Performance (Span<T>, async/await) |
| 👨‍💼 **ARCHITEKT** | SOLID, Design Patterns (DI, Repository, CQRS), TDD/BDD |
| 🗑️ **ENTRÜMPLER** | Dead Code, Unused Deps, Tech Debt eliminieren |
| 🧼 **CODE-POLIZIST** | Clean Code (SRP, OCP, DRY, YAGNI) |

## 🚀 Aktivierung

```
/agent dotnet-debug-team
oder
/dotnet-debug-team
```

## 📋 5 Execution Rules

1. Immer erst reproduzieren, dann fixen
2. Tests nach jeder Änderung
3. Inkrementelle, fokussierte Updates
4. Bestehende Funktionalität erhalten
5. Backup vor größerem Refactoring

## ⚙️ Sofort verwendbar?

✅ **JA — direkt einsetzbar für:**
- Zielprojekt (.NET MAUI 9)
- Alle WPF/.NET Projekte: PCManager, GitHubManager, MedienManager, etc.
- Optimiert für `microsoft.docs.mcp` Integration

## 💡 Anwendungsfall Zielprojekt

```
Bug im Zielprojekt Service → /agent dotnet-debug-team

→ Detective findet Race Condition in Service.cs
→ Architekt empfiehlt SemaphoreSlim Pattern
→ C# Spezialist implementiert async/await korrekt
→ Entrümpler entfernt obsolete sync-Variante
→ Code-Polizist prüft SOLID-Compliance
```



