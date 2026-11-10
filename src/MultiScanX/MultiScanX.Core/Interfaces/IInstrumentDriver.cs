using System.Threading.Tasks;

namespace MultiScanX.Core.Interfaces
{
    /// <summary>
    /// Thick Cu 측정기 (Fischerscope MMS PC2) 드라이버 인터페이스
    /// </summary>
    public interface IMmsThicknessDriver
    {
        bool Connect(string portName, int baudRate = 9600);
        void Disconnect();
        Task<double> MeasureCuThicknessUmAsync();
    }

    /// <summary>
    /// 광학 박막 측정기 (Filmetrics F20-EXR) 드라이버 인터페이스
    /// </summary>
    public interface IF20FilmDriver
    {
        bool Connect();
        void Disconnect();
        Task<(double thicknessNm, double gof)> MeasureFilmAsync(string recipeName);
    }

    /// <summary>
    /// 로드셀 인디케이터 (CAS CI-600A) 드라이버 인터페이스
    /// </summary>
    public interface ILoadCellDriver
    {
        bool Connect(string portName, int baudRate = 19200);
        void Disconnect();
        double GetCurrentWeightG();
        void Tare(); // 영점 보정
    }
}
