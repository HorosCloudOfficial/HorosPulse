# ⚡ powershell.instructions

> **Quelle:** `.github/.instructions/powershell.instructions.md`
> **Typ:** File Instructions (`applyTo: **/*.ps1,**/*.psm1`)
> **Status:** ✅ Sofort aktiv bei PowerShell-Dateien

## Kurzbeschreibung
Leitfaden für idiomatische, sichere und wartbare PowerShell-Skripte nach Microsoft-Cmdlet-Standards.

## Kernregeln
- Verb-Noun Benennung mit `Get-Verb`
- Keine Aliase in Skripten (`Get-ChildItem` statt `gci`)
- Klare Parameternamen und Typvalidierung
- Pipeline-fähig (Begin/Process/End)
- Objektausgabe statt Textformatierung

## Safety/Fehlerbehandlung
- `SupportsShouldProcess = $true`
- Angemessener `ConfirmImpact`
- `Write-Verbose/Warning/Error` korrekt nutzen
- In Advanced Functions: `$PSCmdlet.WriteError()` bevorzugen

## Aktivierung
Automatisch durch `applyTo` bei `.ps1/.psm1`.

## Parameter
Keine manuell zu setzenden Slash-Parameter; Regeln gelten kontextabhängig pro Datei.

## Eignung
Direkt relevant für Build-/Setup-Skripte im Repo (`build.ps1`, `setup-and-build.ps1`).



