using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MultiScanX.Core.Models;
using MultiScanX.Core.Services;
using MultiScanX.Hardware.Simulators;

namespace MultiScanX.App.ViewModels
{
    /// <summary>
    /// DB32T DI/DO 개별 비트 뷰모델 (동적 명칭 편집 및 ON/OFF 토글 지원)
    /// </summary>
    public class IoBitViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isOn;

        public int Index { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsOutput { get; set; }
        public bool IsReserved { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayTitle)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StateText));
                    OnPropertyChanged(nameof(BadgeBg));
                    OnPropertyChanged(nameof(BadgeFg));
                    OnStateChanged?.Invoke(this);
                }
            }
        }

        public string DisplayTitle => $"{Code}: {Name}";
        public string StateText => IsOn ? "ON (1)" : "OFF (0)";
        public string BadgeBg => IsOn ? (IsOutput ? "#DCFCE7" : "#DBEAFE") : "#F1F5F9";
        public string BadgeFg => IsOn ? (IsOutput ? "#15803D" : "#1D4ED8") : "#64748B";

        public Action<IoBitViewModel>? OnStateChanged { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// EtherCAT 슬레이브 노드 상태 뷰모델 (DIP 스위치 ID 0~6)
    /// </summary>
    public class EtherCatSlaveStatus
    {
        public int NodeId { get; set; }
        public string DipSwitchHex { get; set; } = "0x00";
        public string VendorAndModel { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public string AlState { get; set; } = "OP (0x08)";
        public bool IsOnline { get; set; } = true;
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ScanSequenceService _sequencer;
        private readonly RecipeService _recipeService;
        private readonly DataLoggerService _loggerService;
        private readonly SystemConfigService _configService;
        private readonly Random _rng = new Random();

        private Recipe _currentRecipe;
        private InspectionPoint? _selectedPoint;
        private string _statusMessage = "시스템 대기 중 (Simulation Mode)";
        private double _progressValue = 0.0;
        private bool _isScanning = false;

        // 1. 운전 모드 (Mode 0 ~ Mode 5)
        private int _selectedOperationModeIndex = 4; // 기본 Mode 4: 레시피 자동측정
        public ObservableCollection<string> OperationModes { get; } = new ObservableCollection<string>
        {
            "Mode 0: Virtual Simulation (전체 가상 시운전)",
            "Mode 1: H/W Setup & Manual JOG (수동 제어/점검)",
            "Mode 2: Point Move + Manual Measure (자동이동+수동측정)",
            "Mode 3: Single-Point Auto (단일 좌표 자동측정)",
            "Mode 4: Recipe Auto Scan (레시피 다점 자동측정)",
            "Mode 5: Dry-Run Motion Only (센서 미측정 공운전)"
        };

        public int SelectedOperationModeIndex
        {
            get => _selectedOperationModeIndex;
            set
            {
                _selectedOperationModeIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentOperationModeTitle));
                AddLog($"[운전 모드 변경] {OperationModes[Math.Clamp(value, 0, OperationModes.Count - 1)]}");
            }
        }

        public string CurrentOperationModeTitle => OperationModes[Math.Clamp(_selectedOperationModeIndex, 0, OperationModes.Count - 1)];

        // 2. 시료 로딩 3단계 워크플로우 상태
        private bool _isReadyForLoad = false;
        private bool _isVacuumPumpOn = false;
        private double _vacuumPressureKpa = 0.0;

        public bool IsReadyForLoad
        {
            get => _isReadyForLoad;
            set { _isReadyForLoad = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReadyStepBadge)); }
        }

        public bool IsVacuumPumpOn
        {
            get => _isVacuumPumpOn;
            set
            {
                _isVacuumPumpOn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VacuumButtonText));
                OnPropertyChanged(nameof(VacuumStatusText));
                OnPropertyChanged(nameof(VacuumBadgeBg));
                OnPropertyChanged(nameof(VacuumBadgeFg));
            }
        }

        public double VacuumPressureKpa
        {
            get => _vacuumPressureKpa;
            set { _vacuumPressureKpa = value; OnPropertyChanged(); OnPropertyChanged(nameof(VacuumStatusText)); }
        }

        public string ReadyStepBadge => IsReadyForLoad ? "준비 완료 (원점 대기)" : "미준비";
        public string VacuumButtonText => IsVacuumPumpOn ? "② 진공펌프 OFF (흡착해제)" : "② 진공펌프 ON (시료흡착)";
        public string VacuumStatusText => IsVacuumPumpOn ? $"{VacuumPressureKpa:F1} kPa (DI2 흡착완료)" : "0.0 kPa (대기압 해제)";
        public string VacuumBadgeBg => IsVacuumPumpOn ? "#DCFCE7" : "#FEF3C7";
        public string VacuumBadgeFg => IsVacuumPumpOn ? "#15803D" : "#B45309";

        // 3. 3축 실시간 위치 및 서보/리니어스케일 상태 (X/Y: 0.1mm, Z: 0.001mm)
        private double _currX = 0.0;
        private double _currY = 0.0;
        private double _currZ = 5.000;
        private double _jogStepXY = 10.0;
        private double _jogStepZ = 0.100;
        private bool _isZBrakeReleased = true;

        public double CurrX
        {
            get => _currX;
            set
            {
                _currX = Math.Clamp(Math.Round(value, 1), 0.0, 530.0);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrXFormatted));
                OnPropertyChanged(nameof(HeadCanvasLeft));
            }
        }

        public double CurrY
        {
            get => _currY;
            set
            {
                _currY = Math.Clamp(Math.Round(value, 1), 0.0, 530.0);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrYFormatted));
                OnPropertyChanged(nameof(HeadCanvasTop));
            }
        }

        public double CurrZ
        {
            get => _currZ;
            set
            {
                _currZ = Math.Clamp(Math.Round(value, 3), 0.000, 35.000);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrZFormatted));
                OnPropertyChanged(nameof(Cnt2PulseCount));
                OnPropertyChanged(nameof(Cnt2ScaleFormatted));
            }
        }

        public string CurrXFormatted => $"{CurrX:F1} mm";
        public string CurrYFormatted => $"{CurrY:F1} mm";
        public string CurrZFormatted => $"{CurrZ:F3} mm";
        public int Cnt2PulseCount => (int)Math.Round(CurrZ * 20000.0); // Renishaw 50nm = 20,000 counts/mm
        public string Cnt2ScaleFormatted => $"{CurrZ:F3} mm ({Cnt2PulseCount:N0} cnt @50nm)";

        // 2D 스테이지 캔버스(510x515mm 기준, 여백 20px) 내 현재 헤드 좌표
        public double HeadCanvasLeft => 20.0 + CurrX - 8.0;
        public double HeadCanvasTop => 17.0 + CurrY - 8.0;

        public double JogStepXY
        {
            get => _jogStepXY;
            set { _jogStepXY = value; OnPropertyChanged(); }
        }

        public double JogStepZ
        {
            get => _jogStepZ;
            set { _jogStepZ = value; OnPropertyChanged(); }
        }

        public bool IsZBrakeReleased
        {
            get => _isZBrakeReleased;
            set { _isZBrakeReleased = value; OnPropertyChanged(); OnPropertyChanged(nameof(ZBrakeStatusText)); }
        }

        public string ZBrakeStatusText => IsZBrakeReleased ? "Brake: RELEASED (동작가능)" : "Brake: LOCKED (하강방지)";

        // 4. CAS CI-600A 로드셀 인디케이터 (07-2_141116_CI600A_KOR_UM.pdf 사양 반영)
        private double _currentLoadGram = 0.0;
        private double _tareOffsetGram = 0.0;

        public double CurrentLoadGram
        {
            get => _currentLoadGram;
            set
            {
                _currentLoadGram = Math.Clamp(Math.Round(value, 1), 0.0, 50.0);
                OnPropertyChanged();
                OnPropertyChanged(nameof(NetWeightGram));
                OnPropertyChanged(nameof(Ci600ModbusGrossReg));
                OnPropertyChanged(nameof(Ci600ModbusNetReg));
                OnPropertyChanged(nameof(Ci600ModbusAdcReg));
                OnPropertyChanged(nameof(IsRelayOut1On));
                OnPropertyChanged(nameof(IsRelayOut2On));
                OnPropertyChanged(nameof(LoadCellStatusText));
                OnPropertyChanged(nameof(LoadCellBadgeBg));
                OnPropertyChanged(nameof(LoadCellBadgeFg));

                // DI4 (목표 하중 SP1), DI5 (과부하 상한 SP2) 실시간 연동
                if (DiChannels.Count >= 5)
                {
                    DiChannels[3].IsOn = IsRelayOut1On; // DI4: 로드셀 접점1
                    DiChannels[4].IsOn = IsRelayOut2On; // DI5: 로드셀 접점2 (과부하)
                }

                // 과부하(SP2 초과) 발생 시 자동 Z축 상승 보호 인터록
                if (IsRelayOut2On && CurrZ > CurrentRecipe.ZRetractHeight)
                {
                    AddLog($"[🚨 CI-600A 과부하 인터록 작동] 하중 {NetWeightGram:F1}g >= SP2({CurrentConfig.LoadCellLimitGram:F1}g) (DI5 ON) -> Z축 즉시 정지 및 안전 높이({CurrentRecipe.ZRetractHeight:F3}mm) 상승!");
                    CurrZ = CurrentRecipe.ZRetractHeight;
                }
            }
        }

        public double NetWeightGram => Math.Max(0.0, Math.Round(_currentLoadGram - _tareOffsetGram, 1));
        public bool IsRelayOut1On => NetWeightGram >= CurrentConfig.LoadCellSetPoint1Gram;
        public bool IsRelayOut2On => NetWeightGram >= CurrentConfig.LoadCellLimitGram;

        // CI-600A Modbus-RTU Holding Register 시뮬레이션 표시 (p.72~73)
        public string Ci600ModbusGrossReg => $"40008~09: {(int)(_currentLoadGram * 10)} ({_currentLoadGram:F1} g)";
        public string Ci600ModbusNetReg => $"40010~11: {(int)(NetWeightGram * 10)} ({NetWeightGram:F1} g)";
        public string Ci600ModbusAdcReg => $"40014~15: {12400 + (int)(_currentLoadGram * 850):N0} (200Hz)";
        public string Ci600ModbusSpReg => $"40017(SP1)={CurrentConfig.LoadCellSetPoint1Gram:F1}g | 40019(SP2)={CurrentConfig.LoadCellLimitGram:F1}g";
        public string LoadCellStatusText => IsRelayOut2On
            ? $"🚨 OVERLOAD ({NetWeightGram:F1}g >= {CurrentConfig.LoadCellLimitGram:F1}g, OUT2/DI5 ON)"
            : (IsRelayOut1On ? $"● CONTACT OK ({NetWeightGram:F1}g >= {CurrentConfig.LoadCellSetPoint1Gram:F1}g, OUT1/DI4 ON)" : $"대기 중 ({NetWeightGram:F1} g)");
        public string LoadCellBadgeBg => IsRelayOut2On ? "#FEE2E2" : (IsRelayOut1On ? "#DCFCE7" : "#EFF6FF");
        public string LoadCellBadgeFg => IsRelayOut2On ? "#DC2626" : (IsRelayOut1On ? "#15803D" : "#1D4ED8");

        // 5. 실시간 3대 계측 센서 현재 판독값 (PC2 / F20-EXR / CL-3000)
        private double _liveMmsCuUm = 18.12;
        private double _liveF20FilmNm = 1504.5;
        private double _liveF20Gof = 0.986;
        private double _liveCl3000Um = 1600.25;
        private string _f20CalStatusText = "Zero Cal: 정상 (Ref: Si Wafer @ GOF 0.998)";

        public double LiveMmsCuUm { get => _liveMmsCuUm; set { _liveMmsCuUm = value; OnPropertyChanged(); } }
        public double LiveF20FilmNm { get => _liveF20FilmNm; set { _liveF20FilmNm = value; OnPropertyChanged(); } }
        public double LiveF20Gof { get => _liveF20Gof; set { _liveF20Gof = value; OnPropertyChanged(); } }
        public double LiveCl3000Um { get => _liveCl3000Um; set { _liveCl3000Um = value; OnPropertyChanged(); } }
        public string F20CalStatusText { get => _f20CalStatusText; set { _f20CalStatusText = value; OnPropertyChanged(); } }

        // 6. 레시피 10x10 행/열/전체 미세조정 선택 파라미터
        private int _selectedShiftRowIndex = 0; // 0 = 전체 행(All Rows), 1~10 = Row 1~10
        private int _selectedShiftColIndex = 0; // 0 = 전체 열(All Cols), 1~10 = Col 1~10
        private double _shiftDeltaMm = 0.5;
        private double _shiftDeltaZMm = 0.005;
        private string _pathOptimizationSummary = "Snake 최단 경로 정렬 완료 (총 이동거리: 5,040.0 mm)";

        public ObservableCollection<string> RowFilterOptions { get; } = new ObservableCollection<string>
        {
            "전체 행 (All Rows)", "Row 1", "Row 2", "Row 3", "Row 4", "Row 5", "Row 6", "Row 7", "Row 8", "Row 9", "Row 10"
        };

        public ObservableCollection<string> ColFilterOptions { get; } = new ObservableCollection<string>
        {
            "전체 열 (All Cols)", "Col 1", "Col 2", "Col 3", "Col 4", "Col 5", "Col 6", "Col 7", "Col 8", "Col 9", "Col 10"
        };

        public int SelectedShiftRowIndex
        {
            get => _selectedShiftRowIndex;
            set { _selectedShiftRowIndex = value; OnPropertyChanged(); }
        }

        public int SelectedShiftColIndex
        {
            get => _selectedShiftColIndex;
            set { _selectedShiftColIndex = value; OnPropertyChanged(); }
        }

        public double ShiftDeltaMm
        {
            get => _shiftDeltaMm;
            set { _shiftDeltaMm = Math.Round(value, 1); OnPropertyChanged(); }
        }

        public double ShiftDeltaZMm
        {
            get => _shiftDeltaZMm;
            set { _shiftDeltaZMm = Math.Round(value, 3); OnPropertyChanged(); }
        }

        public string PathOptimizationSummary
        {
            get => _pathOptimizationSummary;
            set { _pathOptimizationSummary = value; OnPropertyChanged(); }
        }

        // 7. 비밀번호 잠금 보호 상태
        private bool _isAdminUnlocked = false;
        public bool IsAdminUnlocked
        {
            get => _isAdminUnlocked;
            set
            {
                _isAdminUnlocked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AdminLockButtonText));
                OnPropertyChanged(nameof(AdminLockBadgeBg));
                OnPropertyChanged(nameof(AdminLockBadgeFg));
            }
        }

        public string AdminLockButtonText => IsAdminUnlocked ? "🔓 엔지니어 잠금해제됨 (5분 후 자동잠금)" : "🔒 엔지니어 설정 잠금 (PW: 0000)";
        public string AdminLockBadgeBg => IsAdminUnlocked ? "#DCFCE7" : "#F1F5F9";
        public string AdminLockBadgeFg => IsAdminUnlocked ? "#15803D" : "#475569";

        // 8. 컬렉션 데이터 (DI 16ch, DO 16ch, EtherCAT 7 Slaves, 측정결과, 로그)
        public ObservableCollection<IoBitViewModel> DiChannels { get; } = new ObservableCollection<IoBitViewModel>();
        public ObservableCollection<IoBitViewModel> DoChannels { get; } = new ObservableCollection<IoBitViewModel>();
        public ObservableCollection<EtherCatSlaveStatus> EtherCatSlaves { get; } = new ObservableCollection<EtherCatSlaveStatus>();
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();
        public ObservableCollection<MeasurementPointResult> MeasuredPoints { get; } = new ObservableCollection<MeasurementPointResult>();

        public SystemConfig CurrentConfig { get; }
        public string AppVersion { get; }
        public string AppReleaseNote { get; }
        public string AppTitleHeader => $"🔬 MultiScan-X Studio [v{AppVersion}]";
        public string WindowTitle => $"MultiScan-X Studio [v{AppVersion}] - KCT向 510×515mm 박막두께 자동계측 시스템 (2560×1440 QHD Ready)";
        public string OperationModeBadgeText => CurrentConfig.UseSimulationMode
            ? "● Simulation (Dry-Run Mode)"
            : "● Online (Production Mode)";

        public Recipe CurrentRecipe
        {
            get => _currentRecipe;
            set
            {
                _currentRecipe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RecipeSummaryText));
            }
        }

        public string RecipeSummaryText => $"{CurrentRecipe.RecipeName} ({CurrentRecipe.GridRows}×{CurrentRecipe.GridCols} = {CurrentRecipe.Points.Count} Pts | {CurrentRecipe.SubstrateWidth:F1}×{CurrentRecipe.SubstrateHeight:F1}×{CurrentRecipe.SubstrateThickness:F3}mm)";

        public InspectionPoint? SelectedPoint
        {
            get => _selectedPoint;
            set { _selectedPoint = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(); }
        }

        public bool IsScanning
        {
            get => _isScanning;
            set { _isScanning = value; OnPropertyChanged(); }
        }

        // 통계 요약 프로퍼티
        public double StatCuAvg => MeasuredPoints.Count > 0 ? Math.Round(MeasuredPoints.Average(p => p.CuThicknessUm), 3) : 18.120;
        public double StatFilmAvg => MeasuredPoints.Count > 0 ? Math.Round(MeasuredPoints.Average(p => p.FilmThicknessNm), 1) : 1504.2;
        public double StatEtchDepthAvg => MeasuredPoints.Count > 0 ? Math.Round(MeasuredPoints.Average(p => p.EtchDepthDeltaUm), 2) : 18.04;

        public MainViewModel()
        {
            var asm = Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            AppVersion = ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.1.0";
            AppReleaseNote = asm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;

            _configService = new SystemConfigService();
            CurrentConfig = _configService.LoadOrCreate();

            var mockMotion = new MockMotionController();
            var mockDio = new MockDigitalIO();
            var mockMms = new MockMmsThicknessDriver();
            var mockF20 = new MockF20FilmDriver();
            var mockLoadCell = new MockLoadCellDriver();

            _sequencer = new ScanSequenceService(mockMotion, mockDio, mockMms, mockF20, mockLoadCell);
            _recipeService = new RecipeService();
            _loggerService = new DataLoggerService();

            // 기본 10x10 (100 Points) 마스터 레시피 로드
            _currentRecipe = _recipeService.Create10x10GridRecipe();
            _selectedPoint = _currentRecipe.Points.FirstOrDefault();

            InitializeHardwareChannels();
            PopulateSamplePrePostResults();

            string noteSuffix = string.IsNullOrWhiteSpace(AppReleaseNote) ? string.Empty : $" | {AppReleaseNote}";
            AddLog($"MultiScan-X Studio v{AppVersion} 기동 완료{noteSuffix}");
            AddLog($"설정 로드: {_configService.ActiveConfigPath} | CI-600A={CurrentConfig.LoadCellComPort} ({CurrentConfig.LoadCellBaudRate}bps, {CurrentConfig.LoadCellProtocol})");

            _sequencer.OnLogMessage += msg =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    AddLog(msg);
                    StatusMessage = msg;
                });
            };

            _sequencer.OnProgressChanged += p =>
            {
                Application.Current?.Dispatcher.Invoke(() => ProgressValue = p);
            };

            _sequencer.OnPositionUpdated += (x, y, z) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrX = x;
                    CurrY = y;
                    CurrZ = z;
                });
            };

            _sequencer.OnPointCompleted += pt =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    MeasuredPoints.Add(pt);
                    LiveMmsCuUm = pt.CuThicknessUm;
                    LiveF20FilmNm = pt.FilmThicknessNm;
                    LiveF20Gof = pt.FilmGoodnessOfFit;
                    LiveCl3000Um = pt.PreEtchHeightUm;
                    OnPropertyChanged(nameof(StatCuAvg));
                    OnPropertyChanged(nameof(StatFilmAvg));
                    OnPropertyChanged(nameof(StatEtchDepthAvg));
                });
            };
        }

        private void InitializeHardwareChannels()
        {
            // EtherCAT 7 Slaves (ID 0 ~ 6)
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 0, DipSwitchHex = "0x00", VendorAndModel = "Panasonic MINAS-A6B (400W)", RoleDescription = "X축 갠트리 서보 (POT/NOT/HOME)" });
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 1, DipSwitchHex = "0x01", VendorAndModel = "Panasonic MINAS-A6B (400W)", RoleDescription = "Y축 포러스 척 서보 (POT/NOT/HOME)" });
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 2, DipSwitchHex = "0x02", VendorAndModel = "FASTECH Ezi-SERVO II Plus-E", RoleDescription = "Z축 3센서 헤드 스텝 (Full-Closed + 브레이크)" });
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 3, DipSwitchHex = "0x03", VendorAndModel = "Ajinextek N3ECAT-CNT2", RoleDescription = "CH0: Renishaw 50nm Z축 리니어 스케일" });
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 4, DipSwitchHex = "0x04", VendorAndModel = "Ajinextek N3ECAT-DB32T", RoleDescription = "디지털 입출력 (16 DI / 16 DO)" });
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 5, DipSwitchHex = "0x05", VendorAndModel = "Ajinextek N3ECAT-AI16-AO8", RoleDescription = "아날로그 입출력 (16 AI / 8 AO)" });
            EtherCatSlaves.Add(new EtherCatSlaveStatus { NodeId = 6, DipSwitchHex = "0x06", VendorAndModel = "Keyence CL-3000 + DL-EC1A", RoleDescription = "공초점 레이저 변위 센서 (EtherCAT/TCP)" });

            // DB32T DI 16채널 (확정 사양 DI1~DI5 + 예비 DI6~DI16)
            string[] diNames =
            {
                "비상정지 (EMO)", "진공 흡착 확인 (Vac OK)", "진공 알람 (Vac Alarm)", "CI-600A 접점1 (SP1 목표하중)",
                "CI-600A 접점2 (SP2 과부하)", "예비 입력 06 (Spare)", "예비 입력 07 (Spare)", "예비 입력 08 (Spare)",
                "예비 입력 09 (Spare)", "예비 입력 10 (Spare)", "예비 입력 11 (Spare)", "예비 입력 12 (Spare)",
                "예비 입력 13 (Spare)", "예비 입력 14 (Spare)", "예비 입력 15 (Spare)", "예비 입력 16 (Spare)"
            };

            for (int i = 0; i < 16; i++)
            {
                DiChannels.Add(new IoBitViewModel
                {
                    Index = i + 1,
                    Code = $"DI{i + 1:D2}",
                    Name = diNames[i],
                    IsOutput = false,
                    IsReserved = i >= 5,
                    IsOn = (i == 0) // DI1(EMO Normal NC) 기본 ON
                });
            }

            // DB32T DO 16채널 (확정 사양 DO1~DO6 + 예비 DO7~DO16)
            string[] doNames =
            {
                "EMO 리셋 스위치 램프", "타워램프 - 적색 (RED)", "타워램프 - 황색 (YELLOW)", "타워램프 - 녹색 (GREEN)",
                "타워램프 - 부저 (BUZZER)", "포러스 척 진공 펌프 ON", "예비 출력 07 (Spare)", "예비 출력 08 (Spare)",
                "예비 출력 09 (Spare)", "예비 출력 10 (Spare)", "예비 출력 11 (Spare)", "예비 출력 12 (Spare)",
                "예비 출력 13 (Spare)", "예비 출력 14 (Spare)", "예비 출력 15 (Spare)", "예비 출력 16 (Spare)"
            };

            for (int i = 0; i < 16; i++)
            {
                var bit = new IoBitViewModel
                {
                    Index = i + 1,
                    Code = $"DO{i + 1:D2}",
                    Name = doNames[i],
                    IsOutput = true,
                    IsReserved = i >= 6,
                    IsOn = (i == 3) // DO4 (타워램프 GREEN) 기본 ON
                };
                bit.OnStateChanged = OnDoBitToggled;
                DoChannels.Add(bit);
            }
        }

        private void OnDoBitToggled(IoBitViewModel bit)
        {
            if (bit.Index == 6) // DO06: 진공 펌프 연동
            {
                IsVacuumPumpOn = bit.IsOn;
                VacuumPressureKpa = bit.IsOn ? -78.5 : 0.0;
                if (DiChannels.Count >= 2)
                {
                    DiChannels[1].IsOn = bit.IsOn; // DI02: 진공 흡착 확인 자동 연동
                }
            }
            AddLog($"[DO 수동/시퀀스 출력] {bit.Code} ({bit.Name}) -> {bit.StateText}");
        }

        private void PopulateSamplePrePostResults()
        {
            // 초기 실행 시 2D 맵 및 데이터 테이블/식각 비교 화면에서 즉시 확인 가능한 샘플 측정 데이터 10점 생성
            MeasuredPoints.Clear();
            foreach (var pt in CurrentRecipe.Points.Take(12))
            {
                double preH = Math.Round(1600.0 + Math.Sin(pt.X * 0.015) * 3.2, 2);
                double postH = Math.Round(preH - (18.1 + (_rng.NextDouble() - 0.5) * 0.4), 2);
                MeasuredPoints.Add(new MeasurementPointResult
                {
                    PointId = pt.PointId,
                    GridLabel = pt.GridLabel,
                    TargetX = pt.X,
                    TargetY = pt.Y,
                    ActualX = pt.X,
                    ActualY = pt.Y,
                    ActualZ = pt.TargetZ,
                    CuThicknessUm = Math.Round(18.10 + (_rng.NextDouble() - 0.5) * 0.35, 3),
                    FilmThicknessNm = Math.Round(1502.0 + (_rng.NextDouble() - 0.5) * 14.0, 1),
                    FilmGoodnessOfFit = Math.Round(0.975 + _rng.NextDouble() * 0.022, 3),
                    TtvDeltaUm = Math.Round(preH - 1600.0, 2),
                    PreEtchHeightUm = preH,
                    PostEtchHeightUm = postH,
                    ContactLoadG = 15.2,
                    StopTriggerSource = "DI4(SP1:15g)",
                    IsPass = true,
                    Note = "OK"
                });
            }
        }

        // === 시료 로딩 3단계 워크플로우 메서드 ===
        public void ExecuteStep1PrepareForLoading()
        {
            if (IsScanning) return;
            // 진공 OFF -> Z축 상승 -> XY 원점(시료 장착 위치) 복귀
            if (DoChannels.Count >= 6) DoChannels[5].IsOn = false;
            IsVacuumPumpOn = false;
            VacuumPressureKpa = 0.0;
            CurrZ = CurrentRecipe.ZRetractHeight;
            CurrX = 0.0;
            CurrY = 515.0; // 포러스 척 전면 배출(시료 장착 위치)
            IsReadyForLoad = true;
            StatusMessage = "① 측정준비 완료: 진공 해제 및 포러스 척 시료 장착 위치(X=0, Y=515, Z=5.000) 이동 완료";
            AddLog(StatusMessage);
        }

        public void ExecuteStep2ToggleVacuum()
        {
            bool nextState = !IsVacuumPumpOn;
            if (DoChannels.Count >= 6)
            {
                DoChannels[5].IsOn = nextState; // OnDoBitToggled에서 DI2 및 압력 연동
            }
            StatusMessage = nextState
                ? "② 포러스 척 진공 펌프(DO06) ON -> 흡착 압력 -78.5 kPa 도달 (DI02 흡착확인 ON)"
                : "② 포러스 척 진공 펌프(DO06) OFF -> 대기압 해제 완료";
        }

        public async Task StartScanAsync()
        {
            if (IsScanning) return;

            // 진공 미흡착 시 자동 흡착 안내 및 활성화
            if (!IsVacuumPumpOn)
            {
                ExecuteStep2ToggleVacuum();
            }

            IsScanning = true;
            MeasuredPoints.Clear();

            bool isDryRun = (SelectedOperationModeIndex == 5);
            try
            {
                string lotId = $"LOT-{DateTime.Now:yyyyMMdd}";
                string subId = CurrentRecipe.SampleModelName;
                var result = await _sequencer.ExecuteScanAsync(CurrentRecipe, lotId, subId, isDryRun);

                string resultsDir = Path.Combine(CurrentConfig.DataRootPath, "Results");
                string savedPath;
                try
                {
                    savedPath = _loggerService.ExportToTextFile(result, resultsDir);
                }
                catch
                {
                    savedPath = _loggerService.ExportToTextFile(result, "Results");
                }
                AddLog($"[결과 파일 저장] {savedPath}");
            }
            catch (Exception ex)
            {
                AddLog($"[측정 중단/에러] {ex.Message}");
            }
            finally
            {
                IsScanning = false;
            }
        }

        public void StopScan()
        {
            _sequencer.CancelScan();
            CurrZ = CurrentRecipe.ZRetractHeight;
            StatusMessage = "측정 중지 및 Z축 안전 대기 높이 복귀 완료";
        }

        // === 수동 JOG / 원점 / 단일점 이동 및 측정 메서드 ===
        public void JogAxis(string axis, double direction)
        {
            if (axis == "X") CurrX += direction * JogStepXY;
            else if (axis == "Y") CurrY += direction * JogStepXY;
            else if (axis == "Z") CurrZ += direction * JogStepZ;

            StatusMessage = $"[JOG {axis}] 현재 위치 -> X={CurrX:F1}mm, Y={CurrY:F1}mm, Z={CurrZ:F3}mm";
            AddLog(StatusMessage);
        }

        public void HomeAllAxes()
        {
            CurrZ = CurrentRecipe.ZRetractHeight;
            CurrX = 0.0;
            CurrY = 0.0;
            StatusMessage = "[HOME 완료] Z축 안전 상승 후 X/Y/Z 전 축 원점 복귀 완료 (CNT2 Z-Scale 영점 동기화)";
            AddLog(StatusMessage);
        }

        public void MoveToSelectedPoint()
        {
            if (SelectedPoint == null) return;
            CurrZ = CurrentRecipe.ZRetractHeight;
            CurrX = SelectedPoint.X;
            CurrY = SelectedPoint.Y;
            StatusMessage = $"[Mode 2 좌표 이동] Pt #{SelectedPoint.PointId} ({SelectedPoint.GridLabel}) -> X={CurrX:F1}mm, Y={CurrY:F1}mm 이동 완료";
            AddLog(StatusMessage);
        }

        public void MeasureSelectedPointOnce()
        {
            if (SelectedPoint == null) return;
            MoveToSelectedPoint();
            CurrZ = SelectedPoint.TargetZ;
            CurrentLoadGram = CurrentConfig.LoadCellSetPoint1Gram + 0.3;

            double preH = Math.Round(1600.0 + Math.Sin(SelectedPoint.X * 0.02) * 3.5, 2);
            double postH = Math.Round(preH - 18.15, 2);
            var ptRes = new MeasurementPointResult
            {
                PointId = SelectedPoint.PointId,
                GridLabel = SelectedPoint.GridLabel,
                TargetX = SelectedPoint.X,
                TargetY = SelectedPoint.Y,
                ActualX = CurrX,
                ActualY = CurrY,
                ActualZ = CurrZ,
                CuThicknessUm = Math.Round(18.12 + (_rng.NextDouble() - 0.5) * 0.2, 3),
                FilmThicknessNm = Math.Round(1505.0 + (_rng.NextDouble() - 0.5) * 10.0, 1),
                FilmGoodnessOfFit = 0.991,
                TtvDeltaUm = Math.Round(preH - 1600.0, 2),
                PreEtchHeightUm = preH,
                PostEtchHeightUm = postH,
                ContactLoadG = NetWeightGram,
                StopTriggerSource = CurrentConfig.ZStopPrimarySource,
                IsPass = true,
                Note = "SINGLE-OK"
            };
            MeasuredPoints.Insert(0, ptRes);
            LiveMmsCuUm = ptRes.CuThicknessUm;
            LiveF20FilmNm = ptRes.FilmThicknessNm;
            LiveF20Gof = ptRes.FilmGoodnessOfFit;
            LiveCl3000Um = ptRes.PreEtchHeightUm;

            CurrZ = CurrentRecipe.ZRetractHeight;
            CurrentLoadGram = 0.0;
            StatusMessage = $"[Mode 3 단일점 측정 완료] Pt #{ptRes.PointId}({ptRes.GridLabel}): Cu={ptRes.CuThicknessUm:F3}um, Film={ptRes.FilmThicknessNm:F1}nm (GOF {ptRes.FilmGoodnessOfFit:F3})";
            AddLog(StatusMessage);
        }

        // === CI-600A 로드셀 명령 메서드 ===
        public void ZeroLoadCell()
        {
            _tareOffsetGram = 0.0;
            CurrentLoadGram = 0.0;
            AddLog($"[CI-600A 명령 전송] '01MZ\\r\\n' (Modbus 40037 EX_IN1 영점 실행) -> 현재 중량 0.0g 초기화");
        }

        public void TareLoadCell()
        {
            _tareOffsetGram = _currentLoadGram;
            OnPropertyChanged(nameof(NetWeightGram));
            OnPropertyChanged(nameof(Ci600ModbusNetReg));
            AddLog($"[CI-600A 명령 전송] '01MT\\r\\n' (용기 Tare 설정: {_tareOffsetGram:F1}g 차감) -> 순중량 {NetWeightGram:F1}g");
        }

        public void ApplyLoadCellSetPoints()
        {
            OnPropertyChanged(nameof(Ci600ModbusSpReg));
            OnPropertyChanged(nameof(IsRelayOut1On));
            OnPropertyChanged(nameof(IsRelayOut2On));
            OnPropertyChanged(nameof(LoadCellStatusText));
            AddLog($"[CI-600A Modbus Preset (0x10)] 40017(SP1)={CurrentConfig.LoadCellSetPoint1Gram:F1}g (OUT1->DI4), 40019(SP2)={CurrentConfig.LoadCellLimitGram:F1}g (OUT2->DI5) 적용 완료");
        }

        // === 10x10 레시피 편집 및 행/열 미세조정 메서드 ===
        public void SwitchRecipePreset(bool use10x10)
        {
            CurrentRecipe = use10x10
                ? _recipeService.Create10x10GridRecipe()
                : _recipeService.CreateDefault30PointRecipe();
            SelectedPoint = CurrentRecipe.Points.FirstOrDefault();
            PopulateSamplePrePostResults();
            AddLog($"[레시피 프리셋 로드] {RecipeSummaryText}");
        }

        public void ShiftRecipePoints(double dx, double dy, double dz = 0.0)
        {
            int? targetRow = SelectedShiftRowIndex == 0 ? null : SelectedShiftRowIndex;
            int? targetCol = SelectedShiftColIndex == 0 ? null : SelectedShiftColIndex;
            int count = _recipeService.ShiftPoints(CurrentRecipe, targetRow, targetCol, dx, dy, dz);

            string scope = $"{RowFilterOptions[SelectedShiftRowIndex]} / {ColFilterOptions[SelectedShiftColIndex]}";
            StatusMessage = $"[레시피 좌표 미세조정] {scope} ({count}개 포인트) -> dX={dx:+0.0;-0.0;0.0}mm, dY={dy:+0.0;-0.0;0.0}mm, dZ={dz:+0.000;-0.000;0.000}mm 반영";
            AddLog(StatusMessage);
        }

        public void OptimizeRecipePath()
        {
            var (beforeDist, afterDist) = _recipeService.OptimizeSnakeOrder(CurrentRecipe);
            PathOptimizationSummary = $"Snake 최단 경로 정렬 완료: {beforeDist:N1} mm ➔ {afterDist:N1} mm";
            AddLog($"[경로 최적화] {PathOptimizationSummary}");
        }

        public void SaveCurrentConfigAndRecipe()
        {
            _configService.Save(CurrentConfig);
            string recipePath = Path.Combine(@"C:\MultiScanX\config", $"{CurrentRecipe.RecipeName}.json");
            try
            {
                _recipeService.SaveRecipe(CurrentRecipe, recipePath);
            }
            catch { /* ignore path error in sandbox */ }

            StatusMessage = $"시스템 설정(system_config.json) 및 레시피({CurrentRecipe.RecipeName}) 저장 완료";
            AddLog(StatusMessage);
        }

        // === F20-EXR Zero Calibration 실행 ===
        public void ExecuteF20ZeroCalibration()
        {
            CurrZ = CurrentRecipe.ZRetractHeight;
            CurrX = CurrentConfig.F20CalStageX;
            CurrY = CurrentConfig.F20CalStageY;
            CurrZ = CurrentConfig.F20CalStageZ;
            F20CalStatusText = $"Zero Cal 완료 ({DateTime.Now:HH:mm:ss} @ X={CurrX:F1}, Y={CurrY:F1}, Z={CurrZ:F3} | Si Ref GOF=0.999)";
            StatusMessage = $"[F20-EXR Zero Calibration] Cal 지그(X={CurrX:F1}, Y={CurrY:F1}, Z={CurrZ:F3}) 이동 및 Reference/Dark 스펙트럼 보정 완료";
            AddLog(StatusMessage);
            CurrZ = CurrentRecipe.ZRetractHeight;
        }

        private void AddLog(string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            LogMessages.Insert(0, line);
            if (LogMessages.Count > 200)
            {
                LogMessages.RemoveAt(LogMessages.Count - 1);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
