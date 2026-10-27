# scripts/publish-windows-local.ps1
param(
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

# Guard against running outside of a script file
if (-not $PSScriptRoot) {
    Write-Error "This script must be executed as a file (e.g. .\scripts\publish-windows-local.ps1), not run line-by-line."
    exit 1
}

# Inlined Configuration Settings
$Configuration = "Release"
$CertFriendlyName = "Newt Dev Cert"

# Resolve repo root and paths relative to script location
$RepoRoot = (Get-Item "$PSScriptRoot\..").FullName
$AppDir = [System.IO.Path]::Combine($RepoRoot, "src", "NetWorthTracker.App")
$ProjectPath = [System.IO.Path]::Combine($AppDir, "NetWorthTracker.App.csproj")
$ManifestPath = [System.IO.Path]::Combine($AppDir, "Platforms", "Windows", "Package.appxmanifest")

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Could not find project file at: $ProjectPath"
    exit 1
}

# 1. Auto-detect TargetFramework from .csproj
$Framework = ""
[xml]$csproj = Get-Content $ProjectPath
$tfmNodes = $csproj.SelectNodes("//TargetFrameworks | //TargetFramework")
foreach ($node in $tfmNodes) {
    $tfms = $node.InnerText -split ';'
    $winTfm = $tfms | Where-Object { $_ -like "*windows*" } | Select-Object -First 1
    if ($winTfm) {
        $Framework = $winTfm.Trim()
        break
    }
}
if ([string]::IsNullOrWhiteSpace($Framework)) {
    $Framework = "net9.0-windows10.0.19041.0" # Fallback
}

# 2. Auto-detect Publisher Name and Handle Patch Version Increment
$PublisherName = ""
if (Test-Path $ManifestPath) {
    [xml]$manifest = Get-Content $ManifestPath
    $rawPublisher = $manifest.Package.Identity.Publisher
    if (-not [string]::IsNullOrWhiteSpace($rawPublisher)) {
        $PublisherName = $rawPublisher
    }

    # Patch Version Bump Prompt
    $currentVersion = $manifest.Package.Identity.Version
    Write-Host "Current Package Version: $currentVersion" -ForegroundColor Cyan
    $response = Read-Host "Do you want to increment the patch version? (y/N)"

    if ($response -eq 'y' -or $response -eq 'Y') {
        $versionParts = $currentVersion.Split('.')
        if ($versionParts.Count -eq 4) {
            $patch = [int]$versionParts[3] + 1
            $newVersion = "$($versionParts[0]).$($versionParts[1]).$($versionParts[2]).$patch"
            $manifest.Package.Identity.Version = $newVersion
            $manifest.Save($ManifestPath)
            Write-Host "Updated Version to: $newVersion`n" -ForegroundColor Green
        } else {
            Write-Host "WARNING: Version string format invalid ($currentVersion). Skipping version bump.`n" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Keeping current version: $currentVersion`n" -ForegroundColor Gray
    }
}

if ([string]::IsNullOrWhiteSpace($PublisherName)) {
    $PublisherName = "CN=GLHF" # Fallback
}

# Ensure Subject follows "CN=..." format required by New-SelfSignedCertificate
$SubjectDN = if ($PublisherName -like "CN=*") { $PublisherName } else { "CN=$PublisherName" }

Write-Host "Target Framework: $Framework" -ForegroundColor Cyan
Write-Host "Publisher Subject: $SubjectDN`n" -ForegroundColor Cyan

# 3. Clean Previous Build Artifacts (bin & obj) if -Clean switch provided
if ($Clean) {
    Write-Host "Cleaning previous build artifacts (bin & obj)..." -ForegroundColor Cyan

    foreach ($folder in @("bin", "obj")) {
        $folderPath = [System.IO.Path]::Combine($AppDir, $folder)
        if (Test-Path $folderPath) {
            try {
                Remove-Item -Path $folderPath -Recurse -Force
                Write-Host "  Successfully deleted: $folder\" -ForegroundColor Green
            } catch {
                Write-Host "  WARNING: Could not completely delete $folder\. Some files may be locked by Rider/IDE." -ForegroundColor Yellow
            }
        }
    }
} else {
    # If not cleaning everything, ensure older MSIX outputs are cleaned up to avoid deploying an outdated package
    $packagesDir = [System.IO.Path]::Combine($AppDir, "bin", $Configuration, $Framework, "win10-x64", "AppPackages")
    if (Test-Path $packagesDir) {
        try {
            Remove-Item -Path $packagesDir -Recurse -Force
            Write-Host "Cleaned previous MSIX package output folder." -ForegroundColor Gray
        } catch {
            Write-Host "WARNING: Could not clean AppPackages directory." -ForegroundColor Yellow
        }
    }
}

Write-Host ""

# 4. Check, Create, and Auto-Trust Certificate
Write-Host "Checking for existing certificate ('$CertFriendlyName')..." -ForegroundColor Cyan

$cert = Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object { $_.FriendlyName -eq $CertFriendlyName } | Select-Object -First 1

if ($null -eq $cert) {
    Write-Host "Certificate not found. Creating a new self-signed certificate..." -ForegroundColor Yellow

    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $SubjectDN `
        -KeyUsage DigitalSignature `
        -FriendlyName $CertFriendlyName `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
} else {
    Write-Host "Found existing certificate: $($cert.Subject)" -ForegroundColor Green
}

# Ensure .cer file exists on disk
$cerPath = [System.IO.Path]::Combine($PSScriptRoot, "$CertFriendlyName.cer")
if (-not (Test-Path $cerPath)) {
    Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
    Write-Host "Exported public key to: $cerPath" -ForegroundColor Green
}

# Auto-trust certificate into LocalMachine\TrustedPeople using .NET X509Store
try {
    $store = [System.Security.Cryptography.X509Certificates.X509Store]::new("TrustedPeople", "LocalMachine")
    $store.Open("ReadWrite")
    $store.Add($cert)
    $store.Close()
    Write-Host "Certificate trusted in LocalMachine\TrustedPeople store." -ForegroundColor Green
} catch {
    Write-Host "WARNING: Could not auto-trust certificate to LocalMachine\TrustedPeople store (Admin required)." -ForegroundColor Yellow
    Write-Host "If installation fails, run Rider/Terminal as Administrator once to establish machine trust." -ForegroundColor Yellow
}

$thumbprint = $cert.Thumbprint
Write-Host "Certificate Thumbprint: $thumbprint`n" -ForegroundColor Yellow

# 5. Build and Package NetWorthTracker.App
Write-Host "Building and signing MSIX package for NetWorthTracker.App ($Configuration)..." -ForegroundColor Cyan

dotnet publish "$ProjectPath" `
    -f $Framework `
    -c $Configuration `
    /p:WindowsPackageType=MSIX `
    /p:GenerateAppxPackageOnBuild=true `
    /p:AppxPackageSigningEnabled=true `
    /p:PackageCertificateThumbprint="$thumbprint"

# 6. Launch MSIX Installer
if ($LASTEXITCODE -eq 0) {
    $outputPath = [System.IO.Path]::Combine($AppDir, "bin", $Configuration, $Framework, "win10-x64", "AppPackages")
    Write-Host "`nPublish complete! MSIX package folder:" -ForegroundColor Green
    Write-Host $outputPath -ForegroundColor Yellow

    # Find the newly generated .msix file
    $msixFile = Get-ChildItem -Path $outputPath -Filter "*.msix" -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    if ($msixFile) {
        Write-Host "`nLaunching installer dialog: $($msixFile.Name)..." -ForegroundColor Cyan
        Invoke-Item $msixFile.FullName
    } else {
        Write-Host "WARNING: Could not find .msix package file inside $outputPath" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nPublish failed with exit code $LASTEXITCODE" -ForegroundColor Red
}
