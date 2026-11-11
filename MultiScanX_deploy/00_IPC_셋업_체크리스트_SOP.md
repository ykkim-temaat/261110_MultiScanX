# [SOP] 산업용 제어 PC (Nuvo-9002E) 표준 셋업 가이드 및 체크리스트

* **문서 버전:** Rev 1.4 (2026-11-11)
* **작성 부서:** (주)티마트 기술연구소 (TeMaat Automation)
* **적용 대상:** MultiScanX (K社 KCT向 박막두께 자동계측 시스템) 제어 PC
* **하드웨어 사양:** Neousys Nuvo-9002E-BMI5 (i5-13400, 16GB, 500GB NVMe + 2TB HDD)
* **준비물:** 개발 노트북 & WinSCP (SFTP) 연결 환경

---

## 📌 작업 전 주의사항 (Must Read)
> [!IMPORTANT]
> 1. 모든 프로그램 설치 및 PowerShell 명령어는 반드시 **[관리자 권한으로 실행]**해야 충돌이 없습니다.
> 2. 설치 과정 중 재부팅 안내가 나오면 **반드시 재부팅 후 다음 단계로 진행**합니다.
> 3. 아래 체크박스(`[ ]`)를 순서대로 하나씩 체크하면서 작업하면 누구나 실수 없이 15분 만에 표준 셋업을 완료할 수 있습니다.
> 4. **수많은 데이터/로그/레시피 폴더를 손으로 만들 필요가 없습니다!** 제어 프로그램(`MultiScanX.App.exe`)이 최초 실행될 때 스스로 필요한 모든 폴더를 자동 생성(Zero-Touch Auto Provisioning)합니다.

---

## 📋 1-Page 순서별 현장 작업 체크리스트

### [1단계] 파일 전송 통로 개설 (Windows OpenSSH 서버 활성화)
노트북에서 **WinSCP (SFTP)**로 설치 패키지를 전송하고, 향후 원격 로그 수집/패치 배포를 위해 윈도우 내장 SSH 서버를 활성화합니다.

- [ ] **1. 산업용 PC에서 PowerShell [관리자 권한으로 실행] 후 아래 명령어 순서대로 실행:**
  ```powershell
  # 1. 윈도우 내장 OpenSSH 서버 기능 활성화 (약 3~5분 소요)
  Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

  # 2. [필수] 시스템 레지스트리 서비스 등록을 위한 PC 재부팅
  Restart-Computer
  ```
- [ ] **2. 재부팅 완료 후, 다시 PowerShell [관리자 권한]에서 아래 3줄 복사/실행:**
  ```powershell
  # 3. SSH 서비스 시작 및 부팅 시 자동 시작 등록
  Start-Service sshd
  Set-Service -Name sshd -StartupType 'Automatic'

  # 4. [가장 확실한 방화벽 개방] netsh 명령어로 22번 포트 인바운드 허용
  netsh advfirewall firewall add rule name="OpenSSH" dir=in action=allow protocol=TCP localport=22
  ```

---

### [2단계] 노트북에서 WinSCP로 배포 패키지 일괄 업로드 (`D:\TeMaat`)
- [ ] **1. D: 드라이브 TeMaat 폴더 생성 및 바로가기 연결 (IPC PowerShell 2줄):**
  ```powershell
  New-Item -ItemType Directory -Path "D:\TeMaat" -Force
  New-Item -ItemType Junction -Path "C:\Users\USER\TeMaat" -Target "D:\TeMaat"
  ```
- [ ] **2. 개발 노트북에서 WinSCP로 접속 및 파일 전송:**
  * 프로토콜: `SFTP` / 포트: `22` / 호스트: 산업용 PC IP (`192.168.43.101`)
  * 계정: 산업용 PC 윈도우 계정명(`USER`) 및 비밀번호 (암호가 없는 경우 `net user USER 1234`로 설정)
  * **노트북 좌측의 `MultiScanX_deploy` 폴더를 우측의 `TeMaat` (D:\TeMaat) 폴더로 드래그 앤 드롭 전송**

---

### [3단계] 필수 런타임 및 하드웨어 드라이버 설치
산업용 PC의 `D:\TeMaat\MultiScanX_deploy\` 폴더 안의 파일들로 설치를 진행합니다.

- [ ] **1. .NET 8 데스크톱 런타임 설치:**
  * `01_Runtimes/windowsdesktop-runtime-8.0.33-win-x64.exe` 더블클릭 설치 (약 30초 소요)
- [ ] **2. 런타임 정상 설치 검증 (PowerShell 명령어):**
  ```powershell
  dotnet --list-runtimes
  ```
  * **확인:** `Microsoft.WindowsDesktop.App 8.0.33` 항목이 출력되는지 확인!
- [ ] **3. 아진엑스텍(Ajinextek) 제어 드라이버 설치:**
  * `02_Drivers/` 폴더 내의 **`Setup.exe`** 마우스 우클릭 ➔ **[관리자 권한으로 실행]**
  * 기본 옵션으로 설치 완료 후 안내에 따라 **PC 1회 재부팅**
- [ ] **4. 초기 환경 및 하드웨어 무설치 원클릭 진단 (PowerShell):**
  * 파이썬 설치가 전혀 필요 없습니다! PowerShell에서 아래 명령어 1줄을 실행하여 초록색 `[PASS]`를 확인합니다:
    ```powershell
    powershell -ExecutionPolicy Bypass -File D:\TeMaat\MultiScanX_deploy\03_Diagnostics\ipc_diagnostics.ps1
    ```
  * **확인 항목:**
    * OS 및 CPU 사양 정상 판독 확인
    * C:(NVMe) 및 D:(2TB HDD) 스토리지 용량 확인
    * `.NET 8 데스크톱 런타임` 정상 감지 `[PASS]` 확인
    * 아진엑스텍 `AXL.dll` 감지 완료 `[PASS]` 확인

---

### [4단계] EtherCAT 슬레이브 3종 실물 벤치 테스트 (무설치 PowerShell)
- [ ] **1. 제어반 전원(24V SMPS) 투입 확인:**
  * 전장 제어반 내 마스터 카드와 슬레이브 3종(CNT2, DB32T, AI16)에 RJ45 케이블이 연결되어 있는지 확인합니다.
  * 토폴로지: `마스터 OUT ➔ CNT2 (IN-OUT) ➔ DB32T (IN-OUT) ➔ AI16 (IN)`
- [ ] **2. PowerShell에서 아래 명령어 1줄 실행:**
  ```powershell
  powershell -ExecutionPolicy Bypass -File D:\TeMaat\MultiScanX_deploy\03_Diagnostics\ethercat_bench_test.ps1
  ```
  * **확인 포인트:** N3ECAT-DB32T 모듈 전면의 DO 0번부터 15번까지 녹색 LED가 1개씩 순서대로 켜졌다 꺼지는지 육안으로 확인합니다!
  * 콘솔 창에 `[SUCCESS] EtherCAT 3-Module Benchmark Verification Finished!` 문구가 뜨면 하드웨어 통신 100% 정상입니다.

---

### [5단계] 제어 프로그램 정식 설치 및 실행 (Inno Setup)
- [ ] **1. 정식 인스톨러(`MultiScanX_Setup_v1.0.0.exe`) 더블클릭 설치:**
  * `D:\TeMaat\MultiScanX_deploy\04_App_Release\MultiScanX_Setup_v1.0.0.exe` 파일을 더블클릭하여 설치 마법사를 실행합니다.
  * 기본 경로(`C:\MultiScanX\`) 및 **[바탕 화면에 바로 가기 아이콘 만들기]** 체크를 확인한 후 **[설치]**를 완료합니다.
  * *(`C:\MultiScanX\bin\MultiScanX.App.exe` 설치, 바탕화면 아이콘 생성, 제어판 Uninstall 등록이 자동으로 완료됩니다)*
- [ ] **2. 최초 실행 및 데이터 폴더 자동 생성 확인:**
  * 설치 완료 화면 또는 바탕화면의 **`MultiScan-X Studio`** 아이콘을 더블클릭하여 실행합니다.
  * 프로그램이 켜지면서 2TB 데이터 드라이브(`D:\`)에 아래 필수 폴더들이 자동으로 생성됩니다:
    * `D:\MultiScanX_Data\Results\` (결과 리포트 저장소)
    * `D:\MultiScanX_Data\Maps\` (2D/3D 두께 매핑 원본)
    * `D:\MultiScanX_Data\Logs\` (설비 및 에러 로그)
    * `D:\MultiScanX_Data\Recipes\` (측정 레시피)
- [ ] **(참고) 개발/시운전 중 빠른 패치 업데이트 시:**
  * 개발 노트북에서 `.\src\build_deploy.ps1 -PatchOnly` 실행 ➔ 생성된 `src\Release_Bin\MultiScanX.App.exe` (220 KB)만 IPC의 `C:\MultiScanX\bin\`에 덮어쓰기 복사하면 재설치 없이 즉시 테스트 가능합니다.

---

## 📞 비상 시 연락처
* **시스템 셋업 지원:** (주)티마트 기술연구소 제어개발팀
