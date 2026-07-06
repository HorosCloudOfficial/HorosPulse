#Requires -Version 5.1
<#
.SYNOPSIS
    Publishes WindowsPerformance as a portable win-x64 ZIP distribution.
#>
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
Set-Location $repoRoot

$version = (Select-Xml -Path "src\WindowsPerformance.App\WindowsPerformance.App.csproj" -XPath "//Version" -ErrorAction SilentlyContinue).Node.'#text'
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "0.1.0"
}

$artifactsDir = Join-Path $repoRoot "artifacts"
$stagingDir = Join-Path $artifactsDir "WindowsPerformance-$version-$Runtime"
$zipPath = Join-Path $artifactsDir "WindowsPerformance-$version-$Runtime.zip"

Write-Host "Building solution ($Configuration)..."
dotnet build WindowsPerformance.sln -c $Configuration --no-restore 2>$null | Out-Null
dotnet restore WindowsPerformance.sln | Out-Null
dotnet build WindowsPerformance.sln -c $Configuration

$publishArgs = @(
    "publish", "src\WindowsPerformance.App\WindowsPerformance.App.csproj",
    "-c", $Configuration,
    "-r", $Runtime,
    "-o", $stagingDir,
    "/p:PublishSingleFile=false"
)

if ($SelfContained) {
    $publishArgs += @("--self-contained", "true")
}
else {
    $publishArgs += @("--self-contained", "false")
}

Write-Host "Publishing app to $stagingDir..."
dotnet @publishArgs

$elevationSource = Join-Path $repoRoot "src\WindowsPerformance.Elevation\bin\$Configuration\net9.0\WindowsPerformance.Elevation.exe"
if (-not (Test-Path $elevationSource)) {
    dotnet build "src\WindowsPerformance.Elevation\WindowsPerformance.Elevation.csproj" -c $Configuration
}

Copy-Item -Path $elevationSource -Destination (Join-Path $stagingDir "WindowsPerformance.Elevation.exe") -Force

$readmeSource = Join-Path $repoRoot "README.md"
if (Test-Path $readmeSource) {
    Copy-Item -Path $readmeSource -Destination (Join-Path $stagingDir "README.md") -Force
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Creating ZIP: $zipPath"
Compress-Archive -Path (Join-Path $stagingDir '*') -DestinationPath $zipPath -Force

Write-Host "Done: $zipPath"
Write-Host ""
Write-Host "Installer (Velopack):"
Write-Host "  dotnet tool install -g vpk"
Write-Host "  vpk pack -p $stagingDir -o $artifactsDir --packId WindowsPerformance --packVersion $version"
Write-Host "  See docs/architecture.md for auto-update and uninstall rollback opt-in."
