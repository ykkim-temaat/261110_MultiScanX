# ================================================================================
# MultiScanX - Build & Generate Lightweight Deployment Package
# TeMaat Automation Research Institute
# ================================================================================

param(
    # Use -Standalone switch only if target PC does not have .NET 8 Runtime installed (generates ~154MB all-in-one EXE)
    [switch]$Standalone
)

$ErrorActionPreference = "Stop"

# Project root resolution
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path "$ScriptDir/.."
$CsprojPath = Join-Path $ProjectRoot "src/MultiScanX/MultiScanX.App/MultiScanX.App.csproj"
$OutputDir = Join-Path $ProjectRoot "MultiScanX_deploy/04_App_Release/bin"
$TempPublishDir = Join-Path $ProjectRoot "src/MultiScanX/MultiScanX.App/bin/PublishTemp"
$TargetExe = Join-Path $OutputDir "MultiScanX.App.exe"

Clear-Host
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "     MultiScanX Studio - Automatic Build & Deploy Package Generator   " -ForegroundColor Yellow
Write-Host "              TeMaat Automation Research Institute                    " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

$modeStr = if ($Standalone) { "Standalone All-in-One (~154 MB, No Runtime Required)" } else { "Lightweight Production (~220 KB, IPC Runtime Powered) [RECOMMENDED]" }
$selfContained = if ($Standalone) { "true" } else { "false" }

Write-Host " [Configuration]" -ForegroundColor White
Write-Host "  - Target Project : $CsprojPath" -ForegroundColor Gray
Write-Host "  - Deploy Target  : $OutputDir" -ForegroundColor Gray
Write-Host "  - Package Mode   : $modeStr" -ForegroundColor Yellow
Write-Host ""

# --------------------------------------------------------------------------------
# Step 1: Compiling Single EXE (dotnet publish to temp staging)
# --------------------------------------------------------------------------------
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host " Compiling Single EXE Package (dotnet publish)...                " -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

if (Test-Path $TempPublishDir) {
    Remove-Item -Path $TempPublishDir -Recurse -Force -ErrorAction SilentlyContinue
}

$publishArgs = @(
    "publish",
    $CsprojPath,
    "-c", "Release",
    "-r", "win-x64",
    "--self-contained", $selfContained,
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-o", $TempPublishDir
)

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] Build failed with exit code $LASTEXITCODE." -ForegroundColor Red
    exit 1
}

$stopwatch.Stop()
$tempExe = Join-Path $TempPublishDir "MultiScanX.App.exe"
if (-not (Test-Path $tempExe)) {
    Write-Host "`n[ERROR] Output binary not found at $tempExe" -ForegroundColor Red
    exit 1
}

# --------------------------------------------------------------------------------
# Step 2: Copying to Deploy Directory
# --------------------------------------------------------------------------------
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

try {
    # Remove old exe if unlocked
    if (Test-Path $TargetExe) {
        Remove-Item -Path $TargetExe -Force -ErrorAction Stop
    }
    Copy-Item -Path $tempExe -Destination $TargetExe -Force
} catch {
    # If locked by WinSCP/Explorer, try direct overwrite copy
    try {
        Copy-Item -Path $tempExe -Destination $TargetExe -Force -ErrorAction Stop
    } catch {
        Write-Host "`n[WARN] Could not overwrite '$TargetExe' because it is locked by another program (e.g. WinSCP or Explorer)." -ForegroundColor Yellow
        Write-Host "       The new 220KB executable is ready at: $tempExe" -ForegroundColor White
        Write-Host "       Please close or navigate away from the folder in WinSCP and re-run." -ForegroundColor Gray
        exit 0
    }
}

$exeItem = Get-Item $TargetExe
$fileKb = [math]::Round($exeItem.Length / 1KB, 1)
$fileMb = [math]::Round($exeItem.Length / 1MB, 2)
$displaySize = if ($exeItem.Length -lt 1MB) { "$fileKb KB" } else { "$fileMb MB" }

Write-Host "`n=================================================================" -ForegroundColor Green
Write-Host " [SUCCESS] Deployment Package Successfully Generated!            " -ForegroundColor Yellow
Write-Host "=================================================================" -ForegroundColor Green
Write-Host "  - Target File    : $TargetExe" -ForegroundColor White
Write-Host "  - File Size      : $displaySize (Ultra-Lightweight Single File)" -ForegroundColor Green
Write-Host "  - Elapsed Time   : $($stopwatch.Elapsed.TotalSeconds.ToString('F1')) seconds" -ForegroundColor White
Write-Host "  - Target IPC Dir : D:\TeMaat\MultiScanX_deploy\04_App_Release\bin\" -ForegroundColor White
Write-Host "-----------------------------------------------------------------" -ForegroundColor Gray
Write-Host " >>> You can now copy this file to Industrial PC via WinSCP in 0.1s! " -ForegroundColor Cyan
Write-Host ""
