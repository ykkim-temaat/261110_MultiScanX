using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Simulators
{
    /// <summary>
    /// 가상 디지털 입출력 시뮬레이터 (진공 척 및 센서 인터록 모의)
    /// </summary>
    public class MockDigitalIO : IDigitalIO
    {
        private bool _vacZone1;
        private bool _vacZone2;
        private bool _clampFwd;
        private bool _vacRelease;

        public bool Initialize() => true;
        public void Shutdown() { }

        public void SetOutputBit(int bitNo, bool value) { }
        public bool GetOutputBit(int bitNo) => false;
        public bool GetInputBit(int bitNo) => true;

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
