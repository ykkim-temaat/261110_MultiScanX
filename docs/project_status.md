# MultiScanX 프로젝트 현황 요약 (K社 박막두께 자동계측 시스템)

* **최종 업데이트 일자:** 2026-11-13
* **주관 부서:** TeMaat 기술연구소
* **상태:** 기구/전장 설계 완료 ➔ 아진엑스텍 EtherCAT 제어 솔루션(PCIe 마스터 + 슬레이브 3종) 벤치 테스트 및 S/W 개발 단계
* **원격 저장소:** `git@github.com:ykkim-temaat/261110_MultiScanX.git`

---

## 📋 문서 개정/수정 이력 (Revision History)

| 버전 (Rev) | 개정/수정 일자 | 작성자 / 부서 | 주요 개정 및 수정 내용 |
| :---: | :---: | :---: | :--- |
| **Rev 1.10** | 2026-11-13 | TeMaat 기술연구소 | • **아진 공식 C# 헤더(`AXM.cs`/`AXC.cs`/`AXA.cs`) 기반 정품 API 교정 (`v1.2.3`):** `AxmStatusGetActPos`, `AxcStatusGetActPos`, `AxcStatusSetActPos`, `AxaiSwReadVoltage`, `AxcTriggerSetNotchPos` 등으로 전면 교체하여 `EntryPointNotFoundException` 해소<br/>• **엔지니어 편집 잠금(`PW: 0000`, 5분 자동 잠금) 및 `AI16` CH1~4 전면 DIP 스위치 선택 UI 구현**<br/>• **문서 소급 현행화:** `.agents/AGENTS.md`(상충 시 소급 현행화 규칙 추가), `261110-06` 배포 가이드(`Rev 1.6`), `MultiScanX_deploy` SOP(`Rev 1.5`) 및 드라이버 README 현행화 |
| **Rev 1.9** | 2026-11-13 | TeMaat 기술연구소 | • IPC 실물 `N3ECAT-DB32T` (`DI01`/`DI09`), `AI16`, `CNT2`, 파나소닉 서보 2축 온라인 연동 검증 완료 (`🟢 AXL Online`)<br/>• **`MultiScanX_deploy` 배포 솔루션 3대 개선 (`v1.2.2`):** PowerShell 실행 정책 우회 원클릭 `.bat` 런처 2종 신설, `AXL.dll` 64비트 경로 자동 탐색(`SetDllDirectory`) 및 인스톨러 자동 복사, 진단 스크립트(`ipc_diagnostics.ps1`/`ethercat_bench_test.ps1`) `AXL.dll` 단일 호출/자동경로 통합<br/>• `IoHardwareCheckControl` UI 디테일 개선 (1줄 서브탭 정렬, 16DI/16DO 전 채널 한눈에 보기, Brake 버튼 텍스트 잘림 해소) |
| **Rev 1.8** | 2026-11-13 | TeMaat 기술연구소 | • 일반 관리자 PC 검토용 GUI 3종 비교 실행 패키지(`03_소프트웨어 개발/MultiScanX_GUI_디자인검토용_배포패키지_v1.1.0.zip`) 제작 및 배포<br/>• **MultiScan-X Studio v1.2.0 업그레이드:** 디자인 C의 `H/W & I/O Check` 스튜디오 창에 **실물 `N3ECAT-DB32T` 50ms(20Hz) 16-bit Word 폴링(`0x6000`/`0x7010`)**, **`N3ECAT-AI16` 16채널 정의 및 물리량 스케일링(`Raw×Scale+Offset`) 모니터링 탭**, **`N3ECAT-CNT2` Z축 리니어스케일 Low-Level 세팅(`0x2001` 체배율, `0x2002` 방향, `0x7000/0x7002` 원점 프리셋, `0x7010` 고속 위치 비교 트리거) 탭** 통합 완료 |
| **Rev 1.7** | 2026-11-12 | TeMaat 기술연구소 | • `2560×1440 (QHD)` 권장 해상도 및 파스텔 라이트 테마 기반 **GUI 3종 레이아웃 비교용 .NET 8 WPF 실행 파일 3종(`Layout A/B/C`, v1.1.0)** 구현 및 **초안 C(미니멀 메인 관제 데스크 + 독립 플로팅 스튜디오 팝업형) 최종 확정**<br/>• `CAS CI-600A` 공식 매뉴얼(`07-2_141116_CI600A_KOR_UM.pdf`) 기반 RS-232C Modbus-RTU(`Menu-2207=9`, `40008~40061`) 및 릴레이 비교출력(`OUT1->DI4`, `OUT2->DI5` 과부하 자동상승) 반영<br/>• `CNT2`/`DB32T` 동적 I/O 점검, `10×10` 레시피 행/열 미세조정(`Shift`) & Snake 최단경로 정렬, `ethercat_bench_test.ps1` 파나소닉 서보 2축 스캔 보강 |
| **Rev 1.6** | 2026-11-12 | TeMaat 기술연구소 | • 소프트웨어 기능 정의 체크리스트(`261111-02`) 사용자 구상 반영 완료 및 종합 개발 계획서 초안(`261112-01`) 수립<br/>• 2단계 분리 개발 전략(Phase 1: 3축 갠트리/스케일/로드셀 제어 ➔ Phase 2: 사급 계측기 3종 통합) 및 파스텔 라이트 GUI 아키텍처 확정<br/>• Z축 다중 조건 우선순위 정지(위치/DI접점/RS-232C Modbus 무게), 3종 센서 3D 오프셋 자동 보상, 동적 I/O 매핑 설계 |
| **Rev 1.5** | 2026-11-11 | TeMaat 기술연구소 | • Inno Setup 6 정식 인스톨러(`MultiScanX_Installer.iss`) 및 Dual-Track 빌드 파이프라인 구축/IPC 설치 검증<br/>• Build-Time 버전 SSOT(`.csproj` `<Version>`) 및 Run-Time 설정(`SystemConfig` / `system_config.json`) 분리 적용(v1.0.2)<br/>• 개발/시운전용 PowerShell 명령어 치트시트(`261111-01`) 작성 |
| **Rev 1.4** | 2026-11-10 | TeMaat 기술연구소 | • C# .NET WPF 제어 S/W 단일 EXE 빌드 파이프라인(`src/build_deploy.ps1`) 구축 및 220KB 초경량 배포 실증 완료<br/>• 전장 안전 규격(IEC 60204-1/SEMI S2) 준수: 소프트웨어 조작 버튼 `측정 중지(Stop Scan)` 분리 및 `HW EMO` 감시 인디케이터 신설<br/>• IPC 실물 모니터 1:1 실시간 화면 미러링을 위한 VNC(TightVNC Server + RealVNC Viewer) 가이드 수립 |
| **Rev 1.3** | 2026-11-10 | TeMaat 기술연구소 | • `부품발주 검토_261109.xlsx` 내 단일화된 `부품발주_Panasonic` 시트 기준으로 BOM SSOT 최종 동기화 완료<br/>• 산업용 PC(Nuvo-9002E) 실물 셋업 및 Hello World 검증 가이드(`261110-06`) 연동 |
| **Rev 1.2** | 2026-11-10 | TeMaat 기술연구소 | • 프로젝트 공식 명칭 `MultiScanX` 확정 및 GitHub 원격저장소 연동<br/>• Keyence CL-3000(CL-S015) Ethernet TCP/IP 통신 확정<br/>• PCIe 마스터 + 슬레이브 3종(CNT2, DB32T, AI16) 전용 벤치 테스트 도구 구축 |
| **Rev 1.1** | 2026-11-10 | TeMaat 기술연구소 | • 설계 완료에 따른 제어 S/W(MultiScan-X Studio) 본격 개발 착수<br/>• `src/MultiScanX` C# .NET 솔루션 스캐폴딩 및 아진엑스텍 AXL/계측기 연동 완료<br/>• 30 Point Recipe 기반 로드셀 30g 정압 터치 인터록 및 .txt 파일 생성 시뮬레이션 검증 완료 |
| **Rev 1.0** | 2026-11-10 | TeMaat 기술연구소 | • 프로젝트 초기화 및 표준 AI 협업 환경(`.agents/AGENTS.md`) 구축 완료<br/>• 고객 요구사항(PPTX), 전장도면 18개 시트(PDF), 부품발주 검토(Excel) 전수 분석 및 현황판 수록 |

---

## 1. 프로젝트 개요
* **프로젝트명:** MultiScanX (K社 KCT向 박막두께 자동계측 시스템)
* **저장소 폴더명 / GitHub:** `git@github.com:ykkim-temaat/261110_MultiScanX.git`
* **주관:** TeMaat 기술연구소
* **개발 목표:** Thick Cu, 광학투과막(Filmetrics F20), TTV(Keyence CL-3000 공초점) 3종 계측기를 통합 탑재하여 $510 \times 515\text{ mm}$ 대형 패널 기판의 휨(Warpage)을 극복하고 다포인트 전자동 박막 두께 매핑을 수행하는 고정밀 계측 장비 개발

---

## 2. 주요 시스템 사양 요약

### (1) 핵심 계측기 구성 (Multi-Sensor Configuration)
* **Thick Cu 두께 측정:** Fischerscope MMS PC2 (RS-232C 통신, 전용 프로브 8종 중 선정 예정)
* **광학투과막 두께 측정:** Filmetrics F20-EXR (USB 통신, Z축 광학 초점 자동 제어)
* **TTV 단차/휨 및 변위 측정 (최종 확정 🏆):** **Keyence CL-3000 컨트롤러 + CL-S015 초소형 공초점 광학 변위센서 헤드** (측정거리 15mm, Gigabit Ethernet TCP/IP 소켓 통신 확정)
* **기판 접촉압 감지:** CAS BCL 1kg 싱글포인트 로드셀 + CI-600A 인디케이터 (프로브 보호 인터록)

### (2) 기구 및 스테이지 구동계 (Mechanics & Motion)
* **대응 기판 규격:** $510 \times 515\text{ mm}$ 대형 기판 ($415 \times 510\text{ mm}$ 호환)
* **기판 휨 대응:** 다분할 진공 척(Vacuum Chuck) + 4-Edge 외곽 기계식 클램프(Mechanical Clamp)
* **3축 모션 스트로크:** $X: 550\text{ mm} \times Y: 550\text{ mm} \times Z: 100\text{ mm}$
* **정밀 전동계:** THK C5급 초정밀 연삭 볼스크류(X, Y) 및 C3급 초정밀 볼스크류(Z), THK SHS20/15 정밀 LM 가이드
* **방진 베이스:** 약 650kg 고강성 화강암 석정반 베이스 채택

### (3) 제어 시스템 및 전장 아키텍처 (Control & Electrical)
* **산업용 제어 PC:** Neousys Nuvo-9002E (Intel Core i5-13400, 16GB DDR5, 500GB NVMe + 2TB HDD)
* **필드버스 마스터:** 아진엑스텍 PCIe-R0805-ECAT-E (8축 독립형 EtherCAT 마스터 카드)
* **서보 구동계 (Option 1 🏆):**
  * X, Y축: Panasonic MINAS A6B (MBDLN25BE 드라이버 + MSMF042L1S2 400W 모터)
  * Z축: FASTECH Ezi-SERVOⅡ (EzS2-EC-56L 드라이버 + EzM2-56L-A-BK 브레이크 모터)
* **초정밀 피드백:** Renishaw Q4BCY05D08B 광학식 리니어 스케일 ($50\text{ nm}$ 분해능) ➔ 아진엑스텍 N3ECAT-CNT2 고속 카운터 연동
* **안전 시스템:** OMRON G9SE-201 안전 릴레이 + 비상정지 2채널 직렬 감시 + 서보 4축 하드웨어 STO 연동

---

## 3. 관련 기술 및 설계 산출물 목록

| 분류 | 파일 경로 및 문서 링크 | 주요 내용 설명 |
| :---: | :--- | :--- |
| **규칙** | [.agents/AGENTS.md](../.agents/AGENTS.md) | 세션 시작 시 자동 로드되는 워크스페이스 표준 AI 협업 규칙 |
| **이력** | [docs/session_history.md](./session_history.md) | 날짜/세션별 작업 히스토리 및 Next Action Items 관리 문서 |
| **분석** | [docs/261110-01_K社_박막두께_자동계측_시스템_고객요구사항_분석서.md](./261110-01_K社_박막두께_자동계측_시스템_고객요구사항_분석서.md) | 고객 요구사항(PPTX 5개 슬라이드) 전수 분석 및 사양 명세서 |
| **분석** | [docs/261110-02_MultiSensor-Metrology_전장회로설계_및_시스템아키텍처_검토서.md](./261110-02_MultiSensor-Metrology_전장회로설계_및_시스템아키텍처_검토서.md) | 전장 회로도(18개 시트) 정밀 분석 및 시스템/안전 회로 검토서 |
| **분석** | [docs/261110-03_MultiSensor-Metrology_부품발주검토_및_BOM_명세서.md](./261110-03_MultiSensor-Metrology_부품발주검토_및_BOM_명세서.md) | 부품 발주 검토 엑셀(2개 시트) 전사 명세 및 예산 비교 분석서 |
| **분석** | [docs/261110-04_KCT고객사_기술협의_및_센서선정_검토서.md](./261110-04_KCT고객사_기술협의_및_센서선정_검토서.md) | KCT 고객사 기술협의용 프로브 8종/얼라인/인터록 기술검토서 |
| **배포** | [MultiScanX_deploy/00_IPC_셋업_체크리스트_SOP.md](../MultiScanX_deploy/00_IPC_셋업_체크리스트_SOP.md) | 산업용 제어 PC 표준 셋업 가이드 및 체크리스트 (SOP) |
| **가이드**| [docs/261110-06_산업용PC_셋업_및_HelloWorld_배포가이드.md](./261110-06_산업용PC_셋업_및_HelloWorld_배포가이드.md) | Nuvo-9002E 셋업, OS 튜닝 및 Self-Contained 배포 가이드 |
| **운영** | [docs/261111-01_개발_시운전_PowerShell_명령어_치트시트.md](./261111-01_개발_시운전_PowerShell_명령어_치트시트.md) | 개발/시운전용 빌드, 패치 및 IPC 원격 점검 명령어 치트시트 |
| **기획** | [docs/261111-02_MultiScanX_소프트웨어_기능정의_체크리스트.md](./261111-02_MultiScanX_소프트웨어_기능정의_체크리스트.md) | 제어 소프트웨어 8대 영역 기능 정의 체크리스트 및 구상 시트 (Rev 1.1) |
| **계획** | [docs/261112-01_MultiScanX_소프트웨어_종합_개발계획서_초안.md](./261112-01_MultiScanX_소프트웨어_종합_개발계획서_초안.md) | 사용자 구상 기반 S/W 종합 개발 계획서 (2단계 로드맵 & 변수 해결책) |
| **설계** | [docs/261112-02_MultiScanX_GUI_레이아웃_비교검토_및_최종선정보고서.md](./261112-02_MultiScanX_GUI_레이아웃_비교검토_및_최종선정보고서.md) | GUI 초안 A/B/C 실물 구동 스크린샷 비교 및 초안 C 최종 선정 보고서 |
| **설계** | `../01_제작사양 및 설계자료/02-2_MultiSensor-Metrology_261109-1.pdf` | 전장 회로도 원본 CAD 출력 PDF (TM-DWG-260825 Rev. A) |
| **BOM** | `../01_제작사양 및 설계자료/02-3_부품발주 검토_261109.xlsx` | 부품 발주 및 견적 검토 원본 엑셀 |
| **매뉴얼**| `../02_데이터시트 및 관련자료/01-1_PCIe-R0805-ECAT-E_HW_KOR_V1.0_20260408.pdf` | 아진엑스텍 EtherCAT 마스터 카드 HW 매뉴얼 |
| **매뉴얼**| `../02_데이터시트 및 관련자료/01-2_EzSoftwareUC V2.2.1.5206_HW_20260515.pdf` | 아진엑스텍 모션/I/O 제어 라이브러리 매뉴얼 |

---

## 4. 핵심 검토 및 결정 필요 사항 (Next Action Items)

- [ ] **1. 기판 정렬(Loading Align) 방식 확정:** 고객사 실물 샘플 입수 후 Pattern vs Edge 정렬 알고리즘 테스트 및 확정
- [ ] **2. Fischerscope MMS PC2 프로브 선정:** 사급 계측기의 측정 대상 Cu 두께에 적합한 프로브 종류(8종 중) 확정
- [ ] **3. Keyence 레이저 변위센서 모델 확정:** TTV 측정 센서 인터페이스(아날로그 vs 디지털 통신) 확정
- [ ] **4. THK 볼스크류 및 장납기 부품 발주 착수:** 리드타임 6~8주 품목(BNK2005/BS1402, Renishaw 스케일) 구매 검토
- [ ] **5. 통합 제어 소프트웨어(MultiScan-X Studio) 아키텍처 수립:** C# / .NET 기반 아진엑스텍 AXL 연동 및 Recipe 관리 UI 구조 설계

