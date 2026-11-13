using System;
using MultiScanX.Core.Interfaces;

namespace MultiScanX.Hardware.Ajinextek
{
    /// <summary>
    /// 아진엑스텍 N3ECAT-DB32T (Product Code 0x00000332: 16DI [0x6000] / 16DO [0x7010]) 실물 DIO 제어기
    /// </summary>
    public class AjinDigitalIO : IDigitalIO
    {
        private int _moduleNo = 0;
        private bool _isConnected = false;

        public bool IsHardwareConnected => _isConnected;
        public int DetectedModuleCount { get; private set; }

        public bool Initialize()
        {
            try
            {
                if (CAXL.AxlIsOpened() == 0)
                {
                    int openRet = CAXL.AxlOpen(7);
                    if (openRet != 0)
                    {
                        _isConnected = false;
                        return false;
                    }
                }

                int modCount = 0;
                int ret = CAXD.AxdInfoGetModuleCount(ref modCount);
                DetectedModuleCount = modCount;
                _isConnected = (ret == 0 && modCount > 0);
                return _isConnected;
            }
            catch (Exception)
            {
                _isConnected = false;
                return false;
            }
        }

        public void Shutdown()
        {
            if (!_isConnected) return;
            try
            {
                // 안전을 위해 진공/솔밸브 출력 OFF
                SetOutputBit(0, false);
                SetOutputBit(1, false);
                SetOutputBit(2, false);
                SetOutputBit(5, false);
            }
            catch { }
        }

        public void SetOutputBit(int bitNo, bool value)
        {
            if (!_isConnected) return;
            try
            {
                CAXD.AxdoWriteOutportBit(_moduleNo, bitNo, value ? 1u : 0u);
            }
            catch { }
        }

        public bool GetOutputBit(int bitNo)
        {
            if (!_isConnected) return false;
            try
            {
                uint val = 0;
                CAXD.AxdoReadOutportBit(_moduleNo, bitNo, ref val);
                return val != 0;
            }
            catch
            {
                return false;
            }
        }

        public bool GetInputBit(int bitNo)
        {
            if (!_isConnected) return false;
            try
            {
                uint val = 0;
                CAXD.AxdiReadInportBit(_moduleNo, bitNo, ref val);
                return val != 0;
            }
            catch
            {
                return false;
            }
        }

        public uint GetInputWord()
        {
            if (!_isConnected) return 0;
            try
            {
                uint val = 0;
                CAXD.AxdiReadInportWord(_moduleNo, 0, ref val);
                return val & 0xFFFFu;
            }
            catch
            {
                return 0;
            }
        }

        public uint GetOutputWord()
        {
            if (!_isConnected) return 0;
            try
            {
                uint val = 0;
                CAXD.AxdoReadOutportWord(_moduleNo, 0, ref val);
                return val & 0xFFFFu;
            }
            catch
            {
                return 0;
            }
        }

        public void SetOutputWord(uint wordValue)
        {
            if (!_isConnected) return;
            try
            {
                CAXD.AxdoWriteOutportWord(_moduleNo, 0, wordValue & 0xFFFFu);
            }
            catch { }
        }

        // 전장 도면 매핑 헬퍼
        public void SetVacuumZone1(bool on) => SetOutputBit(5, on);      // DO06: 포러스 척 진공 펌프 ON
        public void SetVacuumZone2(bool on) => SetOutputBit(5, on);
        public void SetVacuumRelease(bool on) => SetOutputBit(6, on);
        public void SetClampForward(bool on) => SetOutputBit(7, on);

        public void SetTowerLight(bool red, bool yellow, bool green, bool buzzer)
        {
            SetOutputBit(1, red);    // DO02: RED
            SetOutputBit(2, yellow); // DO03: YELLOW
            SetOutputBit(3, green);  // DO04: GREEN
            SetOutputBit(4, buzzer); // DO05: BUZZER
        }

        public bool IsVacuumOk() => GetInputBit(1);                      // DI02: 진공 흡착 확인
        public bool IsEmergencyStop() => !GetInputBit(0);                // DI01: 비상정지 EMO (Normal NC)
        public bool IsDoorClosed() => true;
    }
}
