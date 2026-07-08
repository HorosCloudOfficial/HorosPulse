# Capture HorosPulse compact window via PrintWindow.
# Opens Einstellungen > Kompakt-Fenster, launches compact UI, then captures.
# Usage: .\SCRIPTS\capture-horospulse-compact-window.ps1 [-OutputPath <path>]

[CmdletBinding()]
param(
    [string]$OutputPath,
    [int]$WaitSeconds = 60,
    [switch]$NoRestart
)

. "$PSScriptRoot\_HorosPulseCaptureCommon.ps1"

if (-not $OutputPath) {
    $OutputPath = Join-Path $PSScriptRoot '..\docs\screenshots\capture-comparison\method-b-printwindow\18-compact-window.png'
}

$session = $null
try {
    $session = Initialize-HorosPulseCompactCaptureSession -WaitSeconds $WaitSeconds -NoRestart:$NoRestart
    Write-Host "HorosPulse compact HWND $($session.Hwnd) (PID $($session.Process.Id))"

    $path = Invoke-HorosPulsePrintWindowCapture -Hwnd $session.Hwnd -OutputPath $OutputPath
    $check = Test-HorosPulseCaptureImage -ImagePath $path

    Write-Host "Saved: $path"
    Write-Host "Validation: $($check.Reason)"

    if (-not $check.Valid) {
        Write-Error "Capture validation failed: $($check.Reason)"
    }
}
finally {
    if ($null -ne $session -and $session.Hwnd -ne [IntPtr]::Zero) {
        [void][HorosPulseNative]::ShowWindow($session.Hwnd, 0) # SW_HIDE
    }
}
