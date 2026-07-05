# 📋 Plan Mode — Strategic Planning & Architecture

> **Quelldatei:** `.github/chatmodes/plan.chatmode.md`
> **Typ:** Strategischer Planer (kein Code-Edit!)
> **Status:** ✅ Sofort verwendbar

---

## 🎯 Was es ist

> **"Think First, Code Later"** — Strategischer Planer der NIEMALS Code editiert, sondern nur:
> - Codebase versteht
> - Anforderungen klärt
> - Implementations-Strategien entwickelt

## 🛠️ Verfügbare Tools (read-only)

| Tool | Zweck |
|------|-------|
| `codebase` | Code-Struktur untersuchen |
| `search` / `searchResults` | Pattern-Suche |
| `usages` | Wie werden Komponenten verwendet? |
| `problems` | Bestehende Issues |
| `findTestFiles` | Test-Patterns |
| `fetch` | Externe Doku |
| `githubRepo` | Repo-History |
| `vscodeAPI` / `extensions` | IDE-Insights |

> ⚠️ **`editFiles` ist NICHT verfügbar** — bewusst!

## 📊 4-Stufen-Workflow

```
1. UNDERSTAND  → Klärungsfragen, Codebase erkunden
   ↓
2. ANALYZE     → Existing Patterns, Dependencies, Impact
   ↓
3. STRATEGIZE  → Schritte, Alternativen, Risiken
   ↓
4. PRESENT     → Detaillierter Plan mit Begründungen
```

## 💡 Best Practices

### Information Gathering
- **Be Thorough** — Kontext lesen vor Plan
- **Ask Questions** — keine Annahmen
- **Explore Systematically** — Searches + Listings
- **Understand Dependencies** — Komponenten-Interaktion

### Planning Focus
- **Architecture First** — Big-Picture
- **Follow Patterns** — Existing Conventions
- **Consider Impact** — Side Effects
- **Plan for Maintenance**

### Communication
- **Be Consultative** — Berater, kein Coder
- **Explain Reasoning** — Warum dieser Ansatz?
- **Present Options** — Trade-offs aufzeigen
- **Document Decisions**

## 🚀 Aktivierung

```
/chatmode plan
```

## ⚙️ Sofort verwendbar?

✅ **JA — perfekt für Zielprojekt vor großen Features.**

## 💡 Anwendungsbeispiel

```
User: "Plan: Multi-Server-Failover für Zielprojekt"

Plan Mode:
1. Liest Service.cs, ServersViewModel.cs, Server.cs
2. Identifiziert Dependencies + Connection-Logic
3. Stellt Klärungsfragen (max. Reconnect-Versuche? Timeout?)
4. Präsentiert 3 Strategien mit Vor-/Nachteilen
5. Liefert Step-by-Step-Plan (kein Code-Edit!)
```

## 🔄 Verwandte Modi

| Modus | Unterschied |
|-------|-------------|
| `implementation-plan.chatmode.md` | Erstellt strukturierte MD-Pläne in `/plan/` |
| `specification.chatmode.md` | Erstellt formale Specs in `/spec/` |
| `prd.chatmode.md` | Product Requirements Document |



