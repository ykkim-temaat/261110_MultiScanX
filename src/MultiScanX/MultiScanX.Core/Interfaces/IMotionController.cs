using System.Threading.Tasks;

namespace MultiScanX.Core.Interfaces
{
    /// <summary>
    /// 모션 제어기 추상화 인터페이스 (아진엑스텍 및 Mock 지원)
    /// </summary>
    public interface IMotionController
    {
        bool Initialize();
        void Shutdown();

        bool ServoOn(int axis, bool bOn);
        bool IsServoOn(int axis);
        bool IsInMotion(int axis);

        double GetCommandPosition(int axis);
        double GetActualPosition(int axis);

        Task<bool> HomeAsync(int axis);
        Task<bool> MovePosAsync(int axis, double targetPosMm, double speedMmPerSec, double accel = 1000.0);
        void Stop(int axis);
        void EmergencyStop(int axis);

        // Z축 50nm 리니어 스케일 직결 카운터 읽기
        double GetLinearScaleZ();
    }
}
