@echo off
chcp 65001 >nul
title MultiScanX - Ajinextek EtherCAT 3-Module & Servo Benchmark
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0ethercat_bench_test.ps1"
echo.
pause
