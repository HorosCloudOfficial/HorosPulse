# Navigate each HorosPulse sidebar view via UIAutomation and capture full window PNGs.
# Usage: .\SCRIPTS\capture-horospulse-all-views.ps1 [-OutputDir <path>] [-Maximize] [-RestoreWindowState]

[CmdletBinding()]
param(
    [string]$OutputDir,
    [int]$WaitSeconds = 30,
    [int]$NavDelayMs = 1500,
    [string]$FromView,
    [switch]$Maximize,
    [switch]$Fullscreen,
    [switch]$RestoreWindowState
)

. "$PSScriptRoot\_HorosPulseCaptureCommon.ps1"

if (-not $OutputDir) {
    $subdir = if ($Maximize -or $Fullscreen) { 'method-d-fullscreen' } else { 'method-b-printwindow' }
    $OutputDir = Join-Path $PSScriptRoot "..\docs\screenshots\capture-comparison\$subdir"
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$session = $null
try {
    $session = Initialize-HorosPulseCaptureSession -WaitSeconds $WaitSeconds -Maximize:$Maximize -Fullscreen:$Fullscreen
    $views = Get-HorosPulseViewManifest
    if ($FromView) {
        $idx = 0
        for ($i = 0; $i -lt $views.Count; $i++) {
            if ($views[$i].Name -eq $FromView) { $idx = $i; break }
        }
        $views = $views[$idx..($views.Count - 1)]
    }
    $mode = if ($session.Maximized) { 'maximized' } else { 'normal' }

    Write-Host "HorosPulse PID $($session.Process.Id) - capturing $($views.Count) views ($mode) to $OutputDir"

    $results = @()
    foreach ($view in $views) {
        Write-Host "  -> $($view.Name)"
        try {
            $session.Hwnd = Get-HorosPulseMainWindowHandle -Process $session.Process
            Restore-HorosPulseWindow -Hwnd $session.Hwnd
            $session.Automation = Get-HorosPulseAutomationWindow -Hwnd $session.Hwnd
            Invoke-HorosPulseSidebarNavigation -Window $session.Automation -ViewName $view.Name -DelayMs $NavDelayMs -Hwnd $session.Hwnd
        }
        catch {
            Write-Warning "  Navigation failed for $($view.Name): $($_.Exception.Message)"
            $results += [PSCustomObject]@{
                View       = $view.Name
                File       = $view.File
                Valid      = $false
                Resolution = 'n/a'
                Note       = "Navigation: $($_.Exception.Message)"
            }
            continue
        }

        $session.Hwnd = Get-HorosPulseMainWindowHandle -Process $session.Process
        if ($session.Maximized) {
            Maximize-HorosPulseWindow -Hwnd $session.Hwnd
        }
        else {
            Restore-HorosPulseWindow -Hwnd $session.Hwnd
        }
        Start-Sleep -Milliseconds 400

        $outFile = Join-Path $OutputDir $view.File
        try {
            Invoke-HorosPulsePrintWindowCapture -Hwnd $session.Hwnd -OutputPath $outFile | Out-Null
            $check = Test-HorosPulseCaptureImage -ImagePath $outFile
        }
        catch {
            $check = @{ Valid = $false; Reason = $_.Exception.Message }
            Write-Warning "  Capture failed for $($view.Name): $($check.Reason)"
        }

        $results += [PSCustomObject]@{
            View       = $view.Name
            File       = $view.File
            Valid      = $check.Valid
            Resolution = if ($check['Width']) { "$($check.Width)x$($check.Height)" } else { 'n/a' }
            Note       = $check.Reason
        }

        if (-not $check.Valid) {
            Write-Warning "  Validation failed for $($view.Name): $($check.Reason)"
        }
    }

    Write-Host ''
    Write-Host '--- Summary ---'
    $results | Format-Table -AutoSize

    $failed = @($results | Where-Object { -not $_.Valid })
    if ($failed.Count -gt 0) {
        Write-Error "$($failed.Count) capture(s) failed validation."
    }
}
finally {
    if ($null -ne $session) {
        Complete-HorosPulseCaptureSession -Session $session -RestoreWindowState:$RestoreWindowState
    }
}
