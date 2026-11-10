using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Ajinextek
{
    /// <summary>
    /// 아진엑스텍 N3ECAT-DB32T (16DI/16DO 싱크) DIO 제어기 구현체
    /// </summary>
    public class AjinDigitalIO : IDigitalIO
    {
        private const int ModuleNo = 0; // N3ECAT-DB32T 모듈 번호

        public bool Initialize()
        {
            return CAXL.AxlIsOpened() == 0;
        }

        public void Shutdown()
        {
            // 모든 솔밸브 안전 OFF
            SetOutputBit(0, false);
            SetOutputBit(1, false);
            SetOutputBit(2, false);
            SetOutputBit(3, false);
        }

        public void SetOutputBit(int bitNo, bool value)
        {
            CAXD.AxdoWriteOutportBit(ModuleNo, bitNo, value ? 1u : 0u);
        }

        public bool GetOutputBit(int bitNo)
        {
            uint val = 0;
            CAXD.AxdoReadOutportBit(ModuleNo, bitNo, ref val);
            return val == 1;
        }

        public bool GetInputBit(int bitNo)
        {
            uint val = 0;
            CAXD.AxdiReadInportBit(ModuleNo, bitNo, ref val);
            return val == 1;
        }

        // 전장 도면 Page 13 & 16 매핑
        public void SetVacuumZone1(bool on) => SetOutputBit(0, on);      // OUT1: VX2120 Zone 1 밸브
        public void SetVacuumZone2(bool on) => SetOutputBit(1, on);      // OUT2: VX2120 Zone 2 밸브
        public void SetVacuumRelease(bool on) => SetOutputBit(2, on);    // OUT3: VXEZ2230 진공파괴 밸브
        public void SetClampForward(bool on) => SetOutputBit(3, on);     // OUT4: SY3120 클램프 전진 밸브

        public void SetTowerLight(bool red, bool yellow, bool green, bool buzzer)
        {
            SetOutputBit(4, red);
            SetOutputBit(5, yellow);
            SetOutputBit(6, green);
            SetOutputBit(7, buzzer);
        }

        public bool IsVacuumOk() => GetInputBit(1);                      // IN2: SMC ZSE30A 디지털 압력스위치
        public bool IsEmergencyStop() => !GetInputBit(0);                // IN1: OMRON G9SE-201 안전릴레이 정상(NC)
        public bool IsDoorClosed() => GetInputBit(2);                    // IN3: 도어 인터록 센서
    }
}
