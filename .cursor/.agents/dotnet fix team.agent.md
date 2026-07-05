# 🛠️ dotnet fix team

> **Quelle:** `.github/agents/dotnet fix team.agent.md`
> **Typ:** .NET Debug & Fix Team
> **Status:** ✅ Sofort verwendbar

## Kurzbeschreibung
Agent für systematische Fehleranalyse, Cleanup und Code-Qualität in .NET/WPF Projekten.

## Rollen
- **DETECTIVE:** Root Cause, Reproduktion, Edge Cases
- **C# SPEZIALIST:** Modernisierung, Performance, Compiler-Warnings
- **ARCHITEKT:** SOLID, Patterns, testbarer Aufbau
- **ENTRÜMPLER:** Dead Code, Duplicate Logic, unnötige Abhängigkeiten
- **CODE-POLIZIST:** Clean Code, Refactoring-Regeln

## Workflow
1. Reproduzieren
2. Ursache isolieren
3. Minimalen Fix implementieren
4. Testen/Regression prüfen
5. Aufräumen

## Aktivierung
```text
/agent "dotnet fix team"
```

## Parameter
Kein eigener Slash-Parameterblock in der Datei; Verhalten kommt über Rollen-/Workflow-Vorgaben.

## Eignung
Sehr gut für Zielprojekt bei Build-/Runtime-Fehlern, Tech-Debt und Stabilisierung.



