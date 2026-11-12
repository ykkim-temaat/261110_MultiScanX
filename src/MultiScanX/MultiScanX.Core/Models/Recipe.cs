using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiScanX.Core.Models
{
    /// <summary>
    /// 510x515mm 기판 다점 자동 계측 레시피(Recipe) 데이터 모델 (최대 10x10 = 100 Points 지원)
    /// </summary>
    public class Recipe
    {
        public string RecipeName { get; set; } = "KCT_510x515_10x10_Standard";
        public string Description { get; set; } = "KCT向 510×515mm 대면적 기판 10×10 박막두께 자동계측 레시피";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        // 시료 및 기판 규격 사양
        public string SampleModelName { get; set; } = "KCT-PKG-510A";
        public string SampleMaterial { get; set; } = "CCL + Thick Cu / Photoresist";
        public string EtchProcessStage { get; set; } = "Pre-Etch (식각 전)";
        public double SubstrateWidth { get; set; } = 510.0;       // 가로 (mm, 0.1 단위)
        public double SubstrateHeight { get; set; } = 515.0;      // 세로 (mm, 0.1 단위)
        public double SubstrateThickness { get; set; } = 1.600;   // 두께 (mm, 0.001 단위)
        public int GridRows { get; set; } = 10;                   // 격자 행 수 (최대 10)
        public int GridCols { get; set; } = 10;                   // 격자 열 수 (최대 10)

        // 타겟 사양 및 공차
        public double TargetCuThicknessUm { get; set; } = 18.0;
        public double CuTolerancePlusUm { get; set; } = 2.0;
        public double CuToleranceMinusUm { get; set; } = 2.0;

        public double TargetFilmThicknessNm { get; set; } = 1500.0;
        public double FilmMinGof { get; set; } = 0.90; // 최소 적합도(GOF) 기준

        // Z축 프로브 소프트 랜딩 파라미터 (0.001mm 단위 정밀 제어)
        public double ZApproachSpeed { get; set; } = 25.0;        // 고속 접근 속도 (mm/s)
        public double ZCreepSpeed { get; set; } = 0.5;            // 초저속 접촉 속도 (mm/s)
        public double ZRetractHeight { get; set; } = 5.000;       // 안전 대기 높이 (mm)
        public double ZTargetContactPos { get; set; } = 18.400;   // CNT2 목표 접촉 Z위치 (mm)
        public double ContactTargetWeightG { get; set; } = 15.0;  // 로드셀 목표 접촉 하중 SP1 (g)
        public double MaxSafetyWeightG { get; set; } = 30.0;      // 비상 정지 과하중 임계치 SP2 (g)

        // 검사 포인트 목록 (10x10 = 100 포인트 등)
        public List<InspectionPoint> Points { get; set; } = new List<InspectionPoint>();
    }

    /// <summary>
    /// 개별 측정 포인트 좌표 및 센서 활성화 옵션 (실시간 UI 바인딩 지원)
    /// </summary>
    public class InspectionPoint : INotifyPropertyChanged
    {
        private int _pointId;
        private int _rowIndex;
        private int _colIndex;
        private double _x;
        private double _y;
        private double _targetZ = 18.400;
        private int _repeatCount = 1;
        private bool _isEnabled = true;
        private bool _enableMmsCu = true;
        private bool _enableF20Film = true;
        private bool _enableTtvLaser = true;

        public int PointId
        {
            get => _pointId;
            set { _pointId = value; OnPropertyChanged(); }
        }

        public int RowIndex
        {
            get => _rowIndex;
            set { _rowIndex = value; OnPropertyChanged(); OnPropertyChanged(nameof(GridLabel)); }
        }

        public int ColIndex
        {
            get => _colIndex;
            set { _colIndex = value; OnPropertyChanged(); OnPropertyChanged(nameof(GridLabel)); }
        }

        public string GridLabel => $"R{RowIndex}C{ColIndex}";

        /// <summary>X축 좌표 (0.1 mm 단위)</summary>
        public double X
        {
            get => _x;
            set { _x = Math.Round(value, 1); OnPropertyChanged(); }
        }

        /// <summary>Y축 좌표 (0.1 mm 단위, 포러스 척 이동)</summary>
        public double Y
        {
            get => _y;
            set { _y = Math.Round(value, 1); OnPropertyChanged(); }
        }

        /// <summary>Z축 목표 측정 높이 (0.001 mm 단위)</summary>
        public double TargetZ
        {
            get => _targetZ;
            set { _targetZ = Math.Round(value, 3); OnPropertyChanged(); }
        }

        public int RepeatCount
        {
            get => _repeatCount;
            set { _repeatCount = value; OnPropertyChanged(); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public bool EnableMmsCu
        {
            get => _enableMmsCu;
            set { _enableMmsCu = value; OnPropertyChanged(); }
        }

        public bool EnableF20Film
        {
            get => _enableF20Film;
            set { _enableF20Film = value; OnPropertyChanged(); }
        }

        public bool EnableTtvLaser
        {
            get => _enableTtvLaser;
            set { _enableTtvLaser = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
