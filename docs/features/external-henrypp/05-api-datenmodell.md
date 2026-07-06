# Henry++ Referenz-Bibliothek — Native APIs & Datenmodell

> **Diataxis: Referenz** · Stand: 2026-07-06  
> Vollständige Abbildung der NT-API-Aufrufe aus Henry++-Repos auf die geplanten WindowsPerformance-Module (TODO.md Phase 2). Typdefinitionen aus `external/henrypp/routine/src/ntapi.h`.

---

## TODO §5.4 — Memory Optimizer (`IMemoryOptimizerService`)

**Henry++-Referenz:** `external/henrypp/memreduct/src/main.c` (ab Zeile 370)

### `SYSTEM_MEMORY_LIST_COMMAND` — Enum

Definiert in `external/henrypp/routine/src/ntapi.h` (Zeilen 1027–1036):

```c
typedef enum _SYSTEM_MEMORY_LIST_COMMAND
{
    MemoryCaptureAccessedBits,          // 0
    MemoryCaptureAndResetAccessedBits,  // 1
    MemoryEmptyWorkingSets,             // 2 — Working Sets aller Prozesse leeren
    MemoryFlushModifiedList,            // 3 — Modified Page List leeren
    MemoryPurgeStandbyList,             // 4 — Standby-Liste leeren
    MemoryPurgeLowPriorityStandbyList,  // 5 — Standby Priority-0-Liste leeren
    MemoryCommandMax
} SYSTEM_MEMORY_LIST_COMMAND;
```

### `NtSetSystemInformation` — Signatur

```c
// ntapi.h Zeile 5687
NTSYSCALLAPI
NTSTATUS
NTAPI
NtSetSystemInformation(
    _In_ SYSTEM_INFORMATION_CLASS SystemInformationClass,
    _In_reads_bytes_(SystemInformationLength) PVOID SystemInformation,
    _In_ ULONG SystemInformationLength
);
```

### Alle Memory-Flush-Operationen aus `memreduct/src/main.c`

| Schritt | Funktion / Aufruf | Information Class | Privilege |
|---------|-------------------|------------------|-----------|
| 1 | Working Sets leeren | `SystemMemoryListInformation` + `MemoryEmptyWorkingSets` | `SeProfileSingleProcessPrivilege` |
| 2 | System File Cache | `SystemFileCacheInformationEx` + `SYSTEM_FILECACHE_INFORMATION` | `SeIncreaseQuotaPrivilege` |
| 3 | Modified Page List | `SystemMemoryListInformation` + `MemoryFlushModifiedList` | `SeProfileSingleProcessPrivilege` |
| 4 | Standby-Liste | `SystemMemoryListInformation` + `MemoryPurgeStandbyList` | `SeProfileSingleProcessPrivilege` |
| 5 | Standby Priority-0 | `SystemMemoryListInformation` + `MemoryPurgeLowPriorityStandbyList` | `SeProfileSingleProcessPrivilege` |
| 6 | Registry-Cache (Win 8.1+) | `SystemRegistryReconciliationInformation` | Admin |
| 7 | Speicher kombinieren (Win 10+) | `SystemCombinePhysicalMemoryInformation` | Admin |

### `SYSTEM_FILECACHE_INFORMATION` — Struktur

```c
// Aus routine/src/ntapi.h
typedef struct _SYSTEM_FILECACHE_INFORMATION
{
    SIZE_T CurrentSize;
    SIZE_T PeakSize;
    ULONG PageFaultCount;
    SIZE_T MinimumWorkingSet;  // MAXSIZE_T zum Leeren
    SIZE_T MaximumWorkingSet;  // MAXSIZE_T zum Leeren
    SIZE_T CurrentSizeIncludingTransitionInPages;
    SIZE_T PeakSizeIncludingTransitionInPages;
    ULONG TransitionRePurposeCount;
    ULONG Flags;
} SYSTEM_FILECACHE_INFORMATION, *PSYSTEM_FILECACHE_INFORMATION;
```

Zum Leeren: `MinimumWorkingSet = MAXSIZE_T; MaximumWorkingSet = MAXSIZE_T;`

### Übersetzung nach C# (Vanara.PInvoke.NtDll)

```csharp
// Entspricht memreduct/src/main.c Zeilen 419–425
// Voraussetzung: wird via WindowsPerformance.Elevation.exe ausgeführt

using Vanara.PInvoke;

// Standby-Liste leeren
var command = NtDll.SYSTEM_MEMORY_LIST_COMMAND.MemoryPurgeStandbyList;
var status = NtDll.NtSetSystemInformation(
    NtDll.SYSTEM_INFORMATION_CLASS.SystemMemoryListInformation,
    command,
    (uint)Marshal.SizeOf<NtDll.SYSTEM_MEMORY_LIST_COMMAND>());

if (!status.Succeeded)
    logger.LogError("MemoryPurgeStandbyList fehlgeschlagen: {Status}", status);
```

### `MEMORY_COMBINE_INFORMATION_EX` — Struktur (Win 10+)

```c
// Aus routine/src/ntapi.h
typedef struct _MEMORY_COMBINE_INFORMATION_EX
{
    HANDLE Section;
    ULONG_PTR PagesCombined;
    ULONG Flags;
} MEMORY_COMBINE_INFORMATION_EX, *PMEMORY_COMBINE_INFORMATION_EX;
```

---

## TODO §5.5 — Network Optimizer (`INetworkOptimizerService`)

**Henry++-Referenz:** `external/henrypp/simplewall/src/`

### Nagle-Algorithmus deaktivieren (TCP NoDelay)

WindowsPerformance plant Registry-basierte Konfiguration. simplewall ist hierfür nur periphere Referenz; die Hauptreferenz ist die Windows-Dokumentation:

```
HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{GUID}\
  TcpAckFrequency = 1    (DWORD) — TCP ACK sofort senden
  TCPNoDelay      = 1    (DWORD) — Nagle-Algorithmus deaktivieren
```

### WFP — Windows Filtering Platform (Referenz: simplewall)

simplewall zeigt den korrekten Lebenszyklus für WFP-Filter. **Hinweis:** TODO §5.5 plant keinen eigenen WFP-Filter — die Netzwerk-Optimierung beschränkt sich auf TCP-Parameter und DNS-Cache-TTL. simplewall bleibt als Referenz für eventuelle Phase-3-Erweiterungen.

| WFP-API | Verwendung in simplewall | Relevanz für §5.5 |
|---------|-------------------------|-------------------|
| `FwpmEngineOpen0` | Engine-Handle öffnen | Referenz, nicht direkt genutzt |
| `FwpmFilterAdd0` | Filter hinzufügen | Referenz für Phase 3+ |
| `FwpmFilterDeleteByKey0` | Filter entfernen | Referenz für Phase 3+ |
| `FwpmEngineClose0` | Engine-Handle schließen | Referenz für Phase 3+ |

---

## TODO §5.2 — Startup-Manager (`IStartupManagerService`)

**Henry++-Referenz:** `external/henrypp/autoruns2/`

### Registry-Pfade für Autostart-Einträge

```
HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run      — Benutzer-Autostart
HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run      — System-Autostart (Elevation nötig)
HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run  — 32-Bit-Einträge
```

### Shell-Startup-Ordner

```
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup   — Benutzer
%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Startup — Alle Benutzer
```

---

## TODO §5.3 — Visual Effects (`IVisualEffectsService`)

**Henry++-Referenz:** `external/henrypp/routine/src/ntapi.h` (SystemParametersInfo-Typen)

### Relevante Registry-Schlüssel

```
HKCU\Control Panel\Desktop\UserPreferencesMask   — Animations-Flags (BINARY)
HKCU\Control Panel\Desktop\MinAnimate            — Minimieren/Maximieren animieren
HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects\VisualFXSetting
```

### `SystemParametersInfo` P/Invoke

```csharp
// Preset: Alle Effekte deaktivieren
SystemParametersInfo(
    SPI_SETANIMATION,    // 0x0049
    sizeof(ANIMATIONINFO),
    ref animInfo,
    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
```

---

## TODO §5.8 — Process Inspector (`IProcessInspectorService`)

**Henry++-Referenz:** `external/henrypp/processhacker/`

### NT-Typen für Prozess-Details

```c
// Aus routine/src/ntapi.h — NtQuerySystemInformation mit:
SystemProcessInformation        // 5  — Prozessliste mit Basis-Infos
SystemHandleInformation         // 16 — Handle-Liste aller Prozesse
SystemExtendedHandleInformation // 64 — Erweiterte Handle-Infos (Vista+)
```

### `SYSTEM_PROCESS_INFORMATION` — Schlüsselfelder

```c
typedef struct _SYSTEM_PROCESS_INFORMATION
{
    ULONG NextEntryOffset;
    ULONG NumberOfThreads;
    // ... Timing-Felder ...
    UNICODE_STRING ImageName;
    KPRIORITY BasePriority;
    HANDLE UniqueProcessId;
    HANDLE InheritedFromUniqueProcessId;
    ULONG HandleCount;
    ULONG SessionId;
    // ... Working Set Felder ...
    SIZE_T WorkingSetSize;
    SIZE_T PeakWorkingSetSize;
    // ... I/O-Counter-Felder ...
} SYSTEM_PROCESS_INFORMATION, *PSYSTEM_PROCESS_INFORMATION;
```

### .NET-Alternative (ohne phnt)

WindowsPerformance nutzt für Phase 2 primär `System.Diagnostics.Process` und `System.Diagnostics.PerformanceCounter`. Die phnt-basierten NT-Aufrufe sind nur nötig, wenn Handle-Count und Thread-Count aus NT-Sicht benötigt werden:

```csharp
// Über System.Diagnostics (kein P/Invoke nötig für Basis-Infos)
var process = Process.GetProcessById(pid);
var handleCount = process.HandleCount;
var workingSetMB = process.WorkingSet64 / 1024 / 1024;
var threadCount = process.Threads.Count;
```

---

## Privilege-Übersicht

| Privilege | Benötigt für | Wie erhalten |
|-----------|-------------|-------------|
| `SeProfileSingleProcessPrivilege` | `NtSetSystemInformation` Memory-Flush | `WindowsPerformance.Elevation.exe` (UAC on-demand) |
| `SeIncreaseQuotaPrivilege` | `SystemFileCacheInformationEx` | `WindowsPerformance.Elevation.exe` |
| `SeDebugPrivilege` | Prozess-Handles anderer Benutzer | Nur für Process Inspector (optional) |
| Administrator-Rechte | Registry HKLM-Schreibzugriff | `WindowsPerformance.Elevation.exe` |

---

## Verlinkungen

- Architektur und Datenfluss: [04-architektur.md](04-architektur.md)
- Klonschritte für die Repos: [02-benutzer-anleitung.md](02-benutzer-anleitung.md)
- NuGet-Paket `Vanara.PInvoke.NtDll`: [planning-todo-scan-externe-repos/05-api-datenmodell.md](../planning-todo-scan-externe-repos/05-api-datenmodell.md)
- Master-TODO Phase 2: [`TODO.md §5.x`](../../TODO.md)
