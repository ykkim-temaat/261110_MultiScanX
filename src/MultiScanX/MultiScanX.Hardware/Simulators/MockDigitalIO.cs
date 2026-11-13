using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Simulators
{
    /// <summary>
    /// 가상 디지털 입출력 시뮬레이터 (진공 척 및 센서 인터록 모의)
    /// </summary>
    public class MockDigitalIO : IDigitalIO
    {
        private uint _diWord = 0x0001; // DI01 (EMO NC) 기본 ON
        private uint _doWord = 0x0008; // DO04 (Tower Green) 기본 ON
        private bool _vacZone1;
        private bool _vacZone2;
        private bool _clampFwd;
        private bool _vacRelease;

        public bool IsHardwareConnected => false;
        public bool Initialize() => true;
        public void Shutdown() { }

        public void SetOutputBit(int bitNo, bool value)
        {
            if (bitNo < 0 || bitNo >= 16) return;
            if (value) _doWord |= (1u << bitNo);
            else _doWord &= ~(1u << bitNo);
        }

        public bool GetOutputBit(int bitNo)
        {
            if (bitNo < 0 || bitNo >= 16) return false;
            return (_doWord & (1u << bitNo)) != 0;
        }

        public bool GetInputBit(int bitNo)
        {
            if (bitNo < 0 || bitNo >= 16) return false;
            return (_diWord & (1u << bitNo)) != 0;
        }

        public uint GetInputWord() => _diWord;
        public uint GetOutputWord() => _doWord;
        public void SetOutputWord(uint wordValue) => _doWord = wordValue & 0xFFFFu;

        public void SetVacuumZone1(bool on) => _vacZone1 = on;
        public void SetVacuumZone2(bool on) => _vacZone2 = on;
        public void SetVacuumRelease(bool on) => _vacRelease = on;
        public void SetClampForward(bool on) => _clampFwd = on;
        public void SetTowerLight(bool red, bool yellow, bool green, bool buzzer) { }

        // 진공 밸브가 켜지면 자동으로 정상 진공 압력 도달된 것으로 가상 응답
        public bool IsVacuumOk() => _vacZone1 || _vacZone2;
        public bool IsEmergencyStop() => false;
        public bool IsDoorClosed() => true;
    }
}
