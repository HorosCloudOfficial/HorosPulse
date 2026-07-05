# 📐 Implementation Plan Mode

> **Quelldatei:** `.github/chatmodes/implementation-plan.chatmode.md`
> **Typ:** AI-zu-AI Plan-Generator (kein Code-Edit!)
> **Status:** ✅ Sofort verwendbar

---

## 🎯 Was es ist

Generiert **strukturierte, deterministische Implementations-Pläne** die von anderen AI-Systemen oder Menschen direkt ausgeführt werden können. Macht **KEINE Code-Änderungen** — nur Pläne.

## 📋 Output-Spezifikation

| Eigenschaft | Wert |
|-------------|------|
| **Speicherort** | `/plan/` Verzeichnis |
| **Naming** | `[purpose]-[component]-[version].md` |
| **Purpose-Prefixes** | `upgrade`, `refactor`, `feature`, `data`, `infrastructure`, `process`, `architecture`, `design` |
| **Beispiel** | `upgrade-system-command-4.md`, `feature-auth-module-1.md` |

## 🏗️ Mandatory Template Structure

```markdown
---
goal: [Concise Title]
version: 1.0
date_created: YYYY-MM-DD
last_updated: YYYY-MM-DD
owner: [Team/Individual]
status: 'Planned' | 'In progress' | 'Completed' | 'Deprecated' | 'On Hold'
tags: [feature, upgrade, ...]
---

# Introduction
![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

## 1. Requirements & Constraints
- **REQ-001**: ...
- **SEC-001**: ...
- **CON-001**: ...
- **GUD-001**: ...
- **PAT-001**: ...

## 2. Implementation Steps
### Implementation Phase 1
[atomare Tasks mit Datei-Pfaden, Funktionsnamen]

## 3. Alternatives
## 4. Dependencies
## 5. Files
## 6. Testing
## 7. Risks & Assumptions
## 8. Related Specifications / Further Reading
```

## 🎯 AI-Optimized Standards

- ✅ **Explizite, eindeutige Sprache** (zero interpretation)
- ✅ **Maschinell parsbar** (Tabellen, Listen)
- ✅ **Spezifische Pfade + Funktionsnamen**
- ✅ **Standardisierte Prefixes** (REQ-, TASK-, SEC-, CON-, GUD-, PAT-)
- ✅ **Validierungskriterien** (auto-prüfbar)

## 🏷️ Status-Badges

| Status | Farbe |
|--------|-------|
| Completed | 🟢 Bright Green |
| In progress | 🟡 Yellow |
| Planned | 🔵 Blue |
| Deprecated | 🔴 Red |
| On Hold | 🟠 Orange |

## 🚀 Aktivierung

```
/chatmode implementation-plan
```

## ⚙️ Sofort verwendbar?

✅ **JA — perfekt für Zielprojekt-Features:**
```
"Erstelle Implementation Plan für Multi-Server-Failover"
→ feature-app-multiserver-1.md mit allen Phasen
```

## 🔄 Verwandte Modi

| Modus | Output |
|-------|--------|
| `plan.chatmode.md` | Konversationelle Strategie |
| **`implementation-plan.chatmode.md`** | **Strukturierte MD-Pläne** ⭐ |
| `specification.chatmode.md` | Formale Specs |
| `prd.chatmode.md` | Product Requirements |



