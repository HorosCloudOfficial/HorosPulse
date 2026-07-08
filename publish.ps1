#Requires -Version 5.1
<#
.SYNOPSIS
    Publishes HorosPulse as a portable win-x64 ZIP distribution.
.DESCRIPTION
    Builds and publishes HorosPulse.App, bundles HorosPulse.Elevation.exe, and creates a ZIP.
    Use -Velopack to also run `vpk pack` when the Velopack CLI is installed.
#>
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained,
    [switch]$Velopack
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
Set-Location $repoRoot

$version = (Select-Xml -Path "src\HorosPulse.App\HorosPulse.App.csproj" -XPath "//Version" -ErrorAction SilentlyContinue).Node.'#text'
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "0.1.0"
}

$artifactsDir = Join-Path $repoRoot "artifacts"
$stagingDir = Join-Path $artifactsDir "HorosPulse-$version-$Runtime"
$zipPath = Join-Path $artifactsDir "HorosPulse-$version-$Runtime.zip"
$velopackOutDir = Join-Path $artifactsDir "velopack"

Write-Host "Building solution ($Configuration)..."
dotnet build HorosPulse.sln -c $Configuration --no-restore 2>$null | Out-Null
dotnet restore HorosPulse.sln | Out-Null
dotnet build HorosPulse.sln -c $Configuration

$publishArgs = @(
    "publish", "src\HorosPulse.App\HorosPulse.App.csproj",
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

$elevationDir = Join-Path $repoRoot "src\HorosPulse.Elevation\bin\$Configuration\net9.0"
if (-not (Test-Path (Join-Path $elevationDir "HorosPulse.Elevation.exe"))) {
    dotnet build "src\HorosPulse.Elevation\HorosPulse.Elevation.csproj" -c $Configuration
}

foreach ($file in @(
        "HorosPulse.Elevation.exe",
        "HorosPulse.Elevation.dll",
        "HorosPulse.Elevation.deps.json",
        "HorosPulse.Elevation.runtimeconfig.json")) {
    Copy-Item -Path (Join-Path $elevationDir $file) -Destination (Join-Path $stagingDir $file) -Force
}

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
Write-Host "Portable folder: $stagingDir"
Write-Host "Velopack feed URL (appsettings.json): https://github.com/HorosCloudOfficial/HorosPulse"
Write-Host ""

if ($Velopack) {
    $vpk = Get-Command vpk -ErrorAction SilentlyContinue
    if (-not $vpk) {
        Write-Warning "Velopack CLI (vpk) not found. Install with: dotnet tool install -g vpk"
    }
    else {
        if (Test-Path $velopackOutDir) {
            Remove-Item $velopackOutDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $velopackOutDir -Force | Out-Null

        Write-Host "Packaging Velopack installer to $velopackOutDir ..."
        vpk pack `
            -p $stagingDir `
            -o $velopackOutDir `
            --packId HorosPulse `
            --packVersion $version `
            --packTitle "HorosPulse" `
            --mainExe HorosPulse.App.exe

        Write-Host "Velopack output: $velopackOutDir"
        Write-Host "Upload releases.*.json and assets to GitHub Releases for auto-update."
    }
}
else {
    Write-Host "Installer (Velopack):"
    Write-Host "  dotnet tool install -g vpk"
    Write-Host "  .\publish.ps1 -Velopack"
}
