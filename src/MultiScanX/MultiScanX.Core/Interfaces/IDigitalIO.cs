namespace MultiScanX.Core.Interfaces
{
    /// <summary>
    /// 디지털 입출력(DIO) 제어기 추상화 인터페이스
    /// </summary>
    public interface IDigitalIO
    {
        bool Initialize();
        void Shutdown();

        // 비트 단위 출력
        void SetOutputBit(int bitNo, bool value);
        bool GetOutputBit(int bitNo);

        // 비트 단위 입력
        bool GetInputBit(int bitNo);

        // 전용 유틸리티 헬퍼 (전장 도면 매핑)
        void SetVacuumZone1(bool on);
        void SetVacuumZone2(bool on);
        void SetVacuumRelease(bool on);
        void SetClampForward(bool on);
        void SetTowerLight(bool red, bool yellow, bool green, bool buzzer);

        bool IsVacuumOk();
        bool IsEmergencyStop();
        bool IsDoorClosed();
    }
}

