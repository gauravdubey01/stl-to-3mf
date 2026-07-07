# Build MSIX package for Microsoft Store upload
# Uses raw binary ZIP with proper OPC structure
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppName = "StlTo3mfConverter"
$Version = "2.5.0.0"
$Csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"

Write-Host "==> Building MSIX package..." -ForegroundColor Green

& $Csc /reference:"System.IO.Compression.dll" /reference:"System.IO.Compression.FileSystem.dll" /out:"$ScriptDir\BuildMsix.exe" "$ScriptDir\BuildMsix.cs" 2>&1
if (-not $?) { Write-Error "Compilation failed."; exit 1 }

& "$ScriptDir\BuildMsix.exe"
Remove-Item "$ScriptDir\BuildMsix.exe" -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  READY FOR STORE UPLOAD" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
$uploadPath = "$ScriptDir\$AppName-$Version.msixupload"
Write-Host "  Upload file: $uploadPath" -ForegroundColor Cyan
Write-Host "  (or try the .msix directly if .msixupload fails)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Go to https://partner.microsoft.com/dashboard" -ForegroundColor White
Write-Host "  2. Upload either: $AppName-$Version.msixupload (preferred) or .msix" -ForegroundColor White
Write-Host "  3. Submit for certification" -ForegroundColor White
