# MultiScanX 개발·시운전 필수 PowerShell 명령어 모음 (Cheat Sheet)

* **문서 번호:** `261111-01`
* **작성 일자:** 2026-11-11
* **적용 대상:** 개발 노트북 (Antigravity IDE) & 산업용 제어 PC (`192.168.43.101`, 계정 `USER`)
* **연관 가이드:** [261110-06_산업용PC_셋업_및_HelloWorld_배포가이드.md](./261110-06_산업용PC_셋업_및_HelloWorld_배포가이드.md)

---

## 📋 문서 개정/수정 이력 (Revision History)

| 버전 (Rev) | 개정/수정 일자 | 작성자 / 부서 | 주요 변경 내역 (Change Description) |
| :---: | :---: | :---: | :--- |
| **Rev 1.0** | 2026-11-11 | TeMaat 기술연구소 | • 개발 노트북 빌드/테스트, 최초 배포본 생성, IPC 원격 셋업/진단/실행에 필요한 모든 PowerShell 명령어 최초 집대성 |

---

## 1. [개발 노트북] 컴파일 · 로컬 테스트 · 배포본 생성 명령

> **실행 위치:** Antigravity IDE 하단 터미널 (프로젝트 루트: `...\261110_MultiScanX`)

### (1) 로컬 개발 중 컴파일 및 즉시 테스트 (노트북 시뮬레이션)
* **코드 컴파일 오류 확인만 수행 (빠른 문법 검사):**
  ```powershell
  dotnet build src/MultiScanX/MultiScanX.sln
  ```
* **노트북에서 GUI 바로 실행해 보기 (시뮬레이션 모드):**
  ```powershell
  dotnet run --project src/MultiScanX/MultiScanX.App
  ```
* **노트북 테스트로 생성된 임시 폴더(`C:\`) 깔끔 삭제:**
  ```powershell
  Remove-Item -Path "C:\MultiScanX_Data", "C:\MultiScanX" -Recurse -Force -ErrorAction SilentlyContinue
  ```

### (2) 배포본 생성 명령 ([src/build_deploy.ps1](../src/build_deploy.ps1))
* **최초 셋업 / 고객 납품용 정식 인스톨러(`MultiScanX_Setup_v1.0.0.exe`) + 본체 동시 생성:**
  ```powershell
  .\src\build_deploy.ps1
  ```
  *(버전을 올려서 빌드할 때: `.\src\build_deploy.ps1 -Version 1.0.1`)*
* **개발/시운전 중 IPC 복사용 초경량 실행 본체(`220 KB`)만 2초 만에 생성:**
  ```powershell
  .\src\build_deploy.ps1 -PatchOnly
  ```
  * 생성 위치: `src\Release_Bin\MultiScanX.App.exe`
* **런타임 미설치 외부 PC용 올인원 패키지(`154 MB`) 생성 시:**
  ```powershell
  .\src\build_deploy.ps1 -Standalone
  ```

---

## 2. [개발 노트북 ➔ IPC] 네트워크 점검 및 원격 파일 전송 (`192.168.43.101`)

### (1) 산업용 PC 연결 상태 확인
* **OpenSSH 포트(22) 및 TightVNC 포트(5900) 통신 점검:**
  ```powershell
  Test-NetConnection -ComputerName 192.168.43.101 -Port 22 -InformationLevel Quiet
  Test-NetConnection -ComputerName 192.168.43.101 -Port 5900 -InformationLevel Quiet
  ```

### (2) WinSCP 없이 터미널 1줄로 파일 고속 전송 (`scp`)
* **시운전 패치 파일(`MultiScanX.App.exe`) 1개만 IPC 실행 폴더로 바로 복사:**
  ```powershell
  scp src/Release_Bin/MultiScanX.App.exe USER@192.168.43.101:C:/MultiScanX/bin/
  ```
* **정식 인스톨러(`MultiScanX_Setup_v1.0.0.exe`)를 IPC 배포 폴더로 전송:**
  ```powershell
  scp MultiScanX_deploy/04_App_Release/MultiScanX_Setup_v1.0.0.exe USER@192.168.43.101:D:/TeMaat/MultiScanX_deploy/04_App_Release/
  ```
* **진단 스크립트 수정본을 IPC로 덮어쓰기 전송:**
  ```powershell
  scp MultiScanX_deploy/03_Diagnostics/*.ps1 USER@192.168.43.101:D:/TeMaat/MultiScanX_deploy/03_Diagnostics/
  ```

---

## 3. [산업용 PC (IPC)] 하드웨어 진단 · 테스트 · 제어 앱 실행 명령

> **실행 위치:** 산업용 PC PowerShell 창 (VNC 화면 또는 SSH 원격 터미널)

### (1) 런타임 및 하드웨어 / EtherCAT 슬레이브 진단
* **.NET 8 데스크톱 런타임 설치 확인:**
  ```powershell
  dotnet --list-runtimes
  ```
* **IPC 하드웨어, 스토리지, 네트워크, `AXL.dll` 통합 진단:**
  ```powershell
  powershell -ExecutionPolicy Bypass -File D:\TeMaat\MultiScanX_deploy\03_Diagnostics\ipc_diagnostics.ps1
  ```
* **아진엑스텍 EtherCAT 슬레이브 3종(CNT2, DB32T, AI16) 실물/가상 벤치마크 테스트:**
  ```powershell
  powershell -ExecutionPolicy Bypass -File D:\TeMaat\MultiScanX_deploy\03_Diagnostics\ethercat_bench_test.ps1
  ```

### (2) 제어 프로그램(`MultiScanX.App`) 실행 및 패치 전 종료
* **설치된 제어 프로그램 터미널에서 직접 실행:**
  ```powershell
  C:\MultiScanX\bin\MultiScanX.App.exe
  ```
* **패치 덮어쓰기 전 실행 중인 프로그램 안전 종료 (파일 잠금 해제용):**
  ```powershell
  Stop-Process -Name "MultiScanX.App" -Force -ErrorAction SilentlyContinue
  ```

---

## 4. [산업용 PC (IPC)] 신규 PC 초기 인프라 셋업 명령 (최초 1회용)

> **주의:** 아래 명령어는 반드시 **IPC에서 [관리자 권한으로 실행]된 PowerShell**에서 실행합니다.

### (1) Windows OpenSSH 서버 활성화 (WinSCP/SCP 연결용)
```powershell
# 1. OpenSSH 서버 설치 및 서비스 등록용 재부팅
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0
Restart-Computer
```
```powershell
# 2. 재부팅 후 서비스 자동 시작 등록 및 22번 포트 방화벽 개방
Start-Service sshd
Set-Service -Name sshd -StartupType 'Automatic'
netsh advfirewall firewall add rule name="OpenSSH" dir=in action=allow protocol=TCP localport=22
```

### (2) `D:\TeMaat` 폴더 및 WinSCP 바로가기(Junction) 연결
```powershell
New-Item -ItemType Directory -Path "D:\TeMaat" -Force
New-Item -ItemType Junction -Path "C:\Users\USER\TeMaat" -Target "D:\TeMaat"
```

### (3) TightVNC Server 다운로드 및 5900번 포트 개방 (RealVNC 1:1 화면 공유용)
```powershell
Invoke-WebRequest -Uri "https://www.tightvnc.com/download/2.8.84/tightvnc-2.8.84-gpl-setup-64bit.msi" -OutFile "D:\TeMaat\tightvnc-setup-64bit.msi"
netsh advfirewall firewall add rule name="TightVNC" dir=in action=allow protocol=TCP localport=5900
```

---

## 5. [개발 노트북] 참고용 환경 점검 및 Git 백업 명령

* **노트북에 설치된 .NET SDK 및 런타임 정보 확인:**
  ```powershell
  dotnet --info
  ```
* **작업 완료 후 Git 변경 확인 · 커밋 · GitHub 푸시:**
  ```powershell
  git status
  git add -A
  git commit -m "feat: 변경 내용 요약"
  git push origin master
  ```

