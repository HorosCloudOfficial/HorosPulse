# Scroll Dashboard content and stitch overlapping PrintWindow strips into one tall PNG.
# Overlap trim: scroll-% estimate + image template match (Find-BitmapVerticalOverlap) so sections never duplicate.
# Usage: .\SCRIPTS\capture-horospulse-scroll-stitch.ps1 [-OutputPath <path>] [-Maximize] [-RestoreWindowState]

[CmdletBinding()]
param(
    [string]$OutputPath,
    [string]$ViewName = 'Dashboard',
    [int]$WaitSeconds = 30,
    [int]$OverlapPx = 48,
    [int]$MaxStrips = 25,
    [switch]$Maximize,
    [switch]$Fullscreen,
    [switch]$RestoreWindowState,
    [switch]$NoRestart
)

. "$PSScriptRoot\_HorosPulseCaptureCommon.ps1"

if (-not $OutputPath) {
    if ($Maximize -or $Fullscreen) {
        $OutputPath = Join-Path $PSScriptRoot '..\docs\screenshots\capture-comparison\method-d-fullscreen\01-dashboard-stitched.png'
    }
    else {
        $OutputPath = Join-Path $PSScriptRoot '..\docs\screenshots\capture-comparison\method-c-scroll-stitch\01-dashboard-stitched.png'
    }
}

$session = $null
try {
    $session = Initialize-HorosPulseCaptureSession -WaitSeconds $WaitSeconds -Maximize:$Maximize -Fullscreen:$Fullscreen -NoRestart:$NoRestart
    Invoke-HorosPulseSidebarNavigation -Window $session.Automation -ViewName $ViewName -DelayMs 1200
    if ($session.Maximized) {
        Maximize-HorosPulseWindow -Hwnd $session.Hwnd
    }
    else {
        Restore-HorosPulseWindow -Hwnd $session.Hwnd
    }

    $scrollEl = $null
    $scrollPattern = $null
    try {
        for ($attempt = 0; $attempt -lt 6; $attempt++) {
            try {
                $scrollEl = Get-HorosPulseContentScrollViewer -Window $session.Automation
                break
            }
            catch {
                if ($attempt -ge 5) { throw }
                Start-Sleep -Milliseconds 500
                $session.Automation = Get-HorosPulseAutomationWindow -Hwnd $session.Hwnd
            }
        }
        $scrollPattern = $scrollEl.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    }
    catch {
        Write-Warning "No scrollable content area ($($_.Exception.Message)) - saving single full-window capture."
        Invoke-HorosPulsePrintWindowCapture -Hwnd $session.Hwnd -OutputPath $OutputPath | Out-Null
        $check = Test-HorosPulseCaptureImage -ImagePath $OutputPath
        Write-Host "Single capture -> $OutputPath ($($check.Reason))"
        if (-not $check.Valid) {
            Write-Error "Capture validation failed: $($check.Reason)"
        }
        return
    }

    if (-not $scrollPattern.Current.VerticallyScrollable) {
        Write-Warning "$ViewName content is not vertically scrollable - saving single full-window capture."
        Invoke-HorosPulsePrintWindowCapture -Hwnd $session.Hwnd -OutputPath $OutputPath | Out-Null
        $check = Test-HorosPulseCaptureImage -ImagePath $OutputPath
        Write-Host "Single capture -> $OutputPath ($($check.Reason))"
        if (-not $check.Valid) {
            Write-Error "Capture validation failed: $($check.Reason)"
        }
        return
    }

    $contentRect = $scrollEl.Current.BoundingRectangle
    $cx = [int][Math]::Round($contentRect.X)
    $cy = [int][Math]::Round($contentRect.Y)
    $cw = [int][Math]::Max(1, [Math]::Round($contentRect.Width))
    $ch = [int][Math]::Max(1, [Math]::Round($contentRect.Height))

    Write-Host "Scroll area: ${cx},${cy} ${cw}x${ch}"

    # Scroll to top — WPF ScrollPattern.Scroll throws when already at top; use safe helper.
    Reset-HorosPulseScrollToTop -ScrollPattern $scrollPattern

    # Warm-up: scroll full content once so virtualization/layout is stable before stitching.
    Invoke-HorosPulseScrollWarmup -ScrollPattern $scrollPattern
    Reset-HorosPulseScrollToTop -ScrollPattern $scrollPattern
    Start-Sleep -Milliseconds 400

    $strips = New-Object System.Collections.Generic.List[System.Drawing.Bitmap]
    $stripPcts = New-Object System.Collections.Generic.List[float]
    $seenPercents = New-Object 'System.Collections.Generic.HashSet[float]'

    $viewSize = $scrollPattern.Current.VerticalViewSize
    # Scroll percent 0..100 spans the scrollable range (100 - viewSize), not full content height.
    $scrollablePct = [Math]::Max(0.5, 100 - $viewSize)
    $overlapMarginPct = [Math]::Max(2, $viewSize * 0.04)
    $scrollStepPct = [Math]::Max(8, (($viewSize / $scrollablePct) * 100) - $overlapMarginPct)

    for ($i = 0; $i -lt $MaxStrips; $i++) {
        $pct = $scrollPattern.Current.VerticalScrollPercent
        if ($seenPercents.Contains($pct) -and $i -gt 0) { break }
        [void]$seenPercents.Add($pct)
        $stripPcts.Add($pct) | Out-Null

        if ($session.Maximized) {
            Maximize-HorosPulseWindow -Hwnd $session.Hwnd
        }
        else {
            Restore-HorosPulseWindow -Hwnd $session.Hwnd
        }
        Start-Sleep -Milliseconds 200

        $tempFull = Join-Path ([System.IO.Path]::GetTempPath()) "horospulse-strip-$i.png"
        Invoke-HorosPulsePrintWindowCapture -Hwnd $session.Hwnd -OutputPath $tempFull | Out-Null

        $fullBmp = [System.Drawing.Bitmap]::FromFile($tempFull)
        try {
            $winRect = New-Object HorosPulseNative+RECT
            [void][HorosPulseNative]::GetWindowRect($session.Hwnd, [ref]$winRect)

            $relX = [Math]::Max(0, $cx - $winRect.Left)
            $relY = [Math]::Max(0, $cy - $winRect.Top)
            $cropW = [Math]::Min($cw, $fullBmp.Width - $relX)
            $cropH = [Math]::Min($ch, $fullBmp.Height - $relY)

            if ($cropW -gt 0 -and $cropH -gt 0) {
                $strip = Copy-BitmapRegion -Source $fullBmp -X $relX -Y $relY -Width $cropW -Height $cropH
                $strips.Add($strip) | Out-Null
                Write-Host "  Strip $($i + 1): scroll $($pct.ToString('F1'))% -> ${cropW}x${cropH}"
            }
        }
        finally {
            $fullBmp.Dispose()
            Remove-Item $tempFull -Force -ErrorAction SilentlyContinue
        }

        if ($pct -ge 99.5) { break }

        $nextPct = [Math]::Min(100, $pct + $scrollStepPct)
        if ($nextPct -le $pct) { break }

        $scrolled = $false
        try {
            Set-HorosPulseScrollVerticalPercent -ScrollPattern $scrollPattern -Percent $nextPct
            Start-Sleep -Milliseconds 350
            $scrolled = ($scrollPattern.Current.VerticalScrollPercent -gt $pct)
        }
        catch {
            Write-Verbose "SetScrollPercent($nextPct) failed: $($_.Exception.Message)"
        }

        if (-not $scrolled) {
            if (-not (Invoke-HorosPulseScrollPageDown -ScrollPattern $scrollPattern)) { break }
        }
    }

    if ($strips.Count -eq 0) {
        throw 'No content strips captured.'
    }

    # Estimate overlaps from scroll %, then refine via image template match (fixes WPF ViewSize mis-report).
    $estimatedOverlaps = Get-HorosPulseStripMergeOverlaps -ScrollPcts $stripPcts.ToArray() -StripHeight $ch -ViewSizePercent $viewSize -DefaultOverlapPx $OverlapPx
    $mergeOverlaps = Get-HorosPulseStripMergeOverlapsFromImages -Strips $strips.ToArray() -EstimatedOverlapsPx $estimatedOverlaps -DefaultOverlapPx $OverlapPx
    $merged = Merge-VerticalBitmapStrips -Strips $strips.ToArray() -OverlapPx $OverlapPx -OverlapsPx $mergeOverlaps
    $dir = Split-Path -Parent $OutputPath
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    $merged.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    foreach ($s in $strips) { $s.Dispose() }
    $merged.Dispose()

    $check = Test-HorosPulseCaptureImage -ImagePath $OutputPath
    Write-Host "Stitched $($strips.Count) strips -> $OutputPath ($($check.Reason))"

    if (-not $check.Valid) {
        Write-Error "Stitched capture validation failed: $($check.Reason)"
    }
}
finally {
    if ($null -ne $session) {
        Complete-HorosPulseCaptureSession -Session $session -RestoreWindowState:$RestoreWindowState
    }
}
