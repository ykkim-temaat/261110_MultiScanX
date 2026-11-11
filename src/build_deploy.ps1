# ================================================================================
# MultiScanX - Dual-Track Build & Installer Generator
#   Track 1 (Fast Test/Patch) : src\Release_Bin\MultiScanX.App.exe (~220 KB)
#   Track 2 (Official Setup)  : MultiScanX_deploy\04_App_Release\MultiScanX_Setup_v1.0.0.exe
# TeMaat Automation Research Institute
# ================================================================================

param(
    [switch]$PatchOnly,   # Skip Inno Setup installer build (generates fast test EXE only)
    [switch]$Standalone,  # Embed .NET 8 Runtime (~154MB) for offline target without runtime
    [string]$Version = "" # Empty = auto-read <Version> from MultiScanX.App.csproj
)

$ErrorActionPreference = "Stop"

# Resolve paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Resolve-Path "$ScriptDir/.."
$CsprojPath = Join-Path $ProjectRoot "src/MultiScanX/MultiScanX.App/MultiScanX.App.csproj"

# Auto-resolve version and release note from .csproj (SSOT)
[xml]$csprojXml = Get-Content -Path $CsprojPath -Encoding UTF8
if ([string]::IsNullOrWhiteSpace($Version)) {
    $csprojVer = $csprojXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    $Version = if (-not [string]::IsNullOrWhiteSpace($csprojVer)) { $csprojVer.Trim() } else { "1.0.0" }
}
$csprojDesc = $csprojXml.Project.PropertyGroup.Description | Where-Object { $_ } | Select-Object -First 1
$ReleaseNote = if (-not [string]::IsNullOrWhiteSpace($csprojDesc)) { $csprojDesc.Trim() } else { "(none)" }

$ReleaseBinDir = Join-Path $ScriptDir "Release_Bin"
$PatchExe = Join-Path $ReleaseBinDir "MultiScanX.App.exe"
$TempPublishDir = Join-Path $ProjectRoot "src/MultiScanX/MultiScanX.App/bin/PublishTemp"
$IssPath = Join-Path $ScriptDir "MultiScanX_Installer.iss"
$DeployDir = Join-Path $ProjectRoot "MultiScanX_deploy/04_App_Release"
$SetupExe = Join-Path $DeployDir "MultiScanX_Setup_v${Version}.exe"

Clear-Host
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "    MultiScanX Studio - Dual-Track Build & Installer Pipeline v$Version" -ForegroundColor Yellow
Write-Host "              TeMaat Automation Research Institute                    " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

$modeStr = if ($Standalone) { "Standalone All-in-One (~154 MB)" } else { "Lightweight Production (~220 KB, Runtime Powered)" }
$selfContained = if ($Standalone) { "true" } else { "false" }

Write-Host " [Build Configuration]" -ForegroundColor White
Write-Host "  - Runtime Mode  : $modeStr" -ForegroundColor Yellow
Write-Host "  - Release Note  : $ReleaseNote" -ForegroundColor Cyan
Write-Host "  - Track 1 (Bin) : $PatchExe" -ForegroundColor Gray
if (-not $PatchOnly) {
    Write-Host "  - Track 2 (Setup): $SetupExe" -ForegroundColor Gray
}
Write-Host ""

# --------------------------------------------------------------------------------
# Step 1: Compile Single EXE Binary (Track 1: Fast Test / Patch)
# --------------------------------------------------------------------------------
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host " [Step 1] Compiling Core Application Binary (dotnet publish)...  " -ForegroundColor Cyan
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

$sw = [System.Diagnostics.Stopwatch]::StartNew()
& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] dotnet publish failed with exit code $LASTEXITCODE." -ForegroundColor Red
    exit 1
}

$tempExe = Join-Path $TempPublishDir "MultiScanX.App.exe"
if (-not (Test-Path $tempExe)) {
    Write-Host "`n[ERROR] Compiled binary not found at $tempExe" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $ReleaseBinDir)) {
    New-Item -ItemType Directory -Path $ReleaseBinDir -Force | Out-Null
}
Copy-Item -Path $tempExe -Destination $PatchExe -Force

$exeItem = Get-Item $PatchExe
$binSize = if ($exeItem.Length -lt 1MB) { "$([math]::Round($exeItem.Length / 1KB, 1)) KB" } else { "$([math]::Round($exeItem.Length / 1MB, 2)) MB" }
Write-Host "[PASS] Track 1 (Test/Patch EXE) Ready: $binSize ($($sw.Elapsed.TotalSeconds.ToString('F1'))s)" -ForegroundColor Green

if ($PatchOnly) {
    Write-Host "`n=================================================================" -ForegroundColor Green
    Write-Host " [SUCCESS] Fast Patch Binary Generated!                          " -ForegroundColor Yellow
    Write-Host "  -> Copy '$PatchExe' to IPC 'C:\MultiScanX\bin\' for quick test." -ForegroundColor Cyan
    Write-Host "=================================================================" -ForegroundColor Green
    exit 0
}

# --------------------------------------------------------------------------------
# Step 2: Compile Windows Installer Setup.exe (Track 2: Official Setup)
# --------------------------------------------------------------------------------
Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " [Step 2] Packaging Official Installer (Inno Setup 6)...         " -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

# Auto-detect ISCC.exe
$isccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "${env:LocalAppData}\Programs\Inno Setup 6\ISCC.exe"
)
$isccExe = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $isccExe) {
    $cmd = Get-Command iscc -ErrorAction SilentlyContinue
    if ($cmd) { $isccExe = $cmd.Source }
}

if (-not $isccExe) {
    Write-Host "[ERROR] Inno Setup 6 (ISCC.exe) not found. Please install Inno Setup 6 or run with -PatchOnly." -ForegroundColor Red
    exit 1
}

& $isccExe "/DMyAppVersion=$Version" $IssPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] Inno Setup compilation failed with exit code $LASTEXITCODE." -ForegroundColor Red
    exit 1
}

$sw.Stop()
$setupItem = Get-Item $SetupExe
$setupSize = if ($setupItem.Length -lt 1MB) { "$([math]::Round($setupItem.Length / 1KB, 1)) KB" } else { "$([math]::Round($setupItem.Length / 1MB, 2)) MB" }

Write-Host "`n=================================================================" -ForegroundColor Green
Write-Host " [SUCCESS] Dual-Track Build & Installer Packaging Complete!      " -ForegroundColor Yellow
Write-Host "=================================================================" -ForegroundColor Green
Write-Host "  1. [Fast Test/Patch]  : $PatchExe ($binSize)" -ForegroundColor White
Write-Host "     -> Copy directly to IPC 'C:\MultiScanX\bin\' during testing." -ForegroundColor Gray
Write-Host "  2. [Official Setup]   : $SetupExe ($setupSize)" -ForegroundColor Green
Write-Host "     -> Double-click on IPC to install & create Desktop shortcut." -ForegroundColor Gray
Write-Host "  - Total Elapsed Time  : $($sw.Elapsed.TotalSeconds.ToString('F1')) seconds" -ForegroundColor White
Write-Host ""
