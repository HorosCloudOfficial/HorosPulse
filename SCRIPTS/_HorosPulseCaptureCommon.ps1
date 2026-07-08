# Shared helpers for HorosPulse screenshot capture scripts.
# Dot-source: . "$PSScriptRoot\_HorosPulseCaptureCommon.ps1"

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

if (-not ('HorosPulseNative' -as [type])) {
    Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class HorosPulseNative
{
    public const int PW_RENDERFULLCONTENT = 0x00000002;
    public const int SW_RESTORE = 9;
    public const int SW_SHOW = 5;
    public const int SW_MAXIMIZE = 3;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X, Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width { get { return Right - Left; } }
        public int Height { get { return Bottom - Top; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    private class WindowSearchData
    {
        public uint ProcessId;
        public string TitleHint;
        public IntPtr Found;
    }

    private static bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
    {
        var data = (WindowSearchData)GCHandle.FromIntPtr(lParam).Target;
        uint pid;
        GetWindowThreadProcessId(hWnd, out pid);
        if (pid != data.ProcessId) return true;

        var sb = new StringBuilder(256);
        GetWindowText(hWnd, sb, sb.Capacity);
        string title = sb.ToString();
        if (title.IndexOf(data.TitleHint, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            data.Found = hWnd;
            return false;
        }
        return true;
    }

    public static IntPtr FindMainWindow(uint processId, string titleHint)
    {
        var data = new WindowSearchData { ProcessId = processId, TitleHint = titleHint, Found = IntPtr.Zero };
        var handle = GCHandle.Alloc(data);
        try
        {
            EnumWindows(EnumWindowCallback, GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
        }
        return data.Found;
    }
}
"@
}

function Get-HorosPulseProcess {
    param(
        [int]$WaitSeconds = 60,
        [string]$ProcessName = 'HorosPulse.App'
    )

    $deadline = (Get-Date).AddSeconds($WaitSeconds)
    do {
        $procs = @(Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)
        foreach ($p in $procs) {
            if ($p.MainWindowHandle -ne [IntPtr]::Zero) { return $p }
            $hwnd = [HorosPulseNative]::FindMainWindow([uint]$p.Id, 'HorosPulse')
            if ($hwnd -ne [IntPtr]::Zero) { return $p }
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    throw "Process '$ProcessName' not found within ${WaitSeconds}s. Launch HorosPulse first (e.g. starter.bat)."
}

function Get-HorosPulseMainWindowHandle {
    param([System.Diagnostics.Process]$Process)

    $hwnd = $Process.MainWindowHandle
    if ($hwnd -ne [IntPtr]::Zero) { return $hwnd }

    $hwnd = [HorosPulseNative]::FindMainWindow([uint]$Process.Id, 'HorosPulse')
    if ($hwnd -eq [IntPtr]::Zero) {
        throw "Could not find HorosPulse main window HWND for PID $($Process.Id)."
    }
    return $hwnd
}

function Restore-HorosPulseWindow {
    param([IntPtr]$Hwnd)

    if ([HorosPulseNative]::IsIconic($Hwnd)) {
        [void][HorosPulseNative]::ShowWindow($Hwnd, [HorosPulseNative]::SW_RESTORE)
    }
    if (-not [HorosPulseNative]::IsWindowVisible($Hwnd)) {
        [void][HorosPulseNative]::ShowWindow($Hwnd, [HorosPulseNative]::SW_SHOW)
    }

    [void][HorosPulseNative]::BringWindowToTop($Hwnd)
    [void][HorosPulseNative]::SetForegroundWindow($Hwnd)
    Start-Sleep -Milliseconds 400
}

function Save-HorosPulseWindowPlacement {
    param([IntPtr]$Hwnd)

    if ($Hwnd -eq [IntPtr]::Zero) { return $null }

    $placement = New-Object HorosPulseNative+WINDOWPLACEMENT
    $placement.length = [System.Runtime.InteropServices.Marshal]::SizeOf($placement)
    if (-not [HorosPulseNative]::GetWindowPlacement($Hwnd, [ref]$placement)) {
        Write-Warning 'GetWindowPlacement failed - restore after capture will be skipped.'
        return $null
    }
    return $placement
}

function Restore-HorosPulseWindowPlacement {
    param(
        [IntPtr]$Hwnd,
        [HorosPulseNative+WINDOWPLACEMENT]$Placement
    )

    [void][HorosPulseNative]::SetWindowPlacement($Hwnd, [ref]$Placement)
    Start-Sleep -Milliseconds 400
}

function Maximize-HorosPulseWindow {
    param([IntPtr]$Hwnd)

    Restore-HorosPulseWindow -Hwnd $Hwnd
    if (-not [HorosPulseNative]::IsZoomed($Hwnd)) {
        [void][HorosPulseNative]::ShowWindow($Hwnd, [HorosPulseNative]::SW_MAXIMIZE)
    }
    Start-Sleep -Milliseconds 600
}

function Complete-HorosPulseCaptureSession {
    param(
        [hashtable]$Session,
        [switch]$RestoreWindowState
    )

    if ($RestoreWindowState -and $null -ne $Session.WindowPlacement) {
        Restore-HorosPulseWindowPlacement -Hwnd $Session.Hwnd -Placement $Session.WindowPlacement
    }
}

function Start-HorosPulseApp {
    param(
        [string]$Configuration = 'Debug',
        [int]$PostLaunchDelaySeconds = 18
    )

    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $exe = Join-Path $repoRoot "src\HorosPulse.App\bin\$Configuration\net9.0-windows\HorosPulse.App.exe"
    if (-not (Test-Path $exe)) {
        throw "HorosPulse executable not found: $exe. Build with starter.bat first."
    }

    Get-Process -Name 'HorosPulse.App' -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 2
    Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe -Parent)
    Start-Sleep -Seconds $PostLaunchDelaySeconds
}

function Test-HorosPulseWindowRect {
    param([IntPtr]$Hwnd)

    $rect = New-Object HorosPulseNative+RECT
    [void][HorosPulseNative]::GetWindowRect($Hwnd, [ref]$rect)
    return @{
        Valid  = ($rect.Width -ge 400 -and $rect.Height -ge 300)
        Width  = $rect.Width
        Height = $rect.Height
        Rect   = $rect
    }
}

function Ensure-HorosPulseWindowReady {
    param(
        [ref]$Process,
        [ref]$Hwnd,
        [switch]$AllowRestart
    )

    $hwnd = Get-HorosPulseMainWindowHandle -Process $Process.Value
    Restore-HorosPulseWindow -Hwnd $hwnd
    $check = Test-HorosPulseWindowRect -Hwnd $hwnd

    if (-not $check.Valid -and $AllowRestart) {
        Write-Host 'HorosPulse window hidden or zero-size — restarting app...'
        Start-HorosPulseApp
        $Process.Value = Get-HorosPulseProcess -WaitSeconds 60
        $hwnd = Get-HorosPulseMainWindowHandle -Process $Process.Value
        Restore-HorosPulseWindow -Hwnd $hwnd
        $check = Test-HorosPulseWindowRect -Hwnd $hwnd
    }

    if (-not $check.Valid) {
        throw "HorosPulse window rect too small ($($check.Width)x$($check.Height)). Restore from tray and retry."
    }

    $Hwnd.Value = $hwnd
    return $check
}

function Get-HorosPulseAutomationWindow {
    param([IntPtr]$Hwnd)

    try {
        $fromHandle = [System.Windows.Automation.AutomationElement]::FromHandle($Hwnd)
        if ($null -ne $fromHandle) { return $fromHandle }
    }
    catch {
        Write-Verbose "FromHandle failed: $($_.Exception.Message)"
    }

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $nameCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, 'HorosPulse')
    $win = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $nameCond)
    if ($null -eq $win) {
        throw 'UIAutomation could not locate HorosPulse main window.'
    }
    return $win
}

function Invoke-HorosPulsePrintWindowCapture {
    param(
        [IntPtr]$Hwnd,
        [string]$OutputPath,
        [int]$MaxAttempts = 3
    )

    $rect = New-Object HorosPulseNative+RECT
    [void][HorosPulseNative]::GetWindowRect($Hwnd, [ref]$rect)

    $width = [Math]::Max(1, $rect.Width)
    $height = [Math]::Max(1, $rect.Height)
    if ($width -lt 400 -or $height -lt 300) {
        throw "Window rect too small for capture (${width}x${height})."
    }

    $bitmap = New-Object System.Drawing.Bitmap $width, $height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $captured = $false
    try {
        for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
            if ($attempt -gt 1) {
                Start-Sleep -Milliseconds (200 * $attempt)
                Restore-HorosPulseWindow -Hwnd $Hwnd
            }

            $hdc = $graphics.GetHdc()
            try {
                $ok = [HorosPulseNative]::PrintWindow($Hwnd, $hdc, [HorosPulseNative]::PW_RENDERFULLCONTENT)
                if (-not $ok) {
                    $ok = [HorosPulseNative]::PrintWindow($Hwnd, $hdc, 0)
                }
                if ($ok) {
                    $captured = $true
                    break
                }
            }
            finally {
                $graphics.ReleaseHdc($hdc)
            }
        }

        if (-not $captured) {
            Restore-HorosPulseWindow -Hwnd $Hwnd
            Start-Sleep -Milliseconds 300
            $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
            $captured = $true
        }
    }
    finally {
        $graphics.Dispose()
    }

    $dir = Split-Path -Parent $OutputPath
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    return $OutputPath
}

function Test-HorosPulseCaptureImage {
    param([string]$ImagePath)

    if (-not (Test-Path $ImagePath)) {
        return @{ Valid = $false; Reason = 'File missing' }
    }

    $img = [System.Drawing.Image]::FromFile($ImagePath)
    try {
        $w = $img.Width
        $h = $img.Height
        if ($w -lt 400 -or $h -lt 300) {
            return @{ Valid = $false; Reason = "Too small (${w}x${h})" }
        }

        # Sample title-bar region — Tokyo Night uses dark #1E1E2E / #24283B backgrounds.
        $samples = @(
            $img.GetPixel([int]($w * 0.15), 20),
            $img.GetPixel([int]($w * 0.5), [int]($h * 0.5)),
            $img.GetPixel(30, [int]($h * 0.3))
        )

        $darkCount = 0
        foreach ($c in $samples) {
            $lum = 0.2126 * $c.R + 0.7152 * $c.G + 0.0722 * $c.B
            if ($lum -lt 80) { $darkCount++ }
        }

        if ($darkCount -lt 2) {
            return @{ Valid = $false; Reason = 'Image looks too bright (possible IDE/wrong window)' }
        }

        # Reject obvious VS Code light-gray editor backgrounds (RGB ~30-45 all channels mid-high)
        $mid = $img.GetPixel([int]($w * 0.6), [int]($h * 0.45))
        if ($mid.R -gt 180 -and $mid.G -gt 180 -and $mid.B -gt 180) {
            return @{ Valid = $false; Reason = 'Bright editor-like background detected' }
        }

        return @{ Valid = $true; Reason = "OK ${w}x${h}"; Width = $w; Height = $h }
    }
    finally {
        $img.Dispose()
    }
}

function Invoke-HorosPulseSidebarNavigation {
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [string]$ViewName,
        [int]$DelayMs = 900,
        [IntPtr]$Hwnd = [IntPtr]::Zero
    )

    if ($Hwnd -ne [IntPtr]::Zero) {
        $refreshed = [System.Windows.Automation.AutomationElement]::FromHandle($Hwnd)
        if ($null -ne $refreshed) { $Window = $refreshed }
    }

    $buttonType = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $nameType = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $ViewName)
    $and = New-Object System.Windows.Automation.AndCondition @($buttonType, $nameType)

    $btn = $Window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $and)
    if ($null -eq $btn) {
        $allButtons = $Window.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            $buttonType)
        foreach ($candidate in $allButtons) {
            if ($candidate.Current.Name -eq $ViewName) {
                $btn = $candidate
                break
            }
        }
    }

    if ($null -eq $btn) {
        throw "Sidebar button '$ViewName' not found via UIAutomation."
    }

    $invoke = $btn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $invoke.Invoke()
    Start-Sleep -Milliseconds $DelayMs
}

function Get-HorosPulseContentScrollViewer {
    param([System.Windows.Automation.AutomationElement]$Window)

    $scrollType = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ScrollBar)
    $verticalName = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, 'Vertical')
    $vertBar = $Window.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        (New-Object System.Windows.Automation.AndCondition @($scrollType, $verticalName)))

    if ($null -ne $vertBar) {
        $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
        $parent = $walker.GetParent($vertBar)
        while ($null -ne $parent) {
            $scrollPattern = $null
            if ($parent.TryGetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern, [ref]$scrollPattern)) {
                return $parent
            }
            $parent = $walker.GetParent($parent)
        }
    }

    # Fallback: first element supporting ScrollPattern in content area (right of sidebar).
    $all = $Window.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)

    foreach ($el in $all) {
        $rect = $el.Current.BoundingRectangle
        if ($rect.Width -lt 200 -or $rect.Height -lt 200) { continue }
        if ($rect.X -lt 200) { continue }

        $scrollPattern = $null
        if ($el.TryGetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern, [ref]$scrollPattern)) {
            return $el
        }
    }

    throw 'No scrollable content area found.'
}

function Set-HorosPulseScrollVerticalPercent {
    param(
        [System.Windows.Automation.ScrollPattern]$ScrollPattern,
        [double]$Percent
    )

    $noScroll = [System.Windows.Automation.ScrollPattern]::NoScroll
    $ScrollPattern.SetScrollPercent($noScroll, $Percent)
}

function Reset-HorosPulseScrollToTop {
    param([System.Windows.Automation.ScrollPattern]$ScrollPattern)

    try {
        Set-HorosPulseScrollVerticalPercent -ScrollPattern $ScrollPattern -Percent 0
        Start-Sleep -Milliseconds 300
        return
    }
    catch {
        Write-Verbose "SetScrollPercent(0) failed: $($_.Exception.Message)"
    }

    for ($i = 0; $i -lt 20; $i++) {
        $topPct = $ScrollPattern.Current.VerticalScrollPercent
        if ($topPct -lt 0 -or $topPct -le 0.1) { return }
        try {
            $ScrollPattern.Scroll(
                [System.Windows.Automation.ScrollAmount]::LargeDecrement,
                [System.Windows.Automation.ScrollAmount]::NoAmount)
        }
        catch {
            return
        }
        Start-Sleep -Milliseconds 80
    }
}

function Invoke-HorosPulseScrollPageDown {
    param([System.Windows.Automation.ScrollPattern]$ScrollPattern)

    $before = $ScrollPattern.Current.VerticalScrollPercent
    if ($before -ge 99.5) { return $false }

    try {
        $ScrollPattern.Scroll(
            [System.Windows.Automation.ScrollAmount]::NoAmount,
            [System.Windows.Automation.ScrollAmount]::LargeIncrement)
        Start-Sleep -Milliseconds 350
        $after = $ScrollPattern.Current.VerticalScrollPercent
        return ($after -gt $before -or ($after -ge 99.5 -and $before -lt 99.5))
    }
    catch {
        Write-Verbose "LargeIncrement failed: $($_.Exception.Message)"
    }

    $viewSize = $ScrollPattern.Current.VerticalViewSize
    if ($viewSize -le 0) { return $false }

    $target = [Math]::Min(100, $before + ($viewSize * 0.85))
    if ($target -le $before) { return $false }

    try {
        Set-HorosPulseScrollVerticalPercent -ScrollPattern $ScrollPattern -Percent $target
        Start-Sleep -Milliseconds 350
        return ($ScrollPattern.Current.VerticalScrollPercent -gt $before)
    }
    catch {
        Write-Verbose "SetScrollPercent($target) failed: $($_.Exception.Message)"
        return $false
    }
}

# Warm-up: scroll through full content once so WPF layout/virtualization settles, then return to top.
function Invoke-HorosPulseScrollWarmup {
    param(
        [System.Windows.Automation.ScrollPattern]$ScrollPattern,
        [int]$PauseMs = 120
    )

    Reset-HorosPulseScrollToTop -ScrollPattern $ScrollPattern
    Start-Sleep -Milliseconds 200

    $viewSize = $ScrollPattern.Current.VerticalViewSize
    $scrollablePct = [Math]::Max(0.5, 100 - $viewSize)
    $stepPct = [Math]::Max(8, (($viewSize / $scrollablePct) * 100) * 0.94)

    for ($i = 0; $i -lt 40; $i++) {
        $pct = $ScrollPattern.Current.VerticalScrollPercent
        if ($pct -ge 99.5) { break }

        $nextPct = [Math]::Min(100, $pct + $stepPct)
        if ($nextPct -le $pct) { break }

        try {
            Set-HorosPulseScrollVerticalPercent -ScrollPattern $ScrollPattern -Percent $nextPct
        }
        catch {
            if (-not (Invoke-HorosPulseScrollPageDown -ScrollPattern $ScrollPattern)) { break }
        }
        Start-Sleep -Milliseconds $PauseMs
    }

    try {
        Set-HorosPulseScrollVerticalPercent -ScrollPattern $ScrollPattern -Percent 100
        Start-Sleep -Milliseconds $PauseMs
    }
    catch { }

    Reset-HorosPulseScrollToTop -ScrollPattern $ScrollPattern
    Start-Sleep -Milliseconds 300
}

function Copy-BitmapRegion {
    param(
        [System.Drawing.Bitmap]$Source,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [int]$Height
    )

    $crop = New-Object System.Drawing.Bitmap $Width, $Height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($crop)
    try {
        $g.DrawImage($Source, 0, 0, (New-Object System.Drawing.Rectangle $X, $Y, $Width, $Height),
            [System.Drawing.GraphicsUnit]::Pixel)
    }
    finally {
        $g.Dispose()
    }
    return $crop
}

function Merge-VerticalBitmapStrips {
    param(
        [System.Drawing.Bitmap[]]$Strips,
        [int]$OverlapPx,
        [int[]]$OverlapsPx
    )

    if ($Strips.Count -eq 0) { throw 'No strips to merge.' }
    if ($Strips.Count -eq 1) { return $Strips[0] }

    $width = $Strips[0].Width
    $totalHeight = $Strips[0].Height
    for ($i = 1; $i -lt $Strips.Count; $i++) {
        $overlap = if ($null -ne $OverlapsPx -and $i -le $OverlapsPx.Count) { $OverlapsPx[$i - 1] } else { $OverlapPx }
        $totalHeight += $Strips[$i].Height - $overlap
    }

    $result = New-Object System.Drawing.Bitmap $width, $totalHeight, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($result)
    try {
        $y = 0
        for ($i = 0; $i -lt $Strips.Count; $i++) {
            $g.DrawImage($Strips[$i], 0, $y)
            if ($i -lt $Strips.Count - 1) {
                $overlap = if ($null -ne $OverlapsPx -and $i -lt $OverlapsPx.Count) { $OverlapsPx[$i] } else { $OverlapPx }
                $y += $Strips[$i].Height - $overlap
            }
        }
    }
    finally {
        $g.Dispose()
    }
    return $result
}

function Get-HorosPulseStripMergeOverlaps {
    param(
        [float[]]$ScrollPcts,
        [int]$StripHeight,
        [double]$ViewSizePercent,
        [int]$DefaultOverlapPx
    )

    if ($ScrollPcts.Count -le 1) { return @() }

    $viewFraction = if ($ViewSizePercent -gt 0) { $ViewSizePercent / 100.0 } else { 1.0 }
    $contentHeight = $StripHeight / $viewFraction
    # Scroll percent 0..100 maps to 0..(contentHeight - viewportHeight) pixels.
    $scrollablePx = [Math]::Max(1, [int][Math]::Round($contentHeight - $StripHeight))

    $overlaps = New-Object System.Collections.Generic.List[int]
    for ($i = 0; $i -lt $ScrollPcts.Count - 1; $i++) {
        $deltaPct = [Math]::Max(0, $ScrollPcts[$i + 1] - $ScrollPcts[$i])
        $scrollPx = [int][Math]::Round(($deltaPct / 100.0) * $scrollablePx)
        $scrollPx = [Math]::Min($scrollPx, $StripHeight - 1)

        $overlap = $StripHeight - $scrollPx
        if ($overlap -lt 0) { $overlap = 0 }
        if ($overlap -ge $StripHeight) { $overlap = [Math]::Max(0, $StripHeight - 1) }

        $overlaps.Add([int]$overlap) | Out-Null
    }

    return $overlaps.ToArray()
}

# Template-match the bottom of the upper strip against the top of the lower strip to find true pixel overlap.
function Find-BitmapVerticalOverlap {
    param(
        [System.Drawing.Bitmap]$UpperStrip,
        [System.Drawing.Bitmap]$LowerStrip,
        [int]$MinOverlapPx = 16,
        [int]$MaxOverlapPx = 0,
        [int]$StepPx = 2,
        [int]$SampleStrideX = 6
    )

    if ($null -eq $UpperStrip -or $null -eq $LowerStrip) { return $MinOverlapPx }

    $maxSearch = if ($MaxOverlapPx -gt 0) {
        $MaxOverlapPx
    }
    else {
        [Math]::Min($UpperStrip.Height, $LowerStrip.Height) - 1
    }
    $maxSearch = [Math]::Max($MinOverlapPx, $maxSearch)

    $width = [Math]::Min($UpperStrip.Width, $LowerStrip.Width)
    if ($width -lt 8) { return $MinOverlapPx }

    $bestOverlap = $MinOverlapPx
    $bestScore = [double]::MaxValue

    for ($overlap = $MinOverlapPx; $overlap -le $maxSearch; $overlap += $StepPx) {
        $compareRows = [Math]::Min($overlap, 96)
        $rowStart = $overlap - $compareRows
        $score = 0.0
        $samples = 0

        for ($row = 0; $row -lt $compareRows; $row++) {
            $yUpper = $UpperStrip.Height - $overlap + $row
            $yLower = $row
            if ($yUpper -lt 0 -or $yLower -ge $LowerStrip.Height) { continue }

            for ($x = 0; $x -lt $width; $x += $SampleStrideX) {
                $cUpper = $UpperStrip.GetPixel($x, $yUpper)
                $cLower = $LowerStrip.GetPixel($x, $yLower)
                $score += [Math]::Abs($cUpper.R - $cLower.R) +
                    [Math]::Abs($cUpper.G - $cLower.G) +
                    [Math]::Abs($cUpper.B - $cLower.B)
                $samples++
            }
        }

        if ($samples -gt 0) {
            $avg = $score / $samples
            if ($avg -lt $bestScore) {
                $bestScore = $avg
                $bestOverlap = $overlap
            }
        }
    }

    return $bestOverlap
}

function Get-HorosPulseStripMergeOverlapsFromImages {
    param(
        [System.Drawing.Bitmap[]]$Strips,
        [int[]]$EstimatedOverlapsPx,
        [int]$DefaultOverlapPx = 48
    )

    if ($Strips.Count -le 1) { return @() }

    $overlaps = New-Object System.Collections.Generic.List[int]
    for ($i = 0; $i -lt $Strips.Count - 1; $i++) {
        $estimated = if ($null -ne $EstimatedOverlapsPx -and $i -lt $EstimatedOverlapsPx.Count) {
            $EstimatedOverlapsPx[$i]
        }
        else {
            $DefaultOverlapPx
        }

        $searchMax = [Math]::Min($Strips[$i].Height - 1, $Strips[$i + 1].Height - 1)
        $detected = Find-BitmapVerticalOverlap `
            -UpperStrip $Strips[$i] `
            -LowerStrip $Strips[$i + 1] `
            -MinOverlapPx 8 `
            -MaxOverlapPx $searchMax

        # Prefer scroll-math estimate when image match disagrees wildly (repeated section headers confuse template match).
        $tolerance = [Math]::Max(64, [int][Math]::Round($Strips[$i].Height * 0.12))
        $final = if ([Math]::Abs($detected - $estimated) -le $tolerance) { $detected } else { $estimated }
        if ($final -lt 0) { $final = 0 }
        if ($final -ge $Strips[$i].Height) { $final = [Math]::Max(0, $Strips[$i].Height - 1) }

        $overlaps.Add([int]$final) | Out-Null
        Write-Host "  Merge overlap $($i + 1)->$($i + 2): estimated=${estimated}px, detected=${detected}px, used=${final}px"
    }

    return $overlaps.ToArray()
}

function Get-HorosPulseCompactWindowHandle {
    param([System.Diagnostics.Process]$Process)

    $hwnd = [HorosPulseNative]::FindMainWindow([uint]$Process.Id, 'HorosPulse Kompakt')
    if ($hwnd -eq [IntPtr]::Zero) {
        throw "Could not find HorosPulse compact window HWND for PID $($Process.Id). Open it from tray or Einstellungen first."
    }
    return $hwnd
}

function Open-HorosPulseCompactWindow {
    param(
        [System.Windows.Automation.AutomationElement]$MainWindow,
        [IntPtr]$MainHwnd = [IntPtr]::Zero
    )

    if ($MainHwnd -ne [IntPtr]::Zero) {
        $refreshed = [System.Windows.Automation.AutomationElement]::FromHandle($MainHwnd)
        if ($null -ne $refreshed) { $MainWindow = $refreshed }
    }

    $buttonType = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $nameCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, 'Kompakt-Fenster öffnen')
    $btn = $MainWindow.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        (New-Object System.Windows.Automation.AndCondition @($buttonType, $nameCond)))

    if ($null -eq $btn) {
        throw "Button 'Kompakt-Fenster oeffnen' not found - navigate to Einstellungen > Kompakt-Fenster tab first."
    }

    $invoke = $btn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $invoke.Invoke()
    Start-Sleep -Milliseconds 900
}

function Initialize-HorosPulseCompactCaptureSession {
    param(
        [int]$WaitSeconds = 60,
        [switch]$NoRestart
    )

    if (-not $NoRestart) {
        Start-HorosPulseApp
    }

    $proc = Get-HorosPulseProcess -WaitSeconds $WaitSeconds
    $mainHwnd = Get-HorosPulseMainWindowHandle -Process $proc
    Restore-HorosPulseWindow -Hwnd $mainHwnd
    $automation = Get-HorosPulseAutomationWindow -Hwnd $mainHwnd

    Invoke-HorosPulseSidebarNavigation -Window $automation -ViewName 'Einstellungen' -DelayMs 1200 -Hwnd $mainHwnd

    $tabType = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::TabItem)
    $tabName = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, 'Kompakt-Fenster')
    $tab = $automation.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        (New-Object System.Windows.Automation.AndCondition @($tabType, $tabName)))
    if ($null -ne $tab) {
        $selection = $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        $selection.Select()
        Start-Sleep -Milliseconds 500
    }

    Open-HorosPulseCompactWindow -MainWindow $automation -MainHwnd $mainHwnd

    $compactHwnd = $null
    $deadline = (Get-Date).AddSeconds(10)
    do {
        try {
            $compactHwnd = Get-HorosPulseCompactWindowHandle -Process $proc
            break
        }
        catch {
            Start-Sleep -Milliseconds 300
        }
    } while ((Get-Date) -lt $deadline)

    if ($null -eq $compactHwnd -or $compactHwnd -eq [IntPtr]::Zero) {
        throw 'Compact window did not appear within 10s.'
    }

    Restore-HorosPulseWindow -Hwnd $compactHwnd

    return @{
        Process    = $proc
        MainHwnd   = $mainHwnd
        Hwnd       = $compactHwnd
        Automation = $automation
    }
}

function Get-HorosPulseViewManifest {
    return @(
        @{ Name = 'Dashboard';     File = '01-dashboard.png' },
        @{ Name = 'Energie';       File = '02-energie.png' },
        @{ Name = 'Cursor';        File = '03-cursor.png' },
        @{ Name = 'Monitor';       File = '04-monitor.png' },
        @{ Name = 'Snapshots';     File = '05-snapshots.png' },
        @{ Name = 'Presets';       File = '06-presets.png' },
        @{ Name = 'Dienste';       File = '07-dienste.png' },
        @{ Name = 'Startup';       File = '08-startup.png' },
        @{ Name = 'Visuell';       File = '09-visuell.png' },
        @{ Name = 'Speicher';      File = '10-speicher.png' },
        @{ Name = 'Netzwerk';      File = '11-netzwerk.png' },
        @{ Name = 'Trends';        File = '12-trends.png' },
        @{ Name = 'Prozesse';      File = '13-prozesse.png' },
        @{ Name = 'Festplatte';    File = '14-festplatte.png' },
        @{ Name = 'Tasks';         File = '15-tasks.png' },
        @{ Name = 'Registry';      File = '16-registry.png' },
        @{ Name = 'Einstellungen'; File = '17-einstellungen.png' }
    )
}

function Initialize-HorosPulseCaptureSession {
    param(
        [int]$WaitSeconds = 60,
        [switch]$Maximize,
        [switch]$Fullscreen,
        [switch]$NoRestart
    )

    if (-not $NoRestart) {
        Start-HorosPulseApp
    }

    $proc = Get-HorosPulseProcess -WaitSeconds $WaitSeconds
    $hwnd = Get-HorosPulseMainWindowHandle -Process $proc
    Restore-HorosPulseWindow -Hwnd $hwnd

    $placement = $null
    if ($Maximize -or $Fullscreen) {
        $placement = Save-HorosPulseWindowPlacement -Hwnd $hwnd
        Maximize-HorosPulseWindow -Hwnd $hwnd
    }

    $automation = Get-HorosPulseAutomationWindow -Hwnd $hwnd

    # WPF UIAutomation tree needs extra time after first show.
    $deadline = (Get-Date).AddSeconds(15)
    do {
        $buttonType = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Button)
        $dashboard = New-Object System.Windows.Automation.AndCondition @(
            $buttonType,
            (New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::NameProperty, 'Dashboard')))
        if ($null -ne $automation.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $dashboard)) {
            break
        }
        Start-Sleep -Milliseconds 400
        $automation = Get-HorosPulseAutomationWindow -Hwnd $hwnd
    } while ((Get-Date) -lt $deadline)

    return @{
        Process         = $proc
        Hwnd            = $hwnd
        Automation      = $automation
        WindowPlacement = $placement
        Maximized       = [bool]($Maximize -or $Fullscreen)
    }
}
