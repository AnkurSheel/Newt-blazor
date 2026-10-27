$CertFriendlyName = "MAUI Dev Cert"

# 1. Search and remove from all CurrentUser and LocalMachine stores
Write-Host "Searching for '$CertFriendlyName' across all certificate stores..." -ForegroundColor Cyan

Get-ChildItem -Path "Cert:\CurrentUser", "Cert:\LocalMachine" -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FriendlyName -eq $CertFriendlyName } |
        ForEach-Object {
            Write-Host "Removing from: $($_.PSPath)" -ForegroundColor Yellow
            Remove-Item -Path $_.PSPath -Force -ErrorAction SilentlyContinue
        }

# 2. Delete the exported .cer file from the scripts directory
$cerFile = "C:\Projects\NetWorthTracker\scripts\$CertFriendlyName.cer"
if (Test-Path $cerFile) {
    Remove-Item -Path $cerFile -Force
    Write-Host "Deleted file: $cerFile" -ForegroundColor Yellow
}

Write-Host "`nCertificate cleanup complete! System is completely clean." -ForegroundColor Green