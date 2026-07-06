# WindowsPerformance

![CI](../../actions/workflows/ci.yml/badge.svg)

WPF desktop application for optimizing Windows system settings for Cursor IDE development workflows.

## Stack

- **UI:** WPF, .NET 9, C# 13
- **MVVM:** CommunityToolkit.Mvvm 8.x
- **DI:** Microsoft.Extensions.Hosting
- **Theme:** Tokyo Night custom palette
- **Persistence:** Microsoft.Data.Sqlite (Sprint 2+)
- **Elevation:** Separate `WindowsPerformance.Elevation` console helper (UAC on-demand)

Built executable: **WindowsPerformance.Elevation.exe** (not ElevationHelper.exe; see [docs/architecture.md](docs/architecture.md#elevation-binary-namenskonvention)).

## Build & Run

```bash
dotnet build
dotnet run --project src/WindowsPerformance.App
dotnet test
```

Or double-click **`starter.bat`** (Debug by default; pass `Release` for a Release build/run):

```bat
starter.bat
starter.bat Release
```

## Portable Distribution

```powershell
.\publish.ps1                    # framework-dependent win-x64 ZIP
.\publish.ps1 -SelfContained     # self-contained win-x64 ZIP
```

Output: `artifacts/WindowsPerformance-<version>-win-x64.zip` (App + ElevationHelper + README).

## Elevation helper code signing (Dev)

For local development, sign `WindowsPerformance.Elevation.exe` with a self-signed Authenticode certificate:

```powershell
.\scripts\sign-elevation-helper.ps1              # Debug build
.\scripts\sign-elevation-helper.ps1 -Configuration Release
```

The script creates a `CN=WindowsPerformance Dev Code Signing` certificate in `CurrentUser\My` when none exists, then signs the binary with SHA-256. CI skips signing when `SKIP_SIGNING=1` (set automatically in GitHub Actions).

Production releases require a trusted code-signing certificate from a CA.

## Solution Structure

```
src/
  WindowsPerformance.App          WPF entry point
  WindowsPerformance.Core         Domain models & interfaces
  WindowsPerformance.ViewModels   MVVM view models
  WindowsPerformance.Services     Business logic & OS interaction
  WindowsPerformance.Data           SQLite & JSON persistence
  WindowsPerformance.Elevation    Elevation helper executable
  WindowsPerformance.PowerShell   pwsh.exe scripts (placeholder)
tests/
  WindowsPerformance.Tests.Unit
  WindowsPerformance.Tests.Integration
```

## Branching

Branch strategy: `main` (stable), `feature/<name>` for work, squash-merge into `main`.

See [docs/BRANCHING.md](docs/BRANCHING.md) for workflow and example commands.

## Requirements

- .NET 9 SDK
- Windows 10+
- PowerShell 7 (`pwsh.exe`) recommended for full functionality
