@echo off
chcp 65001 >nul
title MultiScan-X Studio - .NET 8 Desktop Runtime Auto Installer
echo ============================================================================
echo   MultiScan-X Studio 실행을 위한 Microsoft .NET 8 Desktop Runtime (x64) 설치
echo ============================================================================
echo.
echo [1/2] Windows 패키지 매니저(winget)를 통한 자동 설치를 시도합니다...
winget install --id Microsoft.DotNet.DesktopRuntime.8 --architecture x64 --accept-package-agreements --accept-source-agreements
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [완료] .NET 8 Desktop Runtime 설치가 완료되었습니다!
    echo 이제 MultiScanX_LayoutC_FloatingStudio.exe 등 실행 파일을 더블클릭해 주세요.
    pause
    exit /b 0
)

echo.
echo [2/2] 공식 설치 파일을 직접 다운로드하여 실행합니다...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$url='https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe'; $out=Join-Path $env:TEMP 'windowsdesktop-runtime-win-x64.exe'; Write-Host '다운로드 중...'; Invoke-WebRequest -Uri $url -OutFile $out; Start-Process -FilePath $out -Wait"
echo.
echo [완료] 설치 마법사가 종료되었습니다. 이제 GUI 실행 파일을 실행해 주세요.
pause

