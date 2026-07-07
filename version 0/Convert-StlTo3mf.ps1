param(
    [Parameter(Mandatory)]
    [string]$SourcePath,

    [string]$PrusaSlicerPath = "C:\Program Files\Prusa3D\PrusaSlicer\prusa-slicer-console.exe",

    [string]$OutputRoot,

    [switch]$Force
)

$script:exePath = $PrusaSlicerPath

if (-not (Test-Path $exePath)) {
    Write-Error "PrusaSlicer not found at: $exePath"
    exit 1
}

if (-not (Test-Path $SourcePath)) {
    Write-Error "Source path not found: $SourcePath"
    exit 1
}

$SourcePath = (Resolve-Path $SourcePath).Path

if (-not $OutputRoot) {
    $OutputRoot = Join-Path $SourcePath "3mf_output"
}

$stlFiles = Get-ChildItem -Path $SourcePath -Recurse -Filter "*.stl"
$total = $stlFiles.Count
$success = 0
$failed = 0

Write-Host "Found $total STL files. Converting..."

for ($i = 0; $i -lt $total; $i++) {
    $stl = $stlFiles[$i]
    $pct = [math]::Round(($i + 1) / $total * 100)

    $relative = $stl.DirectoryName.Substring($SourcePath.Length).TrimStart("\")
    $outDir = if ($relative) { Join-Path $OutputRoot $relative } else { $OutputRoot }

    $outFile = Join-Path $outDir "$([System.IO.Path]::GetFileNameWithoutExtension($stl.Name)).3mf"

    if ((-not $Force) -and (Test-Path $outFile)) {
        Write-Host "[$pct%] Skipping (already exists): $($stl.Name)"
        continue
    }

    Write-Host "[$pct%] Converting: $($stl.Name)"

    New-Item -ItemType Directory -Path $outDir -Force | Out-Null

    $proc = Start-Process -FilePath $exePath -ArgumentList @(
        "--export-3mf",
        "--dont-arrange",
        "--output", "`"$outFile`"",
        "`"$($stl.FullName)`""
    ) -NoNewWindow -Wait -PassThru

    if ($proc.ExitCode -eq 0) {
        $success++
    } else {
        Write-Warning "Failed (exit $($proc.ExitCode)): $($stl.Name)"
        $failed++
    }
}

Write-Host "`nDone. $success converted, $failed failed, $($total - $success - $failed) skipped."
