using System;
using System.Threading.Tasks;
using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Ajinextek
{
    /// <summary>
    /// 아진엑스텍 PCIe-R0805-ECAT-E 8축 마스터 실물 제어기 구현체
    /// </summary>
    public class AjinMotionController : IMotionController
    {
        private bool _isInitialized;

        public bool Initialize()
        {
            try
            {
                int ret = CAXL.AxlOpen(7);
                _isInitialized = (ret == 0);
                return _isInitialized;
            }
            catch (DllNotFoundException)
            {
                // AXL.dll이 시스템에 없는 경우
                return false;
            }
        }

        public void Shutdown()
        {
            if (_isInitialized)
            {
                CAXL.AxlClose();
                _isInitialized = false;
            }
        }

        public bool ServoOn(int axis, bool bOn)
        {
            if (!_isInitialized) return false;
            return CAXM.AxmSignalServoOn(axis, bOn ? 1u : 0u) == 0;
        }

        public bool IsServoOn(int axis)
        {
            if (!_isInitialized) return false;
            uint on = 0;
            CAXM.AxmSignalIsServoOn(axis, ref on);
            return on == 1;
        }

        public bool IsInMotion(int axis)
        {
            if (!_isInitialized) return false;
            uint moving = 0;
            CAXM.AxmStatusReadInMotion(axis, ref moving);
            return moving == 1;
        }

        public double GetCommandPosition(int axis)
        {
            if (!_isInitialized) return 0.0;
            double pos = 0.0;
            CAXM.AxmStatusReadCmdPos(axis, ref pos);
            return pos;
        }

        public double GetActualPosition(int axis)
        {
            if (!_isInitialized) return 0.0;
            double pos = 0.0;
            CAXM.AxmStatusReadActPos(axis, ref pos);
            return pos;
        }

        public async Task<bool> HomeAsync(int axis)
        {
            if (!_isInitialized) return false;
            CAXM.AxmHomeSetStart(axis);

            return await Task.Run(async () =>
            {
                uint mainStep = 0, step = 0, result = 0;
                while (true)
                {
                    CAXM.AxmHomeGetResult(axis, ref mainStep, ref step, ref result);
                    if (result == 1) return true;  // 성공
                    if (result == 2) return false; // 실패
                    await Task.Delay(50);
                }
            });
        }

        public async Task<bool> MovePosAsync(int axis, double targetPosMm, double speedMmPerSec, double accel = 1000.0)
        {
            if (!_isInitialized) return false;

            // S-Curve 프로파일 위치 이동 지령
            int ret = CAXM.AxmMovePos(axis, targetPosMm, speedMmPerSec, accel, accel);
            if (ret != 0) return false;

            // 이동 완료 감시 비동기 대기
            return await Task.Run(async () =>
            {
                while (IsInMotion(axis))
                {
                    await Task.Delay(10);
                }
                return true;
            });
        }

        public void Stop(int axis)
        {
            if (_isInitialized) CAXM.AxmMoveSStop(axis);
        }

        public void EmergencyStop(int axis)
        {
            if (_isInitialized) CAXM.AxmMoveEStop(axis);
        }

        public double GetLinearScaleZ()
        {
            if (!_isInitialized) return 0.0;
            double count = 0.0;
            // N3ECAT-CNT2 채널 0 (Renishaw 50nm 리니어 스케일)
            CAXC.AxcStatusReadCount(0, ref count);
            return count * 0.00005; // 1 count = 50nm = 0.00005mm
        }
    }
}

