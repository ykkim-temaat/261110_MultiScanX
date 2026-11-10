# 01_Runtimes (.NET 데스크톱 런타임 보관 및 설치 폴더)

* **필수 설치 파일:** `windowsdesktop-runtime-8.0.33-win-x64.exe`
* **용도:** MultiScan-X Studio의 GUI 화면(WPF UI)을 산업용 PC에서 구동하기 위한 .NET 8 데스크톱 런타임입니다.
* **Git 버전 관리:** 대용량 바이너리(`.exe`) 파일은 `.gitignore`에 의해 Git 추적에서 제외되어 안전하게 관리됩니다.

---

## 🌐 인터넷에서 직접 다운로드하는 방법

배포 USB나 로컬 폴더에 인스톨러가 없는 경우, 인터넷을 통해 공식 다운로드하는 2가지 방법입니다.

### 방법 1. 웹 브라우저에서 직접 다운로드 (권장 🏆)
1. 마이크로소프트 공식 .NET 8.0 다운로드 페이지에 접속합니다:
   * **URL:** [https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0](https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0)
2. 화면 오른쪽 중간의 **`.NET 데스크톱 런타임 8.0.x`** 섹션을 찾습니다.
3. `Windows` 행의 **[설치 관리자]** 열에 있는 **`x64`** 파란색 링크를 클릭합니다.
4. 다운로드된 **`windowsdesktop-runtime-8.0.x-win-x64.exe`** 파일을 더블클릭하여 설치합니다.

---

### 방법 2. PowerShell 터미널 명령어로 1초 다이렉트 다운로드
산업용 PC의 PowerShell 창에서 아래 명령어를 실행하면 브라우저를 켤 필요 없이 즉시 인스톨러가 다운로드됩니다:

```powershell
Invoke-WebRequest -Uri "https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.33/windowsdesktop-runtime-8.0.33-win-x64.exe" -OutFile "windowsdesktop-runtime-8.0.33-win-x64.exe"
```
*(다운로드 완료 후 생성된 `windowsdesktop-runtime-8.0.33-win-x64.exe`를 더블클릭하여 설치)*

---

## 🔍 설치 완료 검증 및 확인 방법

설치가 끝난 후 제대로 설치되었는지 확인하는 가장 확실한 방법입니다.

### 터미널 명령어로 확인
PowerShell 또는 명령 프롬프트(CMD)를 열고 아래 명령어를 실행합니다:

```powershell
dotnet --list-runtimes
```

* **정상 설치 확인 기준:**
  ```text
  Microsoft.WindowsDesktop.App 8.0.33 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
  Microsoft.NETCore.App 8.0.33 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  ```
  *(반드시 `Microsoft.WindowsDesktop.App` 항목이 포함되어 있어야 합니다)*
