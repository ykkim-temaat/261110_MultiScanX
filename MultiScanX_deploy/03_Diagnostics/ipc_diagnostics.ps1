# ================================================================================
# MultiScanX Industrial PC (Neousys Nuvo-9002E) Diagnostics Tool
# TeMaat Automation Research Institute
# Pure ASCII / No-Installation Standalone Tool for Windows PowerShell 5.1+
# ================================================================================

Clear-Host
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "       MultiScanX Industrial PC (Nuvo-9002E) Diagnostics Tool         " -ForegroundColor Yellow
Write-Host "               TeMaat Automation Research Institute                   " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

# 1. OS & Hardware Specs
Write-Host "[1] Operating System & Hardware Inspection" -ForegroundColor Green
$os = Get-CimInstance Win32_OperatingSystem
$cpu = Get-CimInstance Win32_Processor
Write-Host "  - OS: $($os.Caption) (Version: $($os.Version), Build: $($os.BuildNumber))" -ForegroundColor White
Write-Host "  - Architecture: $($os.OSArchitecture)" -ForegroundColor White
Write-Host "  - Computer Name: $env:COMPUTERNAME" -ForegroundColor White
Write-Host "  - CPU: $($cpu.Name) ($($cpu.NumberOfCores) Cores / $($cpu.NumberOfLogicalProcessors) Threads)" -ForegroundColor White

# 2. Storage Drive Configuration (C: NVMe 500GB / D: HDD 2TB)
Write-Host "`n[2] Storage Drive Configuration" -ForegroundColor Green
$drives = Get-PSDrive -PSProvider FileSystem
foreach ($d in $drives) {
    if ($d.Name -in @("C", "D")) {
        $totalGb = [math]::Round(($d.Used + $d.Free) / 1GB, 1)
        $freeGb = [math]::Round($d.Free / 1GB, 1)
        Write-Host "  - [$($d.Name):] Drive Detected (Total: $totalGb GB, Free: $freeGb GB)" -ForegroundColor White
    }
}
$dDrive = Get-PSDrive -Name "D" -ErrorAction SilentlyContinue
if (-not $dDrive) {
    Write-Host "  - [WARN] D: Drive Not Found! (Please check secondary 2TB HDD)" -ForegroundColor Red
}

# 3. Network Adapters & IPv4
Write-Host "`n[3] Network Adapters & IPv4 Address" -ForegroundColor Green
$adapters = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike "*Loopback*" }
foreach ($nic in $adapters) {
    Write-Host "  - Adapter: $($nic.InterfaceAlias) | IP: $($nic.IPAddress)" -ForegroundColor White
}

# 4. .NET 8 Desktop Runtime Inspection
Write-Host "`n[4] .NET 8 Desktop Runtime Inspection" -ForegroundColor Green
try {
    $runtimes = & dotnet --list-runtimes 2>$null
    if ($runtimes -match "Microsoft.WindowsDesktop.App 8") {
        Write-Host "  - [PASS] .NET 8 Desktop Runtime Successfully Detected!" -ForegroundColor Green
    } else {
        Write-Host "  - [WARN] .NET 8 Desktop Runtime Not Installed! (Run installer in 01_Runtimes)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  - [WARN] 'dotnet' command not found." -ForegroundColor Yellow
}

# 5. Ajinextek Library (AXL.dll) Inspection
Write-Host "`n[5] Ajinextek EtherCAT Master Library (AXL.dll) Inspection" -ForegroundColor Green
try {
    $csharpCode = @"
    using System;
    using System.Runtime.InteropServices;
    public class AxCheck {
        [DllImport("AXL.dll")]
        public static extern uint AxlOpen(int uMode);
        [DllImport("AXL.dll")]
        public static extern int AxlIsOpened();
    }
"@
    Add-Type -TypeDefinition $csharpCode -ErrorAction Stop
    $isOpen = [AxCheck]::AxlIsOpened()
    Write-Host "  - [PASS] AXL.dll Library Loaded Successfully! (EzSoftwareUC Installed)" -ForegroundColor Green
    Write-Host "  - AxlIsOpened Status: $isOpen" -ForegroundColor White
} catch {
    Write-Host "  - [INFO] AXL.dll Not Found (Simulation Mode or Driver Not Yet Installed)" -ForegroundColor Yellow
    Write-Host "    -> Please install 02_Drivers\Setup.exe after mounting PCIe-R0805-ECAT-E." -ForegroundColor Gray
}

Write-Host "`n======================================================================" -ForegroundColor Cyan
Write-Host " [COMPLETED] IPC System Diagnostics Finished! Ready for MultiScanX.   " -ForegroundColor Yellow
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""
