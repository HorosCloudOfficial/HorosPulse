# Henry++ Referenz-Bibliothek — Changelog

> **Diataxis: optional** · Stand: 2026-07-06  
> Aufzeichnung von Änderungen am lokalen Klon und an der Dokumentation der Henry++-Referenzbibliothek.

---

## [Unreleased]

- Vollständige `/doc-epic`-Dokumentationssuite erstellt
- Alle 19 Repos inventarisiert
- NT-API-Aufrufe aus `memreduct/src/main.c` auf Phase-2-Module gemappt

---

## 2026-07-06 — Initialer Klon & Dokumentation

### Klon-Stand

Die 19 Repos wurden als Shallow Clones (`--depth=1`) eingerichtet:

| Repo | Klon-Datum | Branch | Commit (shallow) |
|------|-----------|--------|-----------------|
| `routine` | 2026-07-06 | master | HEAD |
| `memreduct` | 2026-07-06 | master | HEAD |
| `simplewall` | 2026-07-06 | master | HEAD |
| `processhacker` | 2026-07-06 | master | HEAD |
| `builder` | 2026-07-06 | master | HEAD |
| `autoruns2` | 2026-07-06 | master | HEAD |
| weitere 13 | 2026-07-06 | master | HEAD |

### Dokumentation

- `docs/features/external-henrypp/` angelegt (Slug: `external-henrypp`)
- `README.md` — Hub-Dokument
- `01-uebersicht.md` — Was, Warum, Abgrenzung
- `02-benutzer-anleitung.md` — Klonschritte, Code-Navigation
- `03-einstellungen.md` — Repo-Inventar, Pfade, Toolchain
- `04-architektur.md` — Beziehungsdiagramme (Mermaid), Integrationsfluss
- `05-api-datenmodell.md` — NT-API-Mapping zu TODO §5.x
- `06-faq-troubleshooting.md` — Klonprobleme, Lizenzfragen, Fehlerbilder
- `index.html` — Standalone HTML-Übersicht
- Eintrag in `docs/README.md` unter Epic Features

---

## Pflege-Hinweise

Dieser Changelog wird manuell geführt. Relevante Ereignisse:

- Repo-Updates (`git pull` in einem der Henry++-Repos)
- Neue NT-API-Erkenntnisse die Phase-2-Implementierung beeinflussen
- Lizenzänderungen in Henry++-Repos (selten, aber prüfenswert)
- Neue Repos im Henry++-Portfolio die für WindowsPerformance relevant werden

---

## Verlinkungen

- Hub: [README.md](README.md)
- Master-TODO: [`TODO.md`](../../TODO.md)
