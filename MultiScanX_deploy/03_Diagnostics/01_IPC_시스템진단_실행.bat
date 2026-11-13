@echo off
chcp 65001 >nul
title MultiScanX - IPC System Diagnostics (Nuvo-9002E)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0ipc_diagnostics.ps1"
echo.
pause

