namespace MultiScanX.Core.Models
{
    /// <summary>
    /// 장비 실시간 상태 정보 모델
    /// </summary>
    public class DeviceStatus
    {
        // 3축 모션 상태
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double LinearScaleZ { get; set; } // 50nm 리니어 스케일 현재값

        public bool IsServoOnX { get; set; }
        public bool IsServoOnY { get; set; }
        public bool IsServoOnZ { get; set; }

        public bool IsInMotionX { get; set; }
        public bool IsInMotionY { get; set; }
        public bool IsInMotionZ { get; set; }

        public bool IsHomedX { get; set; }
        public bool IsHomedY { get; set; }
        public bool IsHomedZ { get; set; }

        // 전장 및 안전 상태
        public bool IsEStopActive { get; set; }        // 비상 정지 상태
        public bool IsDoorClosed { get; set; }         // 챔버 도어 닫힘 상태
        public bool IsVacuumOk { get; set; }           // 진공 압력 도달 여부
        public double CurrentVacuumKPa { get; set; }   // 현재 진공도
        public double CurrentLoadCellG { get; set; }   // 로드셀 현재 하중

        // 솔레노이드 밸브 출력 상태
        public bool IsVacZone1On { get; set; }
        public bool IsVacZone2On { get; set; }
        public bool IsClampFwd { get; set; }
        public bool IsBlowOn { get; set; }

        // 장비 운용 모드
        public bool IsSimulationMode { get; set; }
        public string MachineState { get; set; } = "IDLE"; // IDLE, RUNNING, PAUSED, ERROR
    }
}

