using System;
using System.Collections.Generic;

namespace MultiScanX.Core.Models
{
    /// <summary>
    /// 기판 박막 두께 자동 계측 검사 레시피 모델
    /// </summary>
    public class Recipe
    {
        public string RecipeName { get; set; } = "Default_510x515_30Pt";
        public string Description { get; set; } = "KCT向 대면적 기판 박막두께 자동계측 레시피";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        // 기판 규격 사양 (단위: mm)
        public double SubstrateWidth { get; set; } = 510.0;
        public double SubstrateHeight { get; set; } = 515.0;

        // 정렬 및 진공 척 설정
        public string AlignMode { get; set; } = "OpticalEdgeAlign"; // OpticalEdgeAlign or VisionPattern
        public bool UseVacuumZone1 { get; set; } = true;
        public bool UseVacuumZone2 { get; set; } = true;
        public double TargetVacuumKPa { get; set; } = -75.0;
        public bool UseMechanicalClamp { get; set; } = true;

        // Z축 모션 및 프로브 보호 안전 파라미터
        public double ZApproachSpeed { get; set; } = 50.0;       // 고속 접근 속도 (mm/s)
        public double ZCreepSpeed { get; set; } = 0.5;           // 초저속 접촉 속도 (mm/s)
        public double ZRetractHeight { get; set; } = 20.0;       // 안전 대기 높이 (mm)
        public double ContactTargetWeightG { get; set; } = 30.0; // 로드셀 목표 접촉 하중 (g)
        public double MaxSafetyWeightG { get; set; } = 80.0;     // 비상 정지 과하중 임계치 (g)

        // 검사 포인트 목록 (최대 30+ 포인트)
        public List<InspectionPoint> Points { get; set; } = new List<InspectionPoint>();
    }

    /// <summary>
    /// 개별 검사 좌표 포인트 모델
    /// </summary>
    public class InspectionPoint
    {
        public int PointId { get; set; }
        public double X { get; set; } // mm
        public double Y { get; set; } // mm
        public int RepeatCount { get; set; } = 3; // 포인트당 반복 측정 횟수

        public bool EnableMmsCu { get; set; } = true;   // Fischer MMS PC2 Cu 두께 측정 여부
        public bool EnableF20Film { get; set; } = true; // Filmetrics F20 광학투과막 측정 여부
        public bool EnableTtvLaser { get; set; } = true;// TTV 레이저 변위 측정 여부
    }
}
