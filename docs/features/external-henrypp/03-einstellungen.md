# Henry++ Referenz-Bibliothek — Repo-Inventar & Konfiguration

> **Diataxis: Referenz** · Stand: 2026-07-06  
> Vollständige Inventur der 19 Repos in `external/henrypp/`: Pfade, Sprachen, Build-Toolchain, .gitignore-Verhalten und Beziehung zum WindowsPerformance-Projekt.

---

## .gitignore-Konfiguration

Die Henry++-Repos sind vollständig aus dem WindowsPerformance-Git-Repository ausgeschlossen:

```
# D:\WindowsPerformance\.gitignore — Zeile 84
external/henrypp/
```

Das bedeutet:
- `git status` zeigt das Verzeichnis nicht an
- `git add .` schließt es aus
- Nach einem frischen `git clone` von `WindowsPerformance` existiert `external/henrypp/` nicht
- Manuelle Klon-Schritte sind erforderlich (→ [02-benutzer-anleitung.md](02-benutzer-anleitung.md))

---

## Verzeichnisstruktur

```
D:\WindowsPerformance\
└── external\
    └── henrypp\           ← gitignored
        ├── autoruns2\
        ├── builder\
        ├── chrlauncher\
        ├── dbgvision\
        ├── drivedotshield\
        ├── errorlookup\
        ├── freeshooter\
        ├── henrypp\        ← GitHub-Profil-Repo (kein Code)
        ├── henrypp.org\    ← Website-Quellcode
        ├── hostsmgr\
        ├── inetops\
        ├── iplookup\
        ├── matrix\
        ├── memreduct\
        ├── processhacker\  ← geklont als systeminformer
        ├── routine\
        ├── simplewall\
        ├── timevertor\
        └── uninstmgr\
```

---

## Vollständige Repo-Tabelle

| # | Repo-Name | Pfad | Sprache | Submodule | Build-Toolchain | Relevanz |
|---|-----------|------|---------|-----------|----------------|---------|
| 1 | `autoruns2` | `external/henrypp/autoruns2/` | C | `routine` | MSVC + NSIS | §5.2 Startup |
| 2 | `builder` | `external/henrypp/builder/` | Python | — | Python 3.13+ | Build-Skripte |
| 3 | `chrlauncher` | `external/henrypp/chrlauncher/` | C | `routine` | MSVC + NSIS | — |
| 4 | `dbgvision` | `external/henrypp/dbgvision/` | C | `routine` | MSVC | Debug-Hilfe |
| 5 | `drivedotshield` | `external/henrypp/drivedotshield/` | C | `routine` | MSVC + NSIS | — |
| 6 | `errorlookup` | `external/henrypp/errorlookup/` | C | `routine` | MSVC + NSIS | — |
| 7 | `freeshooter` | `external/henrypp/freeshooter/` | C | `routine` | MSVC + NSIS | — |
| 8 | `henrypp` | `external/henrypp/henrypp/` | — | — | — | kein Code |
| 9 | `henrypp.org` | `external/henrypp/henrypp.org/` | HTML/CSS | — | — | Website |
| 10 | `hostsmgr` | `external/henrypp/hostsmgr/` | C | `routine` | MSVC + NSIS | — |
| 11 | `inetops` | `external/henrypp/inetops/` | C | `routine` | MSVC + NSIS | §5.5 peripher |
| 12 | `iplookup` | `external/henrypp/iplookup/` | C | `routine` | MSVC + NSIS | — |
| 13 | `matrix` | `external/henrypp/matrix/` | C | `routine` | MSVC + NSIS | — |
| 14 | `memreduct` | `external/henrypp/memreduct/` | C | `routine` | MSVC + NSIS | **§5.4 Memory** |
| 15 | `processhacker` | `external/henrypp/processhacker/` | C | — | MSVC | **§5.8 Prozesse** |
| 16 | `routine` | `external/henrypp/routine/` | C | — | MSVC | **Gemeinsames SDK** |
| 17 | `simplewall` | `external/henrypp/simplewall/` | C | `routine` | MSVC + NSIS | **§5.5 Network** |
| 18 | `timevertor` | `external/henrypp/timevertor/` | C | `routine` | MSVC + NSIS | — |
| 19 | `uninstmgr` | `external/henrypp/uninstmgr/` | C | `routine` | MSVC + NSIS | — |

---

## Schlüssel-Repo-Details

### `routine` — SDK-Bibliothek

**Pfad:** `external/henrypp/routine/src/`

| Datei | Inhalt |
|-------|--------|
| `routine.h` | Hauptheader, inkludiert alle anderen |
| `ntapi.h` | NT-API-Typdefinitionen (~10 000 Zeilen); enthält `NtSetSystemInformation`, `SYSTEM_MEMORY_LIST_COMMAND`, WFP-Typen |
| `ntrtl.h` | NT Runtime Library Definitionen |
| `rtypes.h` | Basis-Typdefinitionen (erweitert Windows.h) |
| `rapp.h` | App-Framework (Fenster, Tray, Settings, Updates) |
| `rconfig.h` | Build-Konfigurationsdefinitionen |

**Build-Konfigurationsflags** (compiletime defines):
```c
APP_BETA               // Pre-Release-Status
APP_HAVE_AUTORUN       // Autorun-Feature
APP_HAVE_SETTINGS_TABS // Einstellungs-Dialog mit Tabs
APP_HAVE_SKIPUAC       // UAC-Bypass via Task Scheduler
APP_HAVE_TRAY          // Tray-Icon-Feature
APP_HAVE_UPDATES       // Update-Check-Feature
APP_NO_APPDATA         // Portable-Build
APP_NO_CONFIG          // Keine Konfiguration schreiben
APP_NO_GUEST           // Kein Gast-Modus
APP_NO_MUTEX           // Kein App-Mutex
PR_SAFE_STRING         // Sichere String-Funktionen
```

---

### `memreduct` — Memory Optimizer

**Pfad:** `external/henrypp/memreduct/`

| Datei | Inhalt |
|-------|--------|
| `src/main.c` | Gesamter Quellcode; Memory-Flush-Funktion ab Zeile 370 |
| `bin/` | Binaries und Lokalisierungsdateien |
| `res/` | Ressourcen (Icons, Manifeste) |
| `.gitmodules` | Verweist auf `routine` als Submodul |

**Systemanforderungen (Original-App):**
- Windows 7, 8, 8.1, 10, 11 (64-Bit/ARM64)
- SSE2-fähige CPU
- Administratorrechte für Speicher-Flush-Operationen

**Lizenz:** MIT · **Aktiv seit:** 2011

---

### `simplewall` — WFP-Firewall

**Pfad:** `external/henrypp/simplewall/`

| Verzeichnis | Inhalt |
|-------------|--------|
| `src/` | C-Quellcode, WFP-Filter-Implementierung |
| `bin/rules/` | Standardregeln (XML) |
| `bin/i18n/` | Lokalisierungsdateien (.ini) |

**Kernmerkmal:** Konfiguriert WFP direkt — kein Wrapper über Windows Firewall. Permanent- vs. Temporärregeln (Reboot-Reset).

**Lizenz:** GPL-3.0 · **Aktiv seit:** 2016

---

### `processhacker` — System Informer

**Pfad:** `external/henrypp/processhacker/`

> Hinweis: Tatsächlich der [winsiderss/systeminformer](https://github.com/winsiderss/systeminformer)-Fork, der ursprüngliche Process Hacker 2 wurde als System Informer weitergeführt.

**Systemanforderungen:** Windows 10+, 32-Bit oder 64-Bit

**Build:**
```powershell
# Im Verzeichnis external/henrypp/processhacker/
.\build\build_init.cmd   # nur beim ersten Mal
.\build\build_release.cmd
```

**Lizenz:** MIT

---

### `builder` — Python-Buildskripte

**Pfad:** `external/henrypp/builder/`

**Inhalt:** Python-Skripte zum Erstellen von 7-Zip-Portables, NSIS-Installern und Lokalisierungsdateien.

**Anforderungen:**
```
Python 3.13+
7-Zip 24+   (im %PATH%)
GPG 2.5+    (im %PATH%)
NSIS 3.10+  (im %PATH%)
```

---

## GPG-Signierungsinfrastruktur

Alle Henry++-Binaries sind GPG-signiert:

| Attribut | Wert |
|----------|------|
| Key-ID | `0x5635B5FD` |
| Fingerprint | `D985 2361 1524 AB29 BE73 30AC 2881 20A7 5635 B5FD` |
| Public Key | [pubkey.asc](https://raw.githubusercontent.com/henrypp/builder/master/pubkey.asc) |
| Signatur-Datei | `<appname>.exe.sig` im App-Verzeichnis |

---

## Portable-Modus-Konvention

Alle Henry++-Apps folgen derselben Portable-Konvention:

```
# INI-Datei im App-Verzeichnis aktiviert Portable-Modus:
memreduct.ini     → Mem Reduct portabel
simplewall.ini    → simplewall portabel
```

Fehlt die `.ini`, werden Einstellungen unter `%APPDATA%\Henry++\<AppName>` gespeichert.

---

## Verlinkungen

- Klonschritte: [02-benutzer-anleitung.md](02-benutzer-anleitung.md)
- NT-API-Typen im Detail: [05-api-datenmodell.md](05-api-datenmodell.md)
- Architektur und Integration: [04-architektur.md](04-architektur.md)
