using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private Recipe _currentRecipe;
        private string _statusMessage = "시스템 대기 중 (Simulation Mode)";
        private double _progressValue = 0.0;
        private bool _isScanning = false;

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

                // 결과 TXT 파일 저장
                string savedPath = _loggerService.ExportToTextFile(result, "Results");
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

