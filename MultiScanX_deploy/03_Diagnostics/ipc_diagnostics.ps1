﻿<#
================================================================================
MultiScanX 산업용 PC (Neousys Nuvo-9002E) 무설치 통합 환경 진단 도구
제조사: (주)티마트 기술연구소 (TeMaat)
용도: Python/추가 툴 설치 없이 윈도우 순정 환경에서 하드웨어, 드라이브, 네트워크, AXL 검증
================================================================================
#>

# 한글 콘솔 인코딩 UTF-8 강제 지정 (글자 깨짐 방지)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

Clear-Host
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "   🌟 MultiScanX 산업용 PC (Nuvo-9002E) 순정 환경 진단 도구 🌟        " -ForegroundColor Yellow
Write-Host "             (주)티마트 기술연구소 (TeMaat Automation)                 " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

# 1. OS 및 시스템 사양
Write-Host "[1] 운영체제 및 플랫폼 점검" -ForegroundColor Green
$os = Get-CimInstance Win32_OperatingSystem
$cpu = Get-CimInstance Win32_Processor
Write-Host "  - OS: $($os.Caption) (버전: $($os.Version), 빌드: $($os.BuildNumber))"
Write-Host "  - 아키텍처: $($os.OSArchitecture)"
Write-Host "  - 컴퓨터 이름: $env:COMPUTERNAME"
Write-Host "  - CPU: $($cpu.Name) ($($cpu.NumberOfCores) Cores / $($cpu.NumberOfLogicalProcessors) Threads)"

# 2. 스토리지 드라이브 구성 점검 (C: NVMe 500GB / D: HDD 2TB)
Write-Host "`n[2] 스토리지 드라이브 구성 점검" -ForegroundColor Green
$drives = Get-PSDrive -PSProvider FileSystem
foreach ($d in $drives) {
    if ($d.Name -in @("C", "D")) {
        $totalGb = [math]::Round(($d.Used + $d.Free) / 1GB, 1)
        $freeGb = [math]::Round($d.Free / 1GB, 1)
        Write-Host "  - [$($d.Name):] 드라이브: 정상 감지 (전체: $totalGb GB, 여유: $freeGb GB)" -ForegroundColor White
    }
}
$dDrive = Get-PSDrive -Name "D" -ErrorAction SilentlyContinue
if (-not $dDrive) {
    Write-Host "  - [D:] 드라이브: [미인식 또는 미생성] -> (주의: 2TB 데이터용 D드라이브 확인 필요)" -ForegroundColor Red
}

# 3. 네트워크 IP 상태
Write-Host "`n[3] 네트워크 어댑터 IP 상태" -ForegroundColor Green
$adapters = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike "*Loopback*" }
foreach ($nic in $adapters) {
    Write-Host "  - 어댑터: $($nic.InterfaceAlias) | IP 주소: $($nic.IPAddress)" -ForegroundColor White
}

# 4. .NET 데스크톱 런타임 점검
Write-Host "`n[4] .NET 8 데스크톱 런타임 설치 점검" -ForegroundColor Green
try {
    $runtimes = & dotnet --list-runtimes 2>$null
    if ($runtimes -match "Microsoft.WindowsDesktop.App 8") {
        Write-Host "  - [PASS] .NET 8 데스크톱 런타임 정상 감지 완료!" -ForegroundColor Green
    } else {
        Write-Host "  - [안내] .NET 8 데스크톱 런타임 미설치 (01_Runtimes 폴더의 인스톨러를 실행하세요)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  - [안내] dotnet 명령어를 찾을 수 없습니다." -ForegroundColor Yellow
}

# 5. 아진엑스텍 제어 드라이버 (AXL.dll) 점검
Write-Host "`n[5] 아진엑스텍 EtherCAT 제어 라이브러리 (AXL.dll) 연동 점검" -ForegroundColor Green
$axlPath = "AXL.dll"
$loaded = $false
try {
    # C# P/Invoke 래퍼 동적 컴파일 및 AxlOpen 호출
    $csharpCode = @"
    using System;
    using System.Runtime.InteropServices;
    public class AxChecker {
        [DllImport("AXL.dll")]
        public static extern uint AxlOpen(int uMode);
        [DllImport("AXL.dll")]
        public static extern int AxlIsOpened();
    }
"@
    Add-Type -TypeDefinition $csharpCode -ErrorAction Stop
    $isOpen = [AxChecker]::AxlIsOpened()
    Write-Host "  - [PASS] AXL.dll 라이브러리 로드 성공! (EzSoftwareUC 정상 설치됨)" -ForegroundColor Green
    Write-Host "  - AxlIsOpened 상태: $isOpen" -ForegroundColor White
    $loaded = $true
} catch {
    Write-Host "  - [안내] AXL.dll 미감지 (시뮬레이션 모드 또는 EzSoftwareUC 설치 전 상태)" -ForegroundColor Yellow
    Write-Host "    -> 실물 마스터 카드(PCIe-R0805-ECAT-E) 장착 후 02_Drivers\Setup.exe를 설치하십시오." -ForegroundColor Gray
}

Write-Host "`n======================================================================" -ForegroundColor Cyan
Write-Host "  🎉 산업용 PC 환경 점검 완료! MultiScanX 제어 준비가 가능합니다." -ForegroundColor Yellow
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

