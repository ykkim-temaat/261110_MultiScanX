using System;
using System.Threading.Tasks;
using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Simulators
{
    /// <summary>
    /// 실물 하드웨어 없이 개발 및 시퀀스 검증이 가능한 가상 3축 모션 제어기 (Dry-Run)
    /// </summary>
    public class MockMotionController : IMotionController
    {
        private readonly double[] _cmdPos = new double[3] { 0, 0, 20.0 };
        private readonly double[] _actPos = new double[3] { 0, 0, 20.0 };
        private readonly bool[] _servoOn = new bool[3] { false, false, false };
        private readonly bool[] _inMotion = new bool[3] { false, false, false };

        public bool Initialize() => true;
        public void Shutdown() { }

        public bool ServoOn(int axis, bool bOn)
        {
            if (axis >= 0 && axis < 3) _servoOn[axis] = bOn;
            return true;
        }

        public bool IsServoOn(int axis) => axis >= 0 && axis < 3 && _servoOn[axis];
        public bool IsInMotion(int axis) => axis >= 0 && axis < 3 && _inMotion[axis];

        public double GetCommandPosition(int axis) => axis >= 0 && axis < 3 ? _cmdPos[axis] : 0.0;
        public double GetActualPosition(int axis) => axis >= 0 && axis < 3 ? _actPos[axis] : 0.0;

        public async Task<bool> HomeAsync(int axis)
        {
            if (axis < 0 || axis >= 3) return false;
            _inMotion[axis] = true;
            await Task.Delay(300); // 가상 원점 복귀 딜레이
            _cmdPos[axis] = 0.0;
            _actPos[axis] = 0.0;
            _inMotion[axis] = false;
            return true;
        }

        public async Task<bool> MovePosAsync(int axis, double targetPosMm, double speedMmPerSec, double accel = 1000.0)
        {
            if (axis < 0 || axis >= 3) return false;
            _inMotion[axis] = true;
            _cmdPos[axis] = targetPosMm;

            double dist = Math.Abs(targetPosMm - _actPos[axis]);
            int durationMs = Math.Max(20, (int)((dist / speedMmPerSec) * 200)); // 시뮬레이션용 스케일링

            int steps = 10;
            double stepDist = (targetPosMm - _actPos[axis]) / steps;
            for (int i = 0; i < steps; i++)
            {
                if (!_inMotion[axis]) break; // 외부에서 Stop 호출 시
                _actPos[axis] += stepDist;
                await Task.Delay(durationMs / steps);
            }

            _actPos[axis] = targetPosMm;
            _inMotion[axis] = false;
            return true;
        }

        public void Stop(int axis)
        {
            if (axis >= 0 && axis < 3) _inMotion[axis] = false;
        }

        public void EmergencyStop(int axis)
        {
            if (axis >= 0 && axis < 3)
            {
                _inMotion[axis] = false;
                _servoOn[axis] = false;
            }
        }

        public double GetLinearScaleZ()
        {
            // 실제 Z축 위치에 미세 물리적 노이즈(±50nm)를 더해 가상 리니어 스케일 값 생성
            double noise = (Random.Shared.NextDouble() - 0.5) * 0.0001;
            return _actPos[2] + noise;
        }
    }
}

