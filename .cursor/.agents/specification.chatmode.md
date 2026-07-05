# 📜 Specification Mode

> **Quelldatei:** `.github/chatmodes/specification.chatmode.md`
> **Typ:** Specification Document Generator
> **Status:** ✅ Sofort verwendbar

---

## 🎯 Was es ist

Erstellt **AI-ready Spezifikationsdokumente** die Anforderungen, Constraints und Interfaces klar, eindeutig und strukturiert definieren.

## 📋 Output-Spezifikation

| Eigenschaft | Wert |
|-------------|------|
| **Speicherort** | `/spec/` Verzeichnis |
| **Naming** | `spec-[a-z0-9-]+.md` |
| **Purpose-Prefixes** | `schema`, `tool`, `data`, `infrastructure`, `process`, `architecture`, `design` |
| **Beispiele** | `spec-data-server-schema.md`, `spec-tool-network-monitor.md` |

## 📚 Best Practices für AI-Ready Specs

- ✅ Präzise, explizite, eindeutige Sprache
- ✅ Klare Trennung: Requirements vs. Constraints vs. Recommendations
- ✅ Strukturiertes Formatting (Headings, Lists, Tables)
- ✅ KEINE Idiome, Metaphern, kontext-abhängige Verweise
- ✅ Alle Akronyme + Domain-Terms definieren
- ✅ Beispiele + Edge Cases inklusive
- ✅ Self-contained (keine externen Abhängigkeiten)

## 🏗️ Template-Struktur (10 Sektionen)

```markdown
---
title: [Title]
version: 1.0
date_created: YYYY-MM-DD
owner: [Team]
tags: [...]
---

# Introduction

## 1. Purpose & Scope
## 2. Definitions
## 3. Requirements, Constraints & Guidelines
   - REQ-001, SEC-001, CON-001, GUD-001, PAT-001

## 4. Interfaces & Data Contracts
## 5. Acceptance Criteria
   - AC-001: Given [...] When [...] Then [...]

## 6. Test Automation Strategy
   - Test Levels, Frameworks, CI/CD
   - Coverage Requirements
   - Performance Testing

## 7. Rationale & Context
## 8. Dependencies & External Integrations
   - EXT-001, SVC-001, INF-001

## 9. Examples & Edge Cases
## 10. Validation Criteria
```

## 🧪 Test Strategy Standard

```
- Test Levels: Unit, Integration, End-to-End
- Frameworks: MSTest, FluentAssertions, Moq (.NET)
- CI/CD: GitHub Actions
- Coverage: Min. Threshold definiert
- Performance: Load Testing
```

## 🚀 Aktivierung

```
/chatmode specification
```

## ⚙️ Sofort verwendbar?

✅ **JA — speziell für .NET-Projekte vorkonfiguriert (MSTest, Moq).**

## 💡 Anwendungsfall Zielprojekt

```
"Erstelle Spec für server-Auswahl-Algorithmus"

→ spec-design-server-selection.md
   ✓ AC-001: Given Latenz-Daten When User wählt aus Then Algorithmus...
   ✓ REQ-001: Server-Auswahl in <100ms
   ✓ SEC-001: Keine Server-Liste an Drittanbieter
```

## 🔄 Verwandte Modi

| Modus | Fokus |
|-------|-------|
| `plan.chatmode.md` | Konversationelle Strategie |
| `implementation-plan.chatmode.md` | Ausführbare Pläne |
| **`specification.chatmode.md`** | **Formale Anforderungen** ⭐ |
| `prd.chatmode.md` | Product Requirements (Business) |



