# MultiScanX 프로젝트 현황 요약 (K社 박막두께 자동계측 시스템)

* **최종 업데이트 일자:** 2026-11-10
* **주관 부서:** TeMaat 기술연구소
* **상태:** 기구/전장 설계 완료 ➔ 아진엑스텍 EtherCAT 제어 솔루션(PCIe 마스터 + 슬레이브 3종) 벤치 테스트 및 S/W 개발 단계
* **원격 저장소:** `git@github.com:ykkim-temaat/261110_MultiScanX.git`

---

## 📋 문서 개정/수정 이력 (Revision History)

| 버전 (Rev) | 개정/수정 일자 | 작성자 / 부서 | 주요 개정 및 수정 내용 |
| :---: | :---: | :---: | :--- |
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

