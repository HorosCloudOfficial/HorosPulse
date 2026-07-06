#Requires -Version 5.1
<#
.SYNOPSIS
    Signs HorosPulse.Elevation.exe with a dev self-signed Authenticode certificate.
.DESCRIPTION
    Creates a code-signing certificate in CurrentUser\My when none exists, then signs
    HorosPulse.Elevation.exe with a self-signed Authenticode certificate for local development.
    CI skips signing when SKIP_SIGNING=1.
#>
param(
    [string]$Configuration = "Debug",
    [string]$CertSubject = "CN=HorosPulse Dev Code Signing"
)

$ErrorActionPreference = "Stop"

if ($env:SKIP_SIGNING -eq "1") {
    Write-Host "SKIP_SIGNING=1 - signing skipped."
    exit 0
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\HorosPulse.Elevation\HorosPulse.Elevation.csproj"
$exePath = Join-Path $repoRoot "src\HorosPulse.Elevation\bin\$Configuration\net9.0\HorosPulse.Elevation.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Building HorosPulse.Elevation ($Configuration)..."
    dotnet build $projectPath -c $Configuration
}

if (-not (Test-Path $exePath)) {
    throw "HorosPulse.Elevation.exe not found at: $exePath"
}

$cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert |
    Where-Object { $_.Subject -eq $CertSubject } |
    Select-Object -First 1

if (-not $cert) {
    Write-Host "Creating dev self-signed code-signing certificate..."
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $CertSubject `
        -CertStoreLocation Cert:\CurrentUser\My `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddYears(5)
}

Write-Host "Signing $exePath with thumbprint $($cert.Thumbprint)..."
$signature = Set-AuthenticodeSignature -FilePath $exePath -Certificate $cert -HashAlgorithm SHA256

if ($signature.Status -ne "Valid" -and $signature.Status -ne "UnknownError") {
    Write-Warning "Signature status: $($signature.Status)"
}

Write-Host "Done. Status: $($signature.Status)"
