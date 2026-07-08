# Capture HorosPulse main window via PrintWindow (PW_RENDERFULLCONTENT).
# Usage: .\SCRIPTS\capture-horospulse-window.ps1 [-OutputPath <path>] [-Maximize] [-RestoreWindowState]

[CmdletBinding()]
param(
    [string]$OutputPath,
    [int]$WaitSeconds = 30,
    [switch]$Maximize,
    [switch]$Fullscreen,
    [switch]$RestoreWindowState
)

. "$PSScriptRoot\_HorosPulseCaptureCommon.ps1"

if (-not $OutputPath) {
    $subdir = if ($Maximize -or $Fullscreen) { 'method-d-fullscreen' } else { 'method-b-printwindow' }
    $OutputPath = Join-Path $PSScriptRoot "..\docs\screenshots\capture-comparison\$subdir\window-single.png"
}

$session = $null
try {
    $session = Initialize-HorosPulseCaptureSession -WaitSeconds $WaitSeconds -Maximize:$Maximize -Fullscreen:$Fullscreen
    $mode = if ($session.Maximized) { 'maximized' } else { 'normal' }
    Write-Host "HorosPulse PID $($session.Process.Id), HWND $($session.Hwnd) ($mode)"

    $path = Invoke-HorosPulsePrintWindowCapture -Hwnd $session.Hwnd -OutputPath $OutputPath
    $check = Test-HorosPulseCaptureImage -ImagePath $path

    Write-Host "Saved: $path"
    Write-Host "Validation: $($check.Reason)"

    if (-not $check.Valid) {
        Write-Error "Capture validation failed: $($check.Reason)"
    }
}
finally {
    if ($null -ne $session) {
        Complete-HorosPulseCaptureSession -Session $session -RestoreWindowState:$RestoreWindowState
    }
}
