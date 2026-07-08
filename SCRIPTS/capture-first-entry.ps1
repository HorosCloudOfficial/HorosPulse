# Capture HorosPulse Dashboard (first sidebar entry) with full scrollable content.
# Uses scroll-stitch when the dashboard content area is vertically scrollable.
# Usage: .\SCRIPTS\capture-first-entry.ps1 [-OutputPath <path>]

[CmdletBinding()]
param(
    [string]$OutputPath,
    [int]$WaitSeconds = 60
)

if (-not $OutputPath) {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $OutputPath = Join-Path $repoRoot 'docs\screenshots\horospulse\01-erster-eintrag.png'
}

try {
    & "$PSScriptRoot\capture-horospulse-scroll-stitch.ps1" `
        -OutputPath $OutputPath `
        -WaitSeconds $WaitSeconds `
        -Maximize `
        -OverlapPx 48 `
        -MaxStrips 25

    if (-not (Test-Path $OutputPath)) {
        throw "Expected output not created: $OutputPath"
    }

    . "$PSScriptRoot\_HorosPulseCaptureCommon.ps1"
    $check = Test-HorosPulseCaptureImage -ImagePath $OutputPath
    Write-Host "Saved: $OutputPath"
    Write-Host "Validation: $($check.Reason)"

    if (-not $check.Valid) {
        throw "Capture validation failed: $($check.Reason)"
    }
}
finally {
    Get-Process -Name 'HorosPulse.App' -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
}
