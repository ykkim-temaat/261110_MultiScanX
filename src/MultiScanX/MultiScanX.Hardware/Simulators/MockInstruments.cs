using System;
using System.Threading.Tasks;
using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Simulators
{
    /// <summary>
    /// 사급 3종 계측기 및 로드셀 가상 시뮬레이터 (Dry-Run 계측값 생성)
    /// </summary>
    public class MockMmsThicknessDriver : IMmsThicknessDriver
    {
        public bool Connect(string portName, int baudRate = 9600) => true;
        public void Disconnect() { }

        public async Task<double> MeasureCuThicknessUmAsync()
        {
            await Task.Delay(50); // 계측 딜레이 50ms
            // 기준 두께 18.5um ± 0.3um 가우시안 랜덤 노이즈
            return 18.5 + (Random.Shared.NextDouble() - 0.5) * 0.6;
        }
    }

    public class MockF20FilmDriver : IF20FilmDriver
    {
        public bool Connect() => true;
        public void Disconnect() { }

        public async Task<(double thicknessNm, double gof)> MeasureFilmAsync(string recipeName)
        {
            await Task.Delay(80); // 광학 계측 딜레이 80ms
            // 기준 박막 두께 125.0nm ± 2.0nm, GOF 0.985
            double thick = 125.0 + (Random.Shared.NextDouble() - 0.5) * 4.0;
            double gof = 0.980 + Random.Shared.NextDouble() * 0.015;
            return (thick, gof);
        }
    }

    public class MockLoadCellDriver : ILoadCellDriver
    {
        private double _mockWeight = 0.0;
        private bool _isDescending = false;

        public bool Connect(string portName, int baudRate = 19200) => true;
        public void Disconnect() { }

        public double GetCurrentWeightG()
        {
            // 프로브 하강 중 가상으로 하중이 점진적 증가하는 시뮬레이션
            _mockWeight += 5.0 + Random.Shared.NextDouble() * 3.0;
            return _mockWeight;
        }

        public void Tare()
        {
            _mockWeight = 0.0;
        }
    }
}
