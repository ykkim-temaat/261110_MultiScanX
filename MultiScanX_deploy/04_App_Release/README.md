# 04_App_Release (MultiScan-X Studio 공식 Windows 인스톨러 보관 폴더)

* **배포 파일:** `MultiScanX_Setup_v1.0.0.exe` (Inno Setup 6 공식 설치 마법사, 약 2 MB)
* **설치 대상 경로:** 산업용 PC의 `C:\MultiScanX\bin\MultiScanX.App.exe`
* **Git 버전 관리:** `.exe` 바이너리는 `.gitignore`에 의해 제외되며, `src\build_deploy.ps1` 실행 시 자동 생성됩니다.

---

## 📌 이원화(Dual-Track) 배포 및 실행 안내

### 1. 트랙 A: 최초 셋업 및 정식 납품 (`MultiScanX_Setup_v1.0.0.exe` 실행 🏆)
신규 산업용 PC에 처음 배포하거나 고객사(K社)에 납품할 때는 본 폴더의 **`MultiScanX_Setup_v1.0.0.exe`**를 더블클릭하여 설치합니다.
* **자동 처리 내용:**
  1. `C:\MultiScanX\bin\MultiScanX.App.exe` 정식 설치
  2. 필수 폴더(`C:\MultiScanX\config`, `C:\MultiScanX\versions`) 생성
  3. 바탕화면에 **`MultiScan-X Studio`** 바로가기 아이콘 자동 생성
  4. Windows 제어판 **[프로그램 추가/제거]** 정식 등록 (원클릭 Uninstall 지원)

### 2. 트랙 B: 개발 / 시운전 중 초고속 테스트 (`src\Release_Bin\MultiScanX.App.exe`)
개발 및 장비 시운전 중 코드를 수정한 뒤 설치 마법사를 다시 돌리지 않고 바로 테스트하려면:
1. 개발 노트북에서 `.\src\build_deploy.ps1 -PatchOnly` 실행 (2초 만에 `src\Release_Bin\MultiScanX.App.exe` (220 KB) 생성)
2. WinSCP를 통해 해당 파일 1개만 산업용 PC의 **`C:\MultiScanX\bin\`** 폴더에 덮어쓰기 복사
3. 산업용 PC 바탕화면의 **`MultiScan-X Studio`** 아이콘을 더블클릭하여 즉시 실행 확인!
