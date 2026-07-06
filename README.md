# WindowsPerformance

WPF desktop application for optimizing Windows system settings for Cursor IDE development workflows.

## Stack

- **UI:** WPF, .NET 9, C# 13
- **MVVM:** CommunityToolkit.Mvvm 8.x
- **DI:** Microsoft.Extensions.Hosting
- **Theme:** Tokyo Night custom palette
- **Persistence:** Microsoft.Data.Sqlite (Sprint 2+)
- **Elevation:** Separate `WindowsPerformance.Elevation` console helper (UAC on-demand)

## Build & Run

```bash
dotnet build
dotnet run --project src/WindowsPerformance.App
```

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
