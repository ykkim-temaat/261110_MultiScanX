using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MultiScanX.Core.Models;
using MultiScanX.Core.Services;
using MultiScanX.Hardware.Simulators;

namespace MultiScanX.App.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ScanSequenceService _sequencer;
        private readonly RecipeService _recipeService;
        private readonly DataLoggerService _loggerService;
        private readonly SystemConfigService _configService;

        private Recipe _currentRecipe;
        private string _statusMessage = "시스템 대기 중 (Simulation Mode)";
        private double _progressValue = 0.0;
        private bool _isScanning = false;

        public SystemConfig CurrentConfig { get; }
        public string AppVersion { get; }
        public string AppReleaseNote { get; }
        public string AppTitleHeader => $"🔬 MultiScan-X Studio [v{AppVersion}]";
        public string WindowTitle => $"MultiScan-X Studio [v{AppVersion}] - KCT向 박막두께 자동계측 시스템 (TeMaat)";
        public string OperationModeBadgeText => CurrentConfig.UseSimulationMode
            ? "● Simulation (Dry-Run Mode)"
            : "● Online (Production Mode)";
        public string OperationModeBadgeColor => CurrentConfig.UseSimulationMode ? "#0E639C" : "#107C41";

        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();
        public ObservableCollection<MeasurementPointResult> MeasuredPoints { get; } = new ObservableCollection<MeasurementPointResult>();

        public Recipe CurrentRecipe
        {
            get => _currentRecipe;
            set { _currentRecipe = value; OnPropertyChanged(); }
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

        public MainViewModel()
        {
            // Build-Time 어셈블리 버전 및 코멘트(SSOT) 추출 + Run-Time 설정 파일 로드
            var asm = Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            AppVersion = ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
            AppReleaseNote = asm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;

            _configService = new SystemConfigService();
            CurrentConfig = _configService.LoadOrCreate();

            _statusMessage = CurrentConfig.UseSimulationMode
                ? "시스템 대기 중 (Simulation Mode)"
                : "시스템 대기 중 (Production Mode)";

            // 기본 Mock HAL 시뮬레이터 인젝션 (Dry-Run 준비)
            var mockMotion = new MockMotionController();
            var mockDio = new MockDigitalIO();
            var mockMms = new MockMmsThicknessDriver();
            var mockF20 = new MockF20FilmDriver();
            var mockLoadCell = new MockLoadCellDriver();

            _sequencer = new ScanSequenceService(mockMotion, mockDio, mockMms, mockF20, mockLoadCell);
            _recipeService = new RecipeService();
            _loggerService = new DataLoggerService();

            _currentRecipe = _recipeService.CreateDefault30PointRecipe();

            string noteSuffix = string.IsNullOrWhiteSpace(AppReleaseNote) ? string.Empty : $" | Note: {AppReleaseNote}";
            LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] MultiScan-X Studio v{AppVersion} 초기화 완료{noteSuffix}");
            LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] 설정 로드: {_configService.ActiveConfigPath} (Sim={CurrentConfig.UseSimulationMode}, Keyence={CurrentConfig.KeyenceIp}:{CurrentConfig.KeyencePort})");

            _sequencer.OnLogMessage += msg =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
                    StatusMessage = msg;
                });
            };

            _sequencer.OnProgressChanged += p =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() => ProgressValue = p);
            };

            _sequencer.OnPointCompleted += pt =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() => MeasuredPoints.Add(pt));
            };
        }

        public async Task StartScanAsync()
        {
            if (IsScanning) return;
            IsScanning = true;
            MeasuredPoints.Clear();
            LogMessages.Clear();

            try
            {
                string lotId = $"LOT-{DateTime.Now:yyyyMMdd}";
                string subId = "SUB-510-001";
                var result = await _sequencer.ExecuteScanAsync(CurrentRecipe, lotId, subId);

                // 결과 TXT 파일 저장 (system_config.json의 DataRootPath 하위 Results 폴더 사용, 실패 시 로컬 Results 폴백)
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
                LogMessages.Add($"[결과 저장 완료] {savedPath}");
            }
            catch (Exception ex)
            {
                LogMessages.Add($"[에러 발생] {ex.Message}");
            }
            finally
            {
                IsScanning = false;
            }
        }

        public void StopScan()
        {
            _sequencer.CancelScan();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

