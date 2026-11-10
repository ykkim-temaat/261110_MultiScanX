# 산업용 PC(Nuvo-9002E) 셋업 및 Hello World 배포 가이드

* **문서 번호:** `261110-06`
* **작성 일자:** 2026-11-10
* **주관 부서:** TeMaat 기술연구소
* **대상 기종:** Neousys Nuvo-9002E-BMI5 (산업용 제어 PC) & 개발 노트북
* **참조 도면/BOM:** [`261110-03_MultiSensor-Metrology_부품발주검토_및_BOM_명세서.md`](./261110-03_MultiSensor-Metrology_부품발주검토_및_BOM_명세서.md)

---

## 📋 문서 개정/수정 이력 (Revision History)

| 버전 (Rev) | 개정/수정 일자 | 작성자 / 부서 | 주요 변경 내역 (Change Description) |
| :---: | :---: | :---: | :--- |
| **Rev 1.4** | 2026-11-10 | TeMaat 기술연구소 | • 실무 기준 셋업 순서 전면 재정비: [Step 1] SSH/WinSCP 개설 ➔ [Step 2] D:\TeMaat 업로드 ➔ [Step 3] 런타임/드라이버 설치 ➔ [Step 4] 앱 구동<br/>• 수동 디렉토리 생성 제거 ➔ C# .NET 앱 최초 실행 시 디렉토리 자체 자동 생성(Auto-Provisioning) 명세화 |
| **Rev 1.3** | 2026-11-10 | TeMaat 기술연구소 | • SSH 설치 후 필수 재부팅 및 가장 확실한 netsh 방화벽 개방 명령어 반영 |
| **Rev 1.2** | 2026-11-10 | TeMaat 기술연구소 | • Windows 내장 OpenSSH 서버 활성화 및 WinSCP(SFTP) 원격 배포 절차 추가 수록 |
| **Rev 1.1** | 2026-11-10 | TeMaat 기술연구소 | • .NET Desktop Runtime 인스톨러 파일명 정정 (`windowsdesktop-runtime-8.0.x-win-x64.exe`) |
| **Rev 1.0** | 2026-11-10 | TeMaat 기술연구소 | • 산업용 PC(Neousys Nuvo-9002E) 최초 셋업 절차 및 개발 노트북 간 역할 분리 가이드 수록 |

---

## 1. 개요 및 환경 분리 기본 철학

반도체/패키징 공정 장비의 산업용 PC(IPC)는 **안정성(24/7 무중단 구동)**과 **보안(소스코드 유출 방지 및 팹 폐쇄망 준수)**이 최우선입니다.  
따라서 개발 노트북과 IPC의 역할을 엄격히 분리하여 셋업합니다:

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                 개발 환경 vs 현장 산업용 PC 운영 환경 분리                  │
├──────────────────────────────────────┬──────────────────────────────────────┤
│       [개발 노트북 (Dev Machine)]     │       [산업용 제어 PC (Nuvo-9002E)]  │
├──────────────────────────────────────┼──────────────────────────────────────┤
│ ㆍVisual Studio 2022 + .NET 8 SDK    │ ㆍ컴파일러/IDE 미설치 (보안/경량화)  │
│ ㆍGit 소스코드 리포지토리 보유       │ ㆍ.NET 8 Desktop Runtime만 설치      │
│ ㆍWinSCP로 D:\TeMaat 에 배포 패키지 전송│ ㆍD:\TeMaat\MultiScanX_deploy 활용   │
│ ㆍSelf-Contained 단일 EXE 빌드/배포  │ ㆍ앱 구동 시 필수 디렉토리 자체 자동생성│
└──────────────────────────────────────┴──────────────────────────────────────┘
```

---

## 2. 산업용 PC (Neousys Nuvo-9002E) 현장 표준 셋업 4단계

### Step 1. 파일 전송 통로 개설 (Windows OpenSSH 서버 활성화)
신규 출하된 산업용 PC에 모니터/원격 데스크톱을 연결한 후 **가장 먼저 노트북과 통신할 통로(SFTP)**를 엽니다:

1. **산업용 PC에서 PowerShell [관리자 권한] 실행 후 설치 및 재부팅:**
   ```powershell
   # 1. 윈도우 내장 OpenSSH 서버 활성화 (약 3~5분 소요)
   Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

   # 2. [필수] 시스템 레지스트리 서비스 등록을 위한 재부팅
   Restart-Computer
   ```
2. **재부팅 완료 후, 다시 PowerShell [관리자 권한]에서 서비스 시작 및 방화벽 개방:**
   ```powershell
   # 3. 서비스 시작 및 부팅 시 자동 시작 등록
   Start-Service sshd
   Set-Service -Name sshd -StartupType 'Automatic'

   # 4. [가장 확실한 방화벽 개방] netsh 명령어로 22번 포트 인바운드 허용
   netsh advfirewall firewall add rule name="OpenSSH" dir=in action=allow protocol=TCP localport=22
   ```

---

### Step 2. 노트북에서 WinSCP로 배포 패키지 일괄 업로드 (`D:\TeMaat`)
1. **D: 드라이브 폴더 생성 및 바로가기 연결 (IPC PowerShell 2줄):**
   ```powershell
   New-Item -ItemType Directory -Path "D:\TeMaat" -Force
   New-Item -ItemType Junction -Path "C:\Users\USER\TeMaat" -Target "D:\TeMaat"
   ```
2. **개발 노트북에서 WinSCP로 접속:**
   * 호스트: `192.168.43.101` / 포트: `22` / 프로토콜: `SFTP`
   * 계정: 산업용 PC 계정명(`USER`) 및 암호 입력 (암호가 없는 경우 `net user USER 1234`로 설정)
3. **업로드:**
   * 노트북 좌측 창의 **`MultiScanX_deploy` 폴더**를 우측 창의 **`TeMaat` (D:\TeMaat)** 폴더로 드래그 앤 드롭 복사합니다.

---

### Step 3. 필수 런타임 및 하드웨어 드라이버 설치
산업용 PC의 `D:\TeMaat\MultiScanX_deploy\` 폴더 안의 파일들로 설치를 진행합니다:

1. **.NET Desktop Runtime 8.0 (x64) 설치 및 검증:**
   * `01_Runtimes/windowsdesktop-runtime-8.0.33-win-x64.exe` 더블클릭 설치 (약 30초)
   * **설치 검증 명령어:** `dotnet --list-runtimes` 실행 ➔ `Microsoft.WindowsDesktop.App 8.0.33` 출력 확인
2. **아진엑스텍 EzSoftwareUC 드라이버 설치:**
   * `02_Drivers/` 폴더 내 `Setup.exe` 마우스 우클릭 ➔ **[관리자 권한으로 실행]**
   * 기본값으로 설치 완료 후 PC 1회 재부팅
3. **초기 환경 무설치 원클릭 진단 (Python 불필요 🏆):**
   * PowerShell에서 아래 1줄 실행하여 하드웨어, 드라이브, .NET 런타임 및 아진 AXL 라이브러리 자동 검증:
     ```powershell
     powershell -ExecutionPolicy Bypass -File D:\TeMaat\MultiScanX_deploy\03_Diagnostics\ipc_diagnostics.ps1
     ```

---

### Step 4. 제어 프로그램(`MultiScanX.App`) 배포 및 디렉토리 자동 생성 (Zero-Touch)
작업자가 수동으로 폴더를 일일이 만들 필요가 없습니다. **C# 프로그램이 최초 실행되면서 모든 디렉토리를 스스로 자동 생성**합니다!

1. **프로그램 파일 배치:**
   * `D:\TeMaat\MultiScanX_deploy\04_App_Release\`의 실행 파일들을 `C:\MultiScanX\bin\`으로 복사하고 바탕화면 바로가기를 만듭니다.
2. **최초 실행 시 자가 생성(Auto-Provisioning)되는 디렉토리 구조:**
   * `MultiScanX.App.exe`가 실행되는 순간 아래 폴더들이 1초 만에 자동 생성됩니다:
     * `D:\MultiScanX_Data\Results\` : 계측 결과 파일(`.txt`, `.csv`)
     * `D:\MultiScanX_Data\Maps\` : 2D/3D 두께 매핑 결과
     * `D:\MultiScanX_Data\Logs\` : 날짜별 설비 구동/인터록 로그
     * `D:\MultiScanX_Data\Recipes\` : 사용자 레시피 파일 (`*.recipe`)
     * `C:\MultiScanX\config\` : 장비 고유 캘리브레이션 (`hardware_config.json`)
     * `C:\MultiScanX\versions\` : 이전 버전 롤백용 디렉토리

---

## 3. 윈도우 OS 환경 최적화 (24/7 가동용)

* **전원 관리:** [제어판] - [전원 옵션] - **고성능(High Performance)** 선택, "화면 끄기" 및 "절전 모드" **해제(없음)**
* **Windows Update 자동 재부팅 방지:** `gpedit.msc` 실행 ➔ 컴퓨터 구성 ➔ 관리 템플릿 ➔ Windows 구성 요소 ➔ Windows 업데이트 ➔ "자동 업데이트 구성" **[사용 안 함]** 설정
* **네트워크 분리:**
  * **LAN 1:** 사내망/원격 지원용 고정/DHCP IP
  * **LAN 2:** Keyence CL-3000 전용 센서 직결망 (`192.168.0.100 / 255.255.255.0`)

---

## 4. 사후 유지보수(A/S)를 위한 골든 이미지(Golden Image) 백업

장비 셋업과 드라이버 점검이 완료된 직후, 반드시 시스템 전체 백업을 수행합니다:
1. **Macrium Reflect Free** 또는 **Clonezilla** 도구 준비
2. C: 드라이브(500GB NVMe) 전체를 단일 이미지 파일(`MultiScanX_IPC_Golden_v1.0.mrimg`)로 백업
3. 백업 파일 저장 위치:
   * **`D:\MultiScanX_Data\System_Backup\`** (IPC 내부 복구용)
   * **사외 보관용 TeMaat 전용 USB** (긴급 출동 A/S용)
4. IPC 하드웨어 불량이나 Windows 손상 시, 15분 만에 초기 출하 상태로 100% 원복 가능
