# Henry++ Referenz-Bibliothek — Übersicht

> **Diataxis: Erklärung** · Stand: 2026-07-06  
> Was die 19 Henry++-Repos sind, warum sie lokal geklont wurden und wie sie sich zu HorosPulse verhalten.

---

## Was ist die Henry++-Bibliothek?

**Henry++** (GitHub: [github.com/henrypp](https://github.com/henrypp)) ist ein einzelner Entwickler, der seit ca. 2011 eine Suite freier, quelloffener Windows-Systemprogramme in purem C entwickelt und pflegt. Seine Projekte nutzen ausschließlich die Windows-Native-API (undokumentierte NT-Interna, WFP, P/Invoke-Äquivalente in C), haben keine externen Abhängigkeiten und laufen auf Windows 7 SP1 bis Windows 11.

Alle 19 Repos teilen:
- **`routine`** als gemeinsame C-SDK-Bibliothek (als Git-Submodul eingebunden)
- Ein einheitliches Buildverfahren via `builder`-Python-Skripte + NSIS-Installer
- GPG-signierte Binaries (Key-ID `0x5635B5FD`)
- Portable-Modus via `.ini`-Datei im App-Verzeichnis

---

## Warum lokal geklont?

HorosPulse (WPF .NET 9) plant in **Phase 2** mehrere Module, die dieselben undokumentierten NT-API-Aufrufe benötigen wie Henry++-Projekte:

| TODO-Item | Geplantes Modul | Relevantes Henry++-Repo |
|-----------|----------------|------------------------|
| §5.4 | Memory Optimizer (`IMemoryOptimizerService`) | `memreduct` |
| §5.5 | Network Optimizer (Nagle-Algorithmus, TCP-Parameter) | `simplewall` |
| §5.3 | Visual Effects (`SystemParametersInfo` P/Invoke) | `routine` |
| §5.8 | Process Inspector (Handle-Count, Working Set) | `processhacker` |

Der Klon dient ausschließlich als **Lese-Referenz**: Wie ruft man `NtSetSystemInformation` korrekt auf? Welche Privilegien sind nötig? Wie geht man mit `NT_SUCCESS`-Prüfung und Fehlerlogging um?

---

## Abgrenzung: Referenz ≠ Integration

| Merkmal | Henry++-Repos | HorosPulse |
|---------|-------------|---------------------|
| Sprache | C (Win32 Native API) | C# / .NET 9 / WPF |
| API-Ebene | Direkte NT-API (`ntdll.dll`) | P/Invoke via `Vanara.PInvoke.NtDll` (NuGet) |
| Build | MSVC + NSIS | `dotnet build` / MSBuild |
| Integration | Nicht in `HorosPulse.sln` enthalten | Kein `external/` Verweis in Projektdateien |
| .gitignore | `external/henrypp/` ist ausgeschlossen | Repos werden nie committed |

HorosPulse **vendort keinen C-Code** aus diesen Repos. Die Interop-Implementierung erfolgt stattdessen über das NuGet-Paket `Vanara.PInvoke.NtDll`, das dieselben API-Signaturen typsicher als .NET-Bindings bereitstellt.

> Vergleich: Warum NuGet statt C-Code? → [planning-todo-scan-externe-repos: §5.4-Empfehlung](../planning-todo-scan-externe-repos/05-api-datenmodell.md)

---

## Klonzeitpunkt und Zustand

Die Repos sind **Shallow Clones** (kein vollständiger Commit-Verlauf). Sie enthalten:
- Aktuelle Quellcode-Snapshots (~2 734 Dateien über alle 19 Repos)
- Keine Build-Artefakte (`bin/`, `obj/` sind nicht committierbar)
- Keine `.user`-Dateien

Der Pfad `external/henrypp/` steht in `.gitignore` (Zeile 84):

```
external/henrypp/
```

Das bedeutet: Nach einem frischen `git clone` von `HorosPulse` müssen die Henry++-Repos separat geklont werden (→ [02-benutzer-anleitung.md](02-benutzer-anleitung.md)).

---

## Die vier Schlüssel-Repos

### `memreduct` — Memory Optimizer Referenz

Leichtgewichtige Echtzeit-Speicherverwaltung. Nutzt `NtSetSystemInformation` mit `SystemMemoryListInformation` um Working Sets, Standby-Listen und Modified Page Lists zu leeren. Die Implementierung in `external/henrypp/memreduct/src/main.c` ist die direkteste Referenz für TODO §5.4.

### `routine` — Gemeinsames C-SDK

Shared Library für alle Henry++-Projekte. Enthält in `src/ntapi.h` die vollständigen Typ-Definitionen für undokumentierte NT-Strukturen — darunter `SYSTEM_MEMORY_LIST_COMMAND`, `SYSTEM_FILECACHE_INFORMATION` und hunderte weitere NT-Typen. Wird als Git-Submodul in alle anderen Repos eingebunden.

### `simplewall` — WFP-Referenz für Network Optimizer

Windows Filtering Platform (WFP) basierte Firewall/Netzwerkfilter-App. Referenz für TODO §5.5 (Network Optimizer): wie WFP-Filter programmatisch installiert, aktiviert und entfernt werden. Nutzt ebenfalls `routine` als Submodul.

### `processhacker` — System Informer (Process Inspector Referenz)

Früher bekannt als Process Hacker, jetzt „System Informer" (gehostet bei [winsiderss/systeminformer](https://github.com/winsiderss/systeminformer)). Referenz für TODO §5.8: detaillierte Prozessinformationen (Handle-Count, Thread-Count, IO-Bytes, Working Set) via NT-API.

---

## Die übrigen 15 Repos im Schnellüberblick

| Repo | Beschreibung | Relevanz für HorosPulse |
|------|-------------|--------------------------------|
| `autoruns2` | Autorun-Manager (Registry + Startup-Ordner) | Referenz §5.2 Startup-Programme |
| `builder` | Python-Buildskripte (7-Zip, GPG, NSIS) | Build-Muster; kein direkter Code-Bezug |
| `chrlauncher` | Chrome-/Chromium-basierter Mini-Launcher | Kein direkter Bezug |
| `dbgvision` | Debug-Visualizer für Windows-Typen | Debugging-Hilfsmittel |
| `drivedotshield` | Laufwerkschutz-Tool | Kein direkter Bezug |
| `errorlookup` | Windows-Fehlercode-Nachschlagwerk | Kein direkter Bezug |
| `freeshooter` | Screenshot-Tool | Kein direkter Bezug |
| `henrypp` | Persönliche GitHub-Profil-Seite | Kein Code |
| `henrypp.org` | Website-Quellcode | Kein Code-Bezug |
| `hostsmgr` | Hosts-Datei-Manager | Kein direkter Bezug |
| `inetops` | Internet-Operationen-Tool | Peripherer Bezug §5.5 |
| `iplookup` | IP-Lookup-Tool | Kein direkter Bezug |
| `matrix` | Matrix-Regen-Screensaver | Kein direkter Bezug |
| `timevertor` | Zeitkonvertierungs-Utility | Kein direkter Bezug |
| `uninstmgr` | Uninstall-Manager | Kein direkter Bezug |

---

## Verlinkungen

- Repo-Inventar mit Pfaden: [03-einstellungen.md](03-einstellungen.md)
- API-Mapping TODO.md → NT-Calls: [05-api-datenmodell.md](05-api-datenmodell.md)
- Klonschritte Schritt für Schritt: [02-benutzer-anleitung.md](02-benutzer-anleitung.md)
- NuGet vs. C-Code Entscheidung: [planning-todo-scan-externe-repos/README.md](../planning-todo-scan-externe-repos/README.md)
