using System.Collections.Generic;

namespace MultiScanX.Core.Models
{
    /// <summary>
    /// 3종 센서 3D 기구 오프셋 설정 모델 (단위: mm)
    /// </summary>
    public class SensorOffsetConfig
    {
        public string SensorName { get; set; } = string.Empty;
        public double OffsetX { get; set; } = 0.0;
        public double OffsetY { get; set; } = 0.0;
        public double OffsetZ { get; set; } = 0.0;
    }

    /// <summary>
    /// 동적 I/O 채널 매핑 정의 모델 (DB32T DI/DO)
    /// </summary>
    public class IoChannelConfig
    {
        public int ChannelIndex { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsReserved { get; set; } = false;
    }

    /// <summary>
    /// N3ECAT-AI16 아날로그 입력 채널 정의 및 물리량 스케일링 설정 모델
    /// 참조: 03-1_N3ECAT-AI16_AO8_Manual(K)_201504.pdf (CH1~4 전압/전류 DIP 선택, CH5~16 전압 전용)
    /// </summary>
    public class AiChannelConfig
    {
        public int ChannelIndex { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DipSwitchMode { get; set; } = "OFF: 전압(V)";
        public string SignalMode { get; set; } = "Voltage (-10~+10V)";
        public double Scale { get; set; } = 1.0;
        public double Offset { get; set; } = 0.0;
        public string Unit { get; set; } = "V";
    }

    /// <summary>
    /// 설비 런타임 하드웨어 및 시스템 운영 설정 모델 (system_config.json 매핑)
    /// </summary>
    public class SystemConfig
    {
        /// <summary>설비 식별명</summary>
        public string EquipmentName { get; set; } = "KCT-MSX-510";

        /// <summary>시뮬레이션(Dry-Run) 모드 사용 여부 (true: Mock 드라이버, false: 실물 하드웨어)</summary>
        public bool UseSimulationMode { get; set; } = true;

        /// <summary>Keyence CL-3000 컨트롤러 IP 주소</summary>
        public string KeyenceIp { get; set; } = "192.168.0.10";

        /// <summary>Keyence CL-3000 TCP 통신 포트</summary>
        public int KeyencePort { get; set; } = 9004;

        /// <summary>모션 제어기(아진엑스텍 PCIe-R0805-ECAT) 마스터 보드 ID</summary>
        public string MotionControllerIp { get; set; } = "192.168.0.20";

        /// <summary>CAS CI-600A 로드셀 인디케이터 RS-232C 포트 (COM1: 2/3/5번 핀)</summary>
        public string LoadCellComPort { get; set; } = "COM1";

        /// <summary>CAS CI-600A Baud Rate (Menu-2204: 9600 ~ 115200 bps)</summary>
        public int LoadCellBaudRate { get; set; } = 9600;

        /// <summary>CAS CI-600A 장비 번호 / Modbus Slave ID (Menu-2201: 1~99)</summary>
        public int LoadCellSlaveId { get; set; } = 1;

        /// <summary>CAS CI-600A 통신 모드 (Menu-2207: 9=MODBUS-RTU, 5=Command Type1, 3=Stream)</summary>
        public string LoadCellProtocol { get; set; } = "MODBUS-RTU (Menu-2207=9)";

        /// <summary>CAS CI-600A Set Point 1 (Modbus 40017~40018 / Relay OUT1 -> DI4) 목표 접촉 하중 (g)</summary>
        public double LoadCellSetPoint1Gram { get; set; } = 15.0;

        /// <summary>CAS CI-600A Set Point 2 (Modbus 40019~40020 / Relay OUT2 -> DI5) 과부하 안전 상한 (g)</summary>
        public double LoadCellLimitGram { get; set; } = 30.0;

        /// <summary>CAS CI-600A ADC 변환 속도 (Modbus 40061: 200 Hz)</summary>
        public int LoadCellAdcSpeedHz { get; set; } = 200;

        /// <summary>Z축 접촉 정지 1순위 조건 (DI4_RelayOut1 / Modbus_WeightG / CNT2_LinearScale)</summary>
        public string ZStopPrimarySource { get; set; } = "DI4 (CI-600A Relay OUT1 접촉)";

        /// <summary>Z축 접촉 정지 2순위 조건</summary>
        public string ZStopSecondarySource { get; set; } = "CI-600A Modbus 순중량 (40010)";

        /// <summary>Z축 접촉 정지 3순위 조건</summary>
        public string ZStopTertiarySource { get; set; } = "CNT2 Renishaw 리니어스케일 목표치";

        /// <summary>N3ECAT-CNT2 (Product 0x0902) Low-Level 설정 (02-2_N3ECAT_Slave_SW_KOR_V1.1.pdf p.67~90)</summary>
        public int Cnt2EncoderMethodIndex { get; set; } = 3; // 0x2001: 0=Up/Down, 1=1체배, 2=2체배, 3=4체배 (Renishaw 기본 4체배)
        public int Cnt2EncoderDirectionIndex { get; set; } = 0; // 0x2002: 0=CW(정방향), 1=CCW(역방향)
        public double Cnt2ScaleResolutionNm { get; set; } = 50.0; // Renishaw RGH24 1 count = 50 nm (0.00005 mm)
        public double Cnt2PresetTargetMm { get; set; } = 0.000; // 0x7000 Preset Position 목표값 (mm)
        public int Cnt2TriggerModeIndex { get; set; } = 1; // 0x2010: 0=주기 위치(Periodic), 1=절대 위치(Absolute)
        public int Cnt2TriggerLevelIndex { get; set; } = 1; // 0x2011: 0=LOW, 1=HIGH
        public double Cnt2TriggerPulseWidthUs { get; set; } = 100.0; // 0x2012: 1 ~ 500,000 us
        public double Cnt2TriggerComparePosMm { get; set; } = 18.400; // 트리거 비교 목표 위치 (mm)

        /// <summary>3종 센서 3D 기구 오프셋 (CL-3000 기준점 대비 PC2 / F20 상대 거리)</summary>
        public SensorOffsetConfig Cl3000Offset { get; set; } = new SensorOffsetConfig { SensorName = "Keyence CL-3000 (Laser)", OffsetX = 0.0, OffsetY = 0.0, OffsetZ = 0.0 };
        public SensorOffsetConfig MmsPc2Offset { get; set; } = new SensorOffsetConfig { SensorName = "Fischer MMS PC2 (Cu)", OffsetX = 25.0, OffsetY = 0.0, OffsetZ = -2.150 };
        public SensorOffsetConfig F20ExrOffset { get; set; } = new SensorOffsetConfig { SensorName = "Filmetrics F20-EXR (Film)", OffsetX = -25.0, OffsetY = 0.0, OffsetZ = -1.850 };

        /// <summary>F20-EXR Reference Si 웨이퍼 Zero Calibration 지그 좌표 (mm)</summary>
        public double F20CalStageX { get; set; } = 505.0;
        public double F20CalStageY { get; set; } = 15.0;
        public double F20CalStageZ { get; set; } = 12.500;
        public bool F20AutoCalBeforeRecipe { get; set; } = false;

        /// <summary>엔지니어 설정 보호 비밀번호 및 자동 잠금 시간(분)</summary>
        public string AdminPassword { get; set; } = "0000";
        public int AutoLockMinutes { get; set; } = 5;

        /// <summary>측정 결과/로그/레시피 루트 데이터 경로</summary>
        public string DataRootPath { get; set; } = @"D:\MultiScanX_Data";

        /// <summary>저장된 DI/DO/AI 채널 커스텀 명칭 및 스케일 목록</summary>
        public List<IoChannelConfig> CustomDiChannels { get; set; } = new List<IoChannelConfig>();
        public List<IoChannelConfig> CustomDoChannels { get; set; } = new List<IoChannelConfig>();
        public List<AiChannelConfig> CustomAiChannels { get; set; } = new List<AiChannelConfig>();
    }
}
