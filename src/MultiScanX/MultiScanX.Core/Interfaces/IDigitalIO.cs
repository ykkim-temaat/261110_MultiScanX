namespace MultiScanX.Core.Interfaces
{
    /// <summary>
    /// 디지털 입출력(DIO: N3ECAT-DB32T 16DI/16DO) 제어기 추상화 인터페이스
    /// </summary>
    public interface IDigitalIO
    {
        bool IsHardwareConnected { get; }
        bool Initialize();
        void Shutdown();

        // 비트 단위 입출력
        void SetOutputBit(int bitNo, bool value);
        bool GetOutputBit(int bitNo);
        bool GetInputBit(int bitNo);

        // 16비트 워드 단위 일괄 읽기 (TxPDO 0x6000 / RxPDO 0x7010 고속 폴링용)
        uint GetInputWord();
        uint GetOutputWord();
        void SetOutputWord(uint wordValue);

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
