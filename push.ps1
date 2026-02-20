# Algoritma Uzmanı - Otomatik Version Artırma ve Push Script
# Kullanım: .\push.ps1 "commit mesajı"
# Veya: .\push.ps1 -Major "commit mesajı" (major version artırır: 1.0.0 -> 2.0.0)
# Veya: .\push.ps1 -Minor "commit mesajı" (minor version artırır: 1.0.0 -> 1.1.0)
# Veya: .\push.ps1 (patch version artırır: 1.0.0 -> 1.0.1)

param(
    [string]$CommitMessage = "",
    [switch]$BumpMajor,
    [switch]$BumpMinor
)

$ErrorActionPreference = "Stop"

# appsettings.json dosyasının yolu
$settingsPath = "AlgoritmaUzmani/appsettings.json"

# Mevcut version'ı oku
$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
$currentVersion = $settings.AppSettings.Version

if (-not $currentVersion) {
    $currentVersion = "1.0.0"
}

Write-Host "Mevcut surum: v$currentVersion" -ForegroundColor Cyan

# Version'ı parçala
$versionParts = $currentVersion.Split('.')
$majorNum = [int]$versionParts[0]
$minorNum = [int]$versionParts[1]
$patchNum = [int]$versionParts[2]

# Version'ı artır
if ($BumpMajor) {
    $majorNum++
    $minorNum = 0
    $patchNum = 0
    Write-Host "Major surum artiriliyor..." -ForegroundColor Yellow
} elseif ($BumpMinor) {
    $minorNum++
    $patchNum = 0
    Write-Host "Minor surum artiriliyor..." -ForegroundColor Yellow
} else {
    $patchNum++
    Write-Host "Patch surum artiriliyor..." -ForegroundColor Yellow
}

$newVersion = "$majorNum.$minorNum.$patchNum"
Write-Host "Yeni surum: v$newVersion" -ForegroundColor Green

# appsettings.json'u güncelle
$settings.AppSettings.Version = $newVersion
$settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8

Write-Host "appsettings.json guncellendi" -ForegroundColor Green

# Git işlemleri
git add -A

if ($CommitMessage -eq "") {
    $CommitMessage = "v$newVersion - Guncelleme"
}

Write-Host "`nCommit mesaji: $CommitMessage" -ForegroundColor Cyan

git commit -m "$CommitMessage"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Commit basarisiz oldu!" -ForegroundColor Red
    exit 1
}

git push

if ($LASTEXITCODE -ne 0) {
    Write-Host "Push basarisiz oldu!" -ForegroundColor Red
    exit 1
}

Write-Host "`n Push tamamlandi! Yeni surum: v$newVersion" -ForegroundColor Green
