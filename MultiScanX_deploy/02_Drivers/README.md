# 02_Drivers (제어 하드웨어 드라이버 보관 및 설치 폴더)

* **필수 설치 파일:** 아진엑스텍 `EzSoftwareUC` 설치 패키지 (V2.2.1.5206 이상)
* **용도:** PCIe-R0805-ECAT-E 8축 EtherCAT 마스터 카드 및 N3ECAT 슬레이브 3종(CNT2, DB32T, AI16) 제어 라이브러리(`AXL.dll`, `AXM.dll`, `AXD.dll`, `AXC.dll`)
* **Git 버전 관리:** 대용량 설치 압축파일(`.zip`)은 약 920MB로 Git 파일 크기 제한(100MB)을 초과하므로, `.gitignore`에 등록하여 안전하게 격리 관리합니다.

---

## 💾 오프라인 로컬 원본 보관 위치

인터넷이 연결되지 않은 반도체/패키징 공정 팹(Fab) 폐쇄망 환경을 위해, 프로젝트 내에 사전 검증된 정품 설치 패키지가 보관되어 있습니다:

* **프로젝트 내 원본 경로:**  
  `02_데이터시트 및 관련자료/01-2_EzSoftwareUC V2.2.1.5206_HW_20260907_Revision6742.zip` (약 921MB)
* **사용 방법:**  
  위 압축 파일을 이 폴더(`02_Drivers/`)로 복사 후 압축을 풀고 **`Setup.exe`를 관리자 권한으로 실행**합니다.

---

## 🌐 인터넷에서 최신 버전 직접 다운로드하는 방법

인터넷이 연결된 환경에서 아진엑스텍 공식 고객지원 센터를 통해 직접 다운로드하는 절차입니다:

### 1. 아진엑스텍 공식 기술지원 자료실 접속
* **공식 웹사이트:** [http://www.ajinextek.com](http://www.ajinextek.com)  
  (또는 고객지원 포털: [http://support.ajinextek.com](http://support.ajinextek.com))

### 2. 다운로드 경로 이동
1. 상단 메뉴에서 **[고객지원]** ➔ **[다운로드 자료실]**을 클릭합니다.
2. 분류에서 **[Software / Library]**를 선택합니다.
3. 검색창에 **`EzSoftwareUC`**를 입력하고 검색합니다.
4. **`EzSoftwareUC (PCIe-R0805-ECAT 지원 통합 라이브러리)`** 최신 설치본(V2.2.1.x 이상)을 다운로드합니다.

---

## 🚀 설치 및 적용 절차

1. 다운로드받은 압축 파일의 압축을 해제합니다.
2. 해제된 폴더 내의 **`Setup.exe`** 파일을 마우스 우클릭 ➔ **[관리자 권한으로 실행]**합니다.
3. 설치 옵션은 기본값(Typical/전체) 그대로 두고 [Next]를 눌러 완료합니다.
4. **[필수] 설치 완료 후 산업용 PC를 1회 재부팅합니다.** (드라이버 커널 모듈 최종 적용)

---

## 🔍 설치 완료 검증 방법

PC 재부팅 후 아래 2가지 중 하나로 정상 설치 여부를 확인합니다:

1. **시작 메뉴 확인:**  
   [시작] 메뉴 ➔ [EzSoftwareUC] 폴더 내에 `EzHardware`, `EzMotion` 등의 유틸리티가 등록되었는지 확인합니다.
2. **진단 스크립트로 자동 검증 (무설치 PowerShell 추천 🏆):**  
   파이썬 설치 없이, 산업용 PC의 PowerShell에서 아래 명령어를 실행하여 초록색 `[PASS]`가 뜨는지 확인합니다:
   ```powershell
   powershell -ExecutionPolicy Bypass -File D:\TeMaat\MultiScanX_deploy\03_Diagnostics\ipc_diagnostics.ps1
   ```
   * **확인:** 5번 항목에서 `[PASS] AXL.dll 라이브러리 로드 성공! (EzSoftwareUC 정상 설치됨)` 문구 확인
