# Henry++ Referenz-Bibliothek — Entwickler-Anleitung

> **Diataxis: How-to** · Stand: 2026-07-06  
> Wie man die 19 Henry++-Repos klont, lokal einrichtet und als Code-Referenz für HorosPulse nutzt. Zielgruppe: Entwickler des HorosPulse-Projekts.

---

## Voraussetzungen

| Werkzeug | Mindestversion | Zweck |
|----------|---------------|-------|
| `git` | beliebig | Shallow-Clone der Repos |
| Visual Studio Code / VS | optional | Quellcode lesen (kein Build nötig) |
| Python 3.13+ | nur bei Build | `builder`-Skripte ausführen |
| MSVC (Visual Studio C++) | nur bei Build | Henry++-Repos kompilieren |

> HorosPulse selbst wird mit `dotnet build` gebaut — kein MSVC nötig.

---

## Schritt 1: Verzeichnis anlegen

Das `external/henrypp/`-Verzeichnis existiert bereits im Workspace. Wenn es fehlt:

```powershell
New-Item -ItemType Directory -Path "D:\HorosPulse\external\henrypp" -Force
```

---

## Schritt 2: Repos klonen

### Alle vier Schlüssel-Repos (empfohlen)

```powershell
Set-Location "D:\HorosPulse\external\henrypp"

# Shared SDK (wird als Submodul von memreduct/simplewall benötigt)
git clone --depth=1 https://github.com/henrypp/routine.git

# Memory Optimizer Referenz (TODO §5.4)
git clone --depth=1 --recurse-submodules https://github.com/henrypp/memreduct.git

# WFP / Network Optimizer Referenz (TODO §5.5)
git clone --depth=1 --recurse-submodules https://github.com/henrypp/simplewall.git

# Process Inspector Referenz (TODO §5.8)
git clone --depth=1 https://github.com/winsiderss/systeminformer.git processhacker

# Build-Skripte
git clone --depth=1 https://github.com/henrypp/builder.git
```

> `--depth=1` ergibt einen Shallow Clone ohne vollständige Commit-Historie. Das spart Disk-Space und Bandbreite — für Lesezwecke ausreichend.

### Alle 19 Repos (vollständig)

Falls alle Repos geklont werden sollen, kann das folgende Skript genutzt werden:

```powershell
$repos = @(
    "autoruns2", "builder", "chrlauncher", "dbgvision",
    "drivedotshield", "errorlookup", "freeshooter",
    "hostsmgr", "inetops", "iplookup", "matrix",
    "memreduct", "routine", "simplewall",
    "timevertor", "uninstmgr"
)

Set-Location "D:\HorosPulse\external\henrypp"

foreach ($repo in $repos) {
    git clone --depth=1 "https://github.com/henrypp/$repo.git"
}

# processhacker ist bei winsiderss gehostet
git clone --depth=1 https://github.com/winsiderss/systeminformer.git processhacker
```

---

## Schritt 3: Submodule initialisieren (wenn nötig)

`memreduct` und `simplewall` binden `routine` als Submodul ein. Wurde mit `--recurse-submodules` geklont, ist dies bereits erledigt. Sonst:

```powershell
Set-Location "D:\HorosPulse\external\henrypp\memreduct"
git submodule update --init --recursive
```

---

## Schritt 4: Quellcode navigieren

### Wichtigste Dateipfade für die Phase-2-Implementierung

| Datei | Relevanz |
|-------|---------|
| `external/henrypp/memreduct/src/main.c` | `NtSetSystemInformation`-Aufrufe für Memory-Flush (ab Zeile 370) |
| `external/henrypp/routine/src/ntapi.h` | Vollständige NT-API-Typdefinitionen (`SYSTEM_MEMORY_LIST_COMMAND`, `NtSetSystemInformation`, etc.) |
| `external/henrypp/routine/src/routine.h` | Hauptheader der SDK-Bibliothek |
| `external/henrypp/simplewall/src/` | WFP-Filterinstallation und -verwaltung |
| `external/henrypp/processhacker/SystemInformer/` | Prozess-API-Aufrufe (Handle-Count, Working Set) |

### Zum Memory-Flush navigieren

Für TODO §5.4 (`IMemoryOptimizerService`) ist folgende Code-Stelle der zentrale Referenzpunkt:

```
external/henrypp/memreduct/src/main.c  — ab Zeile 370
```

Dort werden nacheinander aufgerufen:
1. `MemoryEmptyWorkingSets` — Working Sets aller Prozesse leeren
2. `SystemFileCacheInformationEx` — System File Cache leeren
3. `MemoryFlushModifiedList` — Modified Page List leeren
4. `MemoryPurgeStandbyList` — Standby-Liste leeren
5. `MemoryPurgeLowPriorityStandbyList` — Standby Priority-0-Liste leeren
6. `SystemRegistryReconciliationInformation` — Registry-Cache (nur Win 8.1+)
7. `SystemCombinePhysicalMemoryInformation` — Speicherseiten kombinieren (nur Win 10+)

---

## Schritt 5: Als Referenz im Code nutzen

Die C-Implementierung in `memreduct` dient als **Blaupause**, nicht als eingebundener Code. Die entsprechende .NET-Implementierung in HorosPulse nutzt `Vanara.PInvoke.NtDll`:

```csharp
// Analoges .NET-Pattern zu memreduct/src/main.c Zeile 419–425
// Benötigt: SeProfileSingleProcessPrivilege
using Vanara.PInvoke;

var command = NtDll.SYSTEM_MEMORY_LIST_COMMAND.MemoryPurgeStandbyList;
var status = NtDll.NtSetSystemInformation(
    NtDll.SYSTEM_INFORMATION_CLASS.SystemMemoryListInformation,
    command,
    sizeof(NtDll.SYSTEM_MEMORY_LIST_COMMAND));

if (status.IsSuccess)
    // Standby-Liste erfolgreich geleert
```

> Der genaue API-Aufruf hängt von der Vanara-Version ab — die Typisierung in `routine/src/ntapi.h` ist die Quelldefinition.

---

## Schritt 6: Repos aktualisieren

Da die Repos gitignored sind, werden sie nicht automatisch mit `git pull` auf dem HorosPulse-Repo aktualisiert. Manuelles Update:

```powershell
Set-Location "D:\HorosPulse\external\henrypp\memreduct"
git pull origin master

Set-Location "D:\HorosPulse\external\henrypp\routine"
git pull origin master
```

---

## Henry++-Repos bauen (optional, selten nötig)

Das Bauen der C-Projekte ist für Lesezwecke nicht erforderlich. Falls dennoch benötigt:

**Voraussetzungen für den Build:**
- Visual Studio 2022 (C++-Workload)
- Python 3.13+
- 7-Zip 24+ im `%PATH%`
- GPG 2.5+ im `%PATH%`
- NSIS 3.10+ im `%PATH%`

```powershell
# Beispiel: memreduct bauen
Set-Location "D:\HorosPulse\external\henrypp\memreduct"
# Visual Studio Solution öffnen oder:
msbuild memreduct.sln /p:Configuration=Release /p:Platform=x64
```

> HorosPulse-CI baut die Henry++-Repos **nicht**. Diese Schritte sind rein informativ.

---

## Verlinkungen

- Repo-Inventar mit Pfaden: [03-einstellungen.md](03-einstellungen.md)
- NT-API-Mapping zu TODO-Items: [05-api-datenmodell.md](05-api-datenmodell.md)
- Übersicht und Abgrenzung: [01-uebersicht.md](01-uebersicht.md)
- NuGet-Empfehlung `Vanara.PInvoke.NtDll`: [planning-todo-scan-externe-repos/05-api-datenmodell.md](../planning-todo-scan-externe-repos/05-api-datenmodell.md)
