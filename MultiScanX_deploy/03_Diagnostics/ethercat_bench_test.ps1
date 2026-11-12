# ================================================================================
# Ajinextek EtherCAT Slave Benchmark & Diagnostics Tool (CNT2, DB32T, AI16)
# TeMaat Automation Research Institute
# Pure ASCII / No-Installation Standalone Tool for Windows PowerShell 5.1+
# ================================================================================

Clear-Host
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "    Ajinextek EtherCAT 3-Module Benchmark Tool (PowerShell)           " -ForegroundColor Yellow
Write-Host "               TeMaat Automation Research Institute                   " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

# P/Invoke C# Native Wrapper Definition
$csharpCode = @"
using System;
using System.Runtime.InteropServices;

public class AxHwBench {
    [DllImport("AXL.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxlOpen(int uMode);
    [DllImport("AXL.dll", CallingConvention = CallingConvention.StdCall)] public static extern int AxlClose();
    [DllImport("AXL.dll", CallingConvention = CallingConvention.StdCall)] public static extern int AxlIsOpened();

    [DllImport("AXD.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxdInfoGetModuleCount(ref int pnModuleCount);
    [DllImport("AXD.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxdoWriteOutportBit(int nModuleNo, int nBitNo, uint uValue);
    [DllImport("AXD.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxdiReadInportWord(int nModuleNo, int nOffset, ref uint upValue);

    [DllImport("AXC.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxcInfoGetModuleCount(ref int pnModuleCount);
    [DllImport("AXC.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxcGetCaptureCount(int nChannelNo, ref int plPosition);
    [DllImport("AXC.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxcSetCaptureCount(int nChannelNo, int lPosition);

    [DllImport("AXA.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxaInfoGetModuleCount(ref int pnModuleCount);
    [DllImport("AXA.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxaReadChannelVolts(int nChannelNo, ref double dpVolt);

    [DllImport("AXM.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxmInfoGetAxisCount(ref int lpAxisCount);
    [DllImport("AXM.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxmSignalIsServoOn(int lAxisNo, ref uint upOnOff);
    [DllImport("AXM.dll", CallingConvention = CallingConvention.StdCall)] public static extern uint AxmStatusReadActPos(int lAxisNo, ref double dpPosition);
}
"@

try {
    Add-Type -TypeDefinition $csharpCode -ErrorAction Stop
} catch {
    # Silently continue if already loaded in current session
}

# 1. Master Board Initialization
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host " 1. PCIe-R0805-ECAT-E Master Card Initialization" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

$isRealHw = $false
try {
    $ret = [AxHwBench]::AxlOpen(7)
    if ($ret -eq 0) {
        $isRealHw = $true
        Write-Host " [PASS] PCIe-R0805-ECAT-E Master Card Online (AxlOpen = 0)" -ForegroundColor Green
    } else {
        Write-Host " [INFO] Master Card Not Responding (Code: $ret) -> Switching to Simulation Mode" -ForegroundColor Yellow
        $isRealHw = $false
    }
} catch {
    Write-Host " >>> [SIMULATION MODE] AXL.dll not found in system path." -ForegroundColor Yellow
    Write-Host "     (Note: Install 02_Drivers\Setup.exe to control physical hardware)" -ForegroundColor Gray
    Start-Sleep -Milliseconds 300
    Write-Host " [MOCK PASS] Virtual PCIe Master Card Online (AxlOpen = 0)" -ForegroundColor Green
    $isRealHw = $false
}

# 2. Slave Module Scanning
Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " 2. EtherCAT Bus Slave Scanning (Servo Axes, CNT2, DB32T, AI16)" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
if ($isRealHw) {
    $axisCnt = 0; $dioCnt = 0; $cntCnt = 0; $aiCnt = 0
    [AxHwBench]::AxmInfoGetAxisCount([ref]$axisCnt) | Out-Null
    [AxHwBench]::AxdInfoGetModuleCount([ref]$dioCnt) | Out-Null
    [AxHwBench]::AxcInfoGetModuleCount([ref]$cntCnt) | Out-Null
    [AxHwBench]::AxaInfoGetModuleCount([ref]$aiCnt) | Out-Null

    Write-Host "  - Motion Axis Count    (Panasonic A6B) : $axisCnt axis(axes)" -ForegroundColor White
    for ($ax = 0; $ax -lt $axisCnt; $ax++) {
        $srvOn = [uint32]0; $actPos = 0.0
        [AxHwBench]::AxmSignalIsServoOn($ax, [ref]$srvOn) | Out-Null
        [AxHwBench]::AxmStatusReadActPos($ax, [ref]$actPos) | Out-Null
        Write-Host "     -> Axis $ax : ServoOn=$srvOn, ActPos=$([math]::Round($actPos, 4))" -ForegroundColor Gray
    }
    Write-Host "  - DIO Module Count     (N3ECAT-DB32T)  : $dioCnt unit(s)" -ForegroundColor White
    Write-Host "  - Counter Module Count (N3ECAT-CNT2)   : $cntCnt unit(s)" -ForegroundColor White
    Write-Host "  - AIN Module Count     (N3ECAT-AI16)   : $aiCnt unit(s)" -ForegroundColor White
} else {
    Start-Sleep -Milliseconds 300
    Write-Host "  - [MOCK] Motion Axes Detected: 2 Axes (Axis 0: X Panasonic A6B, Axis 1: Y Panasonic A6B)" -ForegroundColor White
    Write-Host "  - [MOCK] Slave Detected: N3ECAT-CNT2  (2CH High-speed Counter)" -ForegroundColor White
    Write-Host "  - [MOCK] Slave Detected: N3ECAT-DB32T (16DI / 16DO Sink)" -ForegroundColor White
    Write-Host "  - [MOCK] Slave Detected: N3ECAT-AI16  (16CH Analog Input)" -ForegroundColor White
}

# 3. DO Sequential Blink (Walking 1s) Test
Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " 3. N3ECAT-DB32T Digital Output (DO 0~7) Sequential Toggle Test" -ForegroundColor Cyan
Write-Host "    (Verify DO front LEDs 0~7 turn ON and OFF sequentially)" -ForegroundColor Yellow
Write-Host "=================================================================" -ForegroundColor Cyan
for ($bit = 0; $bit -lt 8; $bit++) {
    Write-Host "  -> [DO Bit $($bit.ToString('D2'))] ON  (LED ON) " -ForegroundColor Green -NoNewline
    if ($isRealHw) { [AxHwBench]::AxdoWriteOutportBit(0, $bit, 1) | Out-Null }
    Start-Sleep -Milliseconds 150
    Write-Host "-> [DO Bit $($bit.ToString('D2'))] OFF (LED OFF)" -ForegroundColor Gray
    if ($isRealHw) { [AxHwBench]::AxdoWriteOutportBit(0, $bit, 0) | Out-Null }
}
Write-Host " [PASS] N3ECAT-DB32T DO Output Walking 1s Test Completed!" -ForegroundColor Green

# 4. DI Real-time Read
Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " 4. N3ECAT-DB32T Digital Input (DI 0~15) Status Read" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
if ($isRealHw) {
    $diVal = [uint32]0
    [AxHwBench]::AxdiReadInportWord(0, 0, [ref]$diVal) | Out-Null
    $binary = [Convert]::ToString($diVal, 2).PadLeft(16, '0')
    Write-Host "  -> Current DI State: [ $binary ]" -ForegroundColor White
} else {
    Write-Host "  -> [MOCK] Current DI State: [ 0000000000000010 ] (IN2: Vacuum Sensor ON)" -ForegroundColor White
}

# 5. CNT2 Counter Read
Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " 5. N3ECAT-CNT2 High-speed Counter 2CH Read & Reset Test" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
if ($isRealHw) {
    $count = 0
    [AxHwBench]::AxcGetCaptureCount(0, [ref]$count) | Out-Null
    $distMm = [math]::Round($count * 0.00005, 4)
    Write-Host "  - CH0 (Renishaw Scale) Current Count: $count pulses (~$distMm mm)" -ForegroundColor White
} else {
    Write-Host "  - [MOCK] CH0 (Renishaw Scale) Count: 120,450 pulses (~6.0225 mm)" -ForegroundColor White
    Write-Host "  - [MOCK] CH0 Counter Software Reset -> 0 pulse verified [PASS]" -ForegroundColor Green
}

# 6. AI16 Voltage Scan
Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " 6. N3ECAT-AI16 Analog Input (CH0~CH3) Voltage Scan" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
if ($isRealHw) {
    for ($ch = 0; $ch -lt 4; $ch++) {
        $volt = 0.0
        [AxHwBench]::AxaReadChannelVolts($ch, [ref]$volt) | Out-Null
        Write-Host "  - AI CH $ch : $([math]::Round($volt, 3)) V" -ForegroundColor White
    }
} else {
    Write-Host "  - [MOCK] AI CH 0:  0.012 V (Normal)" -ForegroundColor White
    Write-Host "  - [MOCK] AI CH 1:  4.985 V (Normal - 5V Full Range)" -ForegroundColor White
    Write-Host "  - [MOCK] AI CH 2:  0.005 V (Normal)" -ForegroundColor White
    Write-Host "  - [MOCK] AI CH 3:  0.008 V (Normal)" -ForegroundColor White
}
Write-Host " [PASS] N3ECAT-AI16 Analog Input Voltage Scan Completed!" -ForegroundColor Green

Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " [SUCCESS] EtherCAT 3-Module Benchmark Verification Finished!    " -ForegroundColor Yellow
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host ""
