param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$projRoot = "E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER"
$srcDesktop = "C:\Users\Gaurav\Desktop"
$srcVersion2 = "$projRoot\version2"
$target = "$projRoot\$Version"

Write-Host "=== Saving version $Version ===" -ForegroundColor Cyan

# Create version folder
New-Item -ItemType Directory -Path $target -Force | Out-Null

# Source files (from desktop, the working copies)
$files = @(
    "StlTo3mfConverter.cs",
    "StlParser.cs",
    "ThreeMfWriter.cs"
)

foreach ($file in $files) {
    $src = "$srcDesktop\$file"
    $name = [System.IO.Path]::GetFileNameWithoutExtension($file)
    $ext = [System.IO.Path]::GetExtension($file)
    $dest = "$target\$name`_$Version$ext"
    Copy-Item $src $dest -Force
    Write-Host "  Saved: $dest" -ForegroundColor Green
}

# Compiled binary
Copy-Item "$srcVersion2\StlTo3mfConverter.exe" "$target\StlTo3mfConverter_$Version.exe" -Force
Write-Host "  Saved: $target\StlTo3mfConverter_$Version.exe" -ForegroundColor Green

# Installer (if exists)
if (Test-Path "$srcVersion2\StlTo3mfConverter_Setup.exe") {
    Copy-Item "$srcVersion2\StlTo3mfConverter_Setup.exe" "$target\StlTo3mfConverter_Setup_$Version.exe" -Force
    Write-Host "  Saved: $target\StlTo3mfConverter_Setup_$Version.exe" -ForegroundColor Green
}

# Inno Setup script (if exists)
if (Test-Path "$srcVersion2\installer.iss") {
    Copy-Item "$srcVersion2\installer.iss" "$target\installer_$Version.iss" -Force
    Write-Host "  Saved: $target\installer_$Version.iss" -ForegroundColor Green
}

# Logo
Copy-Item "$projRoot\3MF.png" "$target\3MF.png" -Force
Write-Host "  Saved: $target\3MF.png" -ForegroundColor Green

# Session record
if (Test-Path "$srcVersion2\SESSION.md") {
    Copy-Item "$srcVersion2\SESSION.md" "$target\SESSION_$Version.md" -Force
    Write-Host "  Saved: $target\SESSION_$Version.md" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Version $Version saved successfully ===" -ForegroundColor Cyan
Write-Host "Location: $target"
