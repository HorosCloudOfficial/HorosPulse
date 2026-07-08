# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-07-08

### Added

- **Speicher** (Sidebar): Volle Mem-Reduct-Purge-UI mit allen `MemoryPurgeOptions`-Bereichen (Working Set, System File Cache, Standby, Registry Cache, etc.), Apply-Button und Speicherstatus-Anzeige
- **WSL2 / Docker** (Sidebar): `.wslconfig`-Advisor/Tuner — WSL2/Docker-Erkennung, Ressourcen-Limits vs. System, Dev-Empfehlungen, Apply mit Backup/Rollback, `wsl --shutdown`, Verweis auf Build-Schutz
- **Coding-Boost** (Sidebar): Game Mode, HAGS und Fenster-Optimierung (Win11) für Dev-/GPU-Workflows — Apply mit Tracking + Rollback, `IOptimizationModule`-Integration
- **Dev Drive Advisor** (Sidebar): ReFS/Dev-Drive-Erkennung, Prüfung typischer Dev-Pfade (source, npm, NuGet, pnpm, Cargo, Cursor/VS Code, Temp), deutsche Migrations-Empfehlungen ohne Auto-Moves, Dashboard-Status, Unit-Tests
- **Dev-Cache** (Sidebar): Sicheres Aufräumen von Dev-Temp-Caches (npm, TEMP, NuGet HTTP-Cache, pnpm, Cargo) mit Größenanzeige, Sicherheitsklassifikation und Bestätigungsdialog; `IOptimizationModule`-Preset für sichere Caches
- Kompakt-Fenster (Mem-Reduct-Style): Live RAM/CPU/Disk-Stats, Schnellaktionen, Tray-Eintrag, Einstellungen-Tab
- `IElevationUiInvoker` / `ElevationUiInvoker`: UAC-Helper-Start auf UI-Thread; `SyncElevationUiInvoker` für Headless-Tests
- Screenshot-Skripte `SCRIPTS/capture-horospulse-*.ps1` inkl. optionalem Kompakt-Fenster-Capture

### Changed

- Rebrand cleanup: removed stale `WindowsPerformance.*` tree; migrated PowerShell module to `HorosPulse.PowerShell`
- Light theme (`Resources/Themes/Light.xaml`) with working Theme ComboBox (Dark / Tokyo Night / Light)
- Velopack `UpdateManager` wired for non-blocking startup check and manual update in Einstellungen
- `publish.ps1` supports `-Velopack` for `vpk pack` output under `artifacts/velopack/`
- User-facing elevation messages use `HorosPulse.Elevation.exe`; CHANGELOG URLs point to HorosCloudOfficial/HorosPulse

### Added (prior)

- MVP P2: Fenster-Geometrie-Persistenz, Light-Mode-Skeleton, Ultimate Performance Plan
- MVP P2: Monitor-Refresh/Cursor-Filter, Defender-Pfadvalidierung, Indexer WSearch-Restart (Opt-in)
- MVP P2: DB-Backup vor Schema-Init, Audit-CSV-Export, IObservable-Metriken-Wrapper
- MVP P2: Preset JSON Import/Export, ViewModel- und Integrations-Tests
- Phase 3: Disk Optimizer, Task Scheduler Dev-Schutz, ML-Anomalien + Empfehlungen
- Phase 3: Velopack-Installer-Skeleton, Registry Tuner (opt-in, dokumentierte Tweaks)
- Dokumentation: `docs/architecture.md`, `docs/presets.md`

### Changed (prior)

- `publish.ps1` erweitert um Velopack-Hinweise und Installer-Dokumentation

## [0.1.0] - 2026-07-06

### Added

- Initiale HorosPulse-App mit Tokyo Night UI
- Cursor Dev Mode Preset, Snapshot/Rollback, HorosPulse.Elevation.exe
- Phase-2-Module: Dienste, Startup, Visuell, Speicher, Netzwerk, Trends, Prozesse

[Unreleased]: https://github.com/HorosCloudOfficial/HorosPulse/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/HorosCloudOfficial/HorosPulse/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/HorosCloudOfficial/HorosPulse/releases/tag/v0.1.0
