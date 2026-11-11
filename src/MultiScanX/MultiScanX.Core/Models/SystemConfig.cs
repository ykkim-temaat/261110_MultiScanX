namespace MultiScanX.Core.Models
{
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

        /// <summary>모션 제어기(ACS/EtherCAT 마스터) IP 주소</summary>
        public string MotionControllerIp { get; set; } = "192.168.0.20";

        /// <summary>로드셀 접촉 과부하 안전 한계치 (단위: g)</summary>
        public double LoadCellLimitGram { get; set; } = 30.0;

        /// <summary>측정 결과/로그/레시피 루트 데이터 경로</summary>
        public string DataRootPath { get; set; } = @"D:\MultiScanX_Data";
    }
}

