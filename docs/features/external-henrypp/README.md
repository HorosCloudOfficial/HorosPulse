# Henry++ Referenz-Bibliothek

> **Planungsartefakt / Entwickler-Referenz** · Stand: 2026-07-06  
> 19 lokale Shallow Clones von [github.com/henrypp](https://github.com/henrypp) als Code-Referenz für undokumentierte Windows-Native-APIs — Grundlage für Phase-2-Module in WindowsPerformance.

---

## Auf einen Blick

`external/henrypp/` enthält 19 freie Windows-Systemprogramme in purem C von Entwickler Henry++. Sie sind **nicht** in die Solution eingebunden — sie dienen ausschließlich als Lese-Referenz für NT-API-Aufrufe, die WindowsPerformance ab Phase 2 über `Vanara.PInvoke.NtDll` (NuGet) selbst implementiert.

### Vier Schlüssel-Repos

| Repo | Pfad | Relevanz für WindowsPerformance |
|------|------|--------------------------------|
| `memreduct` | `external/henrypp/memreduct/` | **§5.4** Memory Optimizer — `NtSetSystemInformation` + Standby-Liste |
| `routine` | `external/henrypp/routine/` | **Gemeinsames C-SDK** — `ntapi.h` mit allen NT-Typdefinitionen |
| `simplewall` | `external/henrypp/simplewall/` | **§5.5** Network Optimizer — WFP-Filter-API-Referenz |
| `processhacker` | `external/henrypp/processhacker/` | **§5.8** Process Inspector — Handle-Count, Working Set |

### Abgrenzung

```
Henry++ Repos     →  Lese-Referenz (C, MSVC)    →  gitignored, nie committed
Vanara NuGet      →  .NET-Bindungen (C#)         →  eingebundene Abhängigkeit
ElevationHelper   →  UAC-Brücke für NT-Calls     →  Teil der Solution
```

---

## Inhaltsverzeichnis

| Datei | Diataxis | Inhalt |
|-------|----------|--------|
| [01-uebersicht.md](01-uebersicht.md) | Erklärung | Was Henry++ ist, warum die Repos geklont wurden, Abgrenzung |
| [02-benutzer-anleitung.md](02-benutzer-anleitung.md) | Anleitung | Repos klonen, Submodule, Quellcode navigieren, Code-Beispiele |
| [03-einstellungen.md](03-einstellungen.md) | Referenz | Vollständiges Repo-Inventar (19 Repos), Pfade, Toolchain, .gitignore |
| [04-architektur.md](04-architektur.md) | Erklärung | Repo-Beziehungsdiagramme (Mermaid), Übertragung nach .NET, UAC-Kontrast |
| [05-api-datenmodell.md](05-api-datenmodell.md) | Referenz | NT-API-Aufrufe mapped auf TODO §5.x, C-Typen, C#-Äquivalente |
| [06-faq-troubleshooting.md](06-faq-troubleshooting.md) | Anleitung | Klonprobleme, Lizenzen, `STATUS_PRIVILEGE_NOT_HELD`, WFP-Verhalten |
| [07-changelog-feature.md](07-changelog-feature.md) | optional | Klon-Stand, Dokumentationshistorie |
| [index.html](index.html) | — | Standalone HTML-Übersicht (Dark Theme, Sidebar, Mermaid) |

---

## NT-API-Mapping (Kurzfassung)

| TODO-Item | Phase | NT-API | Henry++ Quelldatei |
|-----------|-------|--------|--------------------|
| §5.4 Memory Optimizer | Phase 2 | `NtSetSystemInformation(SystemMemoryListInformation, MemoryPurgeStandbyList)` | `memreduct/src/main.c:419` |
| §5.4 Working Sets | Phase 2 | `NtSetSystemInformation(SystemMemoryListInformation, MemoryEmptyWorkingSets)` | `memreduct/src/main.c:383` |
| §5.4 File Cache | Phase 2 | `NtSetSystemInformation(SystemFileCacheInformationEx, ...)` | `memreduct/src/main.c:395` |
| §5.5 Network (WFP) | Phase 3+ | `FwpmFilterAdd0`, `FwpmEngineOpen0` | `simplewall/src/` |
| §5.8 Process Inspector | Phase 2 | `NtQuerySystemInformation(SystemProcessInformation)` | `processhacker/` |
| §5.2 Startup-Manager | Phase 2 | Registry `HKCU\...\Run` | `autoruns2/src/` |

---

## Wichtige Hinweise

- `external/henrypp/` ist in `.gitignore` Zeile 84 ausgeschlossen
- WindowsPerformance compiliert **keinen** C-Code aus diesen Repos
- `simplewall` ist **GPL-3.0** — kein Code kopieren; alle anderen Schlüssel-Repos sind MIT
- NT-API-Aufrufe erfordern `SeProfileSingleProcessPrivilege` → läuft via `ElevationHelper.exe`

---

## Verlinkungen

- Master-TODO: [`TODO.md`](../../TODO.md)
- NuGet-Empfehlung `Vanara.PInvoke.NtDll`: [planning-todo-scan-externe-repos/README.md](../planning-todo-scan-externe-repos/README.md)
- Projekt-Dokumentation: [`docs/README.md`](../../docs/README.md)
- Branch-Workflow: [`docs/BRANCHING.md`](../BRANCHING.md)
