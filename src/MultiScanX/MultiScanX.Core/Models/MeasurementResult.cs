using System;

namespace MultiScanX.Core.Models
{
    /// <summary>
    /// 개별 측정 포인트 계측 결과 모델
    /// </summary>
    public class MeasurementPointResult
    {
        public int PointId { get; set; }
        public string GridLabel { get; set; } = "R1C1";
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double ActualX { get; set; }
        public double ActualY { get; set; }
        public double ActualZ { get; set; } // Renishaw 50nm 리니어 스케일 계측값 (0.001mm 표시)

        // 1. Thick Cu 두께 (Fischer MMS PC2)
        public double CuThicknessUm { get; set; }

        // 2. 광학 투과막 두께 및 적합도 (Filmetrics F20-EXR)
        public double FilmThicknessNm { get; set; }
        public double FilmGoodnessOfFit { get; set; } // GOF (0.0 ~ 1.0)

        // 3. TTV / 단차 변위 (Keyence CL-3000) 및 식각 전/후 비교
        public double TtvDeltaUm { get; set; }
        public double PreEtchHeightUm { get; set; }
        public double PostEtchHeightUm { get; set; }
        public double EtchDepthDeltaUm => Math.Round(PreEtchHeightUm - PostEtchHeightUm, 2);

        // 4. 프로브 접촉 하중 (CAS CI-600A Load Cell)
        public double ContactLoadG { get; set; }
        public string StopTriggerSource { get; set; } = "DI4(SP1)";

        public bool IsPass { get; set; } = true;
        public string Note { get; set; } = "OK";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 전체 기판 1매에 대한 종합 계측 결과 롯(Lot) 데이터
    /// </summary>
    public class SubstrateScanResult
    {
        public string LotId { get; set; } = string.Empty;
        public string SubstrateId { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalCycleTime => EndTime - StartTime;

        public System.Collections.Generic.List<MeasurementPointResult> PointResults { get; set; } 
            = new System.Collections.Generic.List<MeasurementPointResult>();

        // 통계 지표
        public double CuAvgUm { get; set; }
        public double CuMinUm { get; set; }
        public double CuMaxUm { get; set; }
        public double CuStdDevUm { get; set; }

        public double FilmAvgNm { get; set; }
        public double TotalTtvUm { get; set; }
        public bool OverallJudgement { get; set; } = true;
    }
}
