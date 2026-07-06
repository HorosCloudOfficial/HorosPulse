# Henry++ Referenz-Bibliothek — FAQ & Troubleshooting

> **Diataxis: How-to** · Stand: 2026-07-06  
> Häufige Fragen, Klonprobleme, Missverständnisse über den Verwendungszweck und Hinweise zum Umgang mit der Bibliothek.

---

## Klonen & Einrichtung

### F: Nach `git clone` des WindowsPerformance-Repos fehlt `external/henrypp/` komplett. Warum?

`external/henrypp/` ist in `.gitignore` (Zeile 84) ausgeschlossen. Das ist beabsichtigt — die Repos sind nicht Teil des WindowsPerformance-Commits und müssen separat geklont werden.

**Lösung:** Klonschritte in [02-benutzer-anleitung.md](02-benutzer-anleitung.md) befolgen.

---

### F: `git submodule update --init` in `memreduct` schlägt fehl. Was tun?

Das passiert, wenn `memreduct` ohne `--recurse-submodules` geklont wurde und `routine` fehlt.

```powershell
Set-Location "D:\WindowsPerformance\external\henrypp\memreduct"
git submodule update --init --recursive
```

Wenn das Netzwerk das Submodul nicht auflöst, kann `routine` auch direkt geklont und verlinkt werden:

```powershell
Set-Location "D:\WindowsPerformance\external\henrypp"
git clone --depth=1 https://github.com/henrypp/routine.git

# Manuell in memreduct verlinken:
Set-Location "D:\WindowsPerformance\external\henrypp\memreduct"
# .gitmodules zeigt den erwarteten Pfad; routine-Ordner muss dort liegen
```

---

### F: Soll ich die Henry++-Repos mit `git pull` aktuell halten?

Das liegt im Ermessen des Entwicklers. Da es sich um Leserefrenz handelt, sind regelmäßige Updates nicht zwingend. Empfehlung: einmal klonen, bei Bedarf aktualisieren wenn eine spezifische neue Funktion untersucht werden soll.

```powershell
# Update einzelner Repos:
Set-Location "D:\WindowsPerformance\external\henrypp\memreduct"
git pull origin master
```

---

## Integration & Verwendungszweck

### F: Warum wird der C-Code nicht direkt in WindowsPerformance eingebunden?

WindowsPerformance ist ein WPF .NET 9 Projekt. Henry++-Repos sind C/MSVC-Projekte. Eine direkte Einbindung würde bedeuten:

1. MSVC als zusätzliche Build-Abhängigkeit
2. Komplexe C-Interop via P/Invoke auf eigene DLLs
3. Zusätzliche Deployment-Artefakte (native DLLs)

`Vanara.PInvoke.NtDll` (NuGet) bietet dieselben NT-API-Bindungen typsicher in .NET — ohne diese Komplexität.

> Referenz: [planning-todo-scan-externe-repos/README.md](../planning-todo-scan-externe-repos/README.md) → §5.4 `Vanara.PInvoke.NtDll` Empfehlung

---

### F: Kann `memreduct.exe` oder `simplewall.exe` als Child-Process aus WindowsPerformance gestartet werden?

Technisch möglich, aber bewusst **nicht** geplant:

- Keine Kontrolle über UI oder Verhalten des externen Prozesses
- Keine programmatische Schnittstelle (keine API, nur UI)
- Benutzer würde plötzlich ein fremdes Fenster sehen
- UAC-Prompt käme vom externen Prozess, nicht von WindowsPerformance

WindowsPerformance implementiert die Funktionalität selbst (via Vanara/ElevationHelper).

---

### F: Sind die Henry++-Repos in `WindowsPerformance.sln` eingetragen?

Nein. Die Solution-Datei enthält ausschließlich die sieben .NET-Projekte unter `src/` und `tests/`. `external/henrypp/` hat keinen Eintrag in der Solution.

---

### F: Darf ich Code aus Henry++-Repos in WindowsPerformance kopieren?

Die Repos haben unterschiedliche Lizenzen:

| Repo | Lizenz | Für WindowsPerformance nutzbar? |
|------|--------|--------------------------------|
| `memreduct` | MIT | Ja, mit Namensnennung |
| `routine` | MIT | Ja, mit Namensnennung |
| `simplewall` | GPL-3.0 | **Nein** — GPL-Code würde das Gesamtprojekt der GPL unterwerfen |
| `processhacker` | MIT | Ja, mit Namensnennung |
| `autoruns2` | MIT | Ja, mit Namensnennung |

**Empfehlung:** Keine direkte Code-Kopie. Algorithmen und Patterns als Inspiration nutzen, eigene .NET-Implementierung schreiben.

---

## `NtSetSystemInformation` — Fehlerbilder

### F: `NtSetSystemInformation` gibt `STATUS_PRIVILEGE_NOT_HELD` zurück. Was fehlt?

Das Privilege `SeProfileSingleProcessPrivilege` ist nicht aktiviert. In WindowsPerformance bedeutet das, der Aufruf wurde nicht über `ElevationHelper.exe` gesendet.

**Lösung:** Sicherstellen, dass der Aufruf über `ElevationService → ElevationHelper.exe` geht. Der ElevationHelper hat `requireAdministrator` im Manifest und kann das Privilege selbst erteilen.

---

### F: `MemoryPurgeStandbyList` wird ohne Fehler ausgeführt, aber RAM-Auslastung sinkt kaum. Warum?

Das ist normales Verhalten. Der Effekt hängt vom aktuellen Systemzustand ab:

- Ist die Standby-Liste bereits klein, gibt es wenig freizugeben
- Nach intensiver RAM-Nutzung (Browser, Node.js, VS Code) ist der Effekt deutlicher (10–50% der belegten Seiten)
- Der Effekt ist temporär: Windows füllt die Standby-Liste für zukünftige Prozesse wieder auf

`memreduct` (das Original) meldet selbst: *"variable result ~10-50%"*

---

### F: `SystemRegistryReconciliationInformation` schlägt auf Windows 10 mit `STATUS_NOT_IMPLEMENTED` fehl. Warum?

Dieser Aufruf ist nur auf Windows 8.1+ verfügbar und in manchen Windows-10-Versionen nicht implementiert. memreduct prüft dies explizit:

```c
if (_r_sys_isosversiongreaterorequal (WINDOWS_8_1))
{
    // Registry-Cache-Flush
}
```

In WindowsPerformance entsprechend mit `Environment.OSVersion.Version`-Check absichern.

---

## WFP (simplewall)

### F: Blockiert `simplewall` alle Netzwerkverbindungen wenn es beendet wird?

Ja — installierte WFP-Filter bleiben aktiv auch wenn `simplewall.exe` beendet wird. Das ist eine Eigenschaft von WFP: Filter werden beim Kernel registriert, nicht im Prozess gehalten.

Zum Entfernen: `simplewall.exe --uninstall` oder "Filter deaktivieren"-Button in der UI.

**Relevanz für WindowsPerformance:** Wenn TODO §5.5 (Network Optimizer) WFP-Filter nutzen sollte (Phase 3+), müssen diese beim App-Deinstall explizit entfernt werden.

---

### F: Warum ist `simplewall` GPL-3.0, aber `routine` (das es einbindet) MIT?

Das ist eine gängige Kombination. `routine` als SDK-Bibliothek ist MIT-lizenziert. `simplewall` als eigenständiges Programm wählte GPL-3.0. Das ändert nichts an der MIT-Lizenz von `routine` selbst.

---

## Build-Probleme

### F: MSVC-Build von memreduct schlägt mit "Cannot open include file: 'ntdll.h'" fehl.

`routine` muss als Submodul initialisiert sein. Außerdem benötigen die Repos das Windows SDK in der im `.vcxproj` konfigurierten Version.

```powershell
Set-Location "D:\WindowsPerformance\external\henrypp\memreduct"
git submodule update --init --recursive
```

---

### F: Ich will die Henry++-Repos nur lesen, nicht bauen. Muss ich Python/NSIS installieren?

Nein. Zum Lesen des Quellcodes genügt ein Texteditor oder Visual Studio Code. Python, 7-Zip, GPG und NSIS werden nur benötigt um tatsächliche Binaries/Installer zu erzeugen — was für WindowsPerformance-Entwicklung nicht nötig ist.

---

## Verlinkungen

- Klonschritte: [02-benutzer-anleitung.md](02-benutzer-anleitung.md)
- Übersicht und Abgrenzung: [01-uebersicht.md](01-uebersicht.md)
- NT-API-Details: [05-api-datenmodell.md](05-api-datenmodell.md)
- NuGet-Empfehlungen gesamt: [planning-todo-scan-externe-repos/06-faq-troubleshooting.md](../planning-todo-scan-externe-repos/06-faq-troubleshooting.md)
