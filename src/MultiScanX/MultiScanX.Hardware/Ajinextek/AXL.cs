using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MultiScanX.Hardware.Ajinextek
{
    /// <summary>
    /// 아진엑스텍 AXL(EzSoftware UC) 표준 P/Invoke C# 래퍼 선언부 (단일 AXL.dll 통합 호출)
    /// 참조: 02-2_N3ECAT_Slave_SW_KOR_V1.1.pdf &amp; 03-1_N3ECAT-AI16_AO8_Manual(K)_201504.pdf
    /// </summary>
    public static class CAXL
    {
        private const string DllName = "AXL.dll";
        private static bool _dllPathChecked = false;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// 실행 폴더에 AXL.dll이 없을 경우 EzSoftware UC 64비트 공식 설치 경로를 자동 탐색하여 DLL 검색 경로에 등록
        /// </summary>
        public static string? EnsureAjinDllPathRegistered()
        {
            if (_dllPathChecked) return null;
            _dllPathChecked = true;

            string[] candidates =
            {
                AppContext.BaseDirectory,
                @"C:\MultiScanX\bin",
                @"C:\Program Files (x86)\EzSoftware UC\AXL(Library)\Library\64Bit",
                @"C:\Program Files\EzSoftware UC\AXL(Library)\Library\64Bit",
                @"C:\Program Files (x86)\EzSoftware UC\Examples\Visual C#\Executable\64Bit"
            };

            foreach (string dir in candidates)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dir) && File.Exists(Path.Combine(dir, DllName)))
                    {
                        SetDllDirectory(dir);
                        return dir;
                    }
                }
                catch { }
            }
            return null;
        }

        [DllImport(DllName)]
        public static extern int AxlOpen(int lIrqNo);

        [DllImport(DllName)]
        public static extern int AxlClose();

        [DllImport(DllName)]
        public static extern int AxlIsOpened();
    }

    /// <summary>
    /// 모션 제어 모듈 API (Panasonic MINAS-A6B X/Y축, FASTECH Ezi-SERVO II Plus-E Z축)
    /// </summary>
    public static class CAXM
    {
        private const string DllName = "AXL.dll";

        [DllImport(DllName)]
        public static extern int AxmInfoGetAxisCount(ref int lpAxisCount);

        [DllImport(DllName)]
        public static extern int AxmSignalServoOn(int lAxisNo, uint uOnOff);

        [DllImport(DllName)]
        public static extern int AxmSignalIsServoOn(int lAxisNo, ref uint upOnOff);

        [DllImport(DllName)]
        public static extern int AxmStatusReadInMotion(int lAxisNo, ref uint upInMotion);

        [DllImport(DllName)]
        public static extern int AxmStatusReadActPos(int lAxisNo, ref double dpPosition);

        [DllImport(DllName)]
        public static extern int AxmStatusReadCmdPos(int lAxisNo, ref double dpPosition);

        [DllImport(DllName)]
        public static extern int AxmMovePos(int lAxisNo, double dPosition, double dVelocity, double dAccel, double dDecel);

        [DllImport(DllName)]
        public static extern int AxmMoveSStop(int lAxisNo);

        [DllImport(DllName)]
        public static extern int AxmMoveEStop(int lAxisNo);

        [DllImport(DllName)]
        public static extern int AxmHomeSetStart(int lAxisNo);

        [DllImport(DllName)]
        public static extern int AxmHomeGetResult(int lAxisNo, ref uint upMainStepNumber, ref uint upStepNumber, ref uint upResult);
    }

    /// <summary>
    /// 디지털 입출력 모듈 API (N3ECAT-DB32T: Product Code 0x00000332, 16 DI [0x6000] / 16 DO [0x7010])
    /// </summary>
    public static class CAXD
    {
        private const string DllName = "AXL.dll";

        [DllImport(DllName)]
        public static extern int AxdInfoIsDIOModule(ref uint upStatus);

        [DllImport(DllName)]
        public static extern int AxdInfoGetModuleCount(ref int lpModuleCount);

        [DllImport(DllName)]
        public static extern int AxdoWriteOutportBit(int lModuleNo, int lOffset, uint uValue);

        [DllImport(DllName)]
        public static extern int AxdoReadOutportBit(int lModuleNo, int lOffset, ref uint upValue);

        [DllImport(DllName)]
        public static extern int AxdiReadInportBit(int lModuleNo, int lOffset, ref uint upValue);

        [DllImport(DllName)]
        public static extern int AxdiReadInportWord(int lModuleNo, int lOffset, ref uint upValue);

        [DllImport(DllName)]
        public static extern int AxdoReadOutportWord(int lModuleNo, int lOffset, ref uint upValue);

        [DllImport(DllName)]
        public static extern int AxdoWriteOutportWord(int lModuleNo, int lOffset, uint uValue);
    }

    /// <summary>
    /// 아날로그 입력 모듈 API (N3ECAT-AI16: 16CH 16-bit, CH1~4 전압/전류 스위치 선택, CH5~16 전압 전용)
    /// </summary>
    public static class CAXA
    {
        private const string DllName = "AXL.dll";

        [DllImport(DllName)]
        public static extern int AxaInfoIsAIOModule(ref uint upStatus);

        [DllImport(DllName)]
        public static extern int AxaInfoGetModuleCount(ref int lpModuleCount);

        [DllImport(DllName)]
        public static extern int AxaInfoGetInputCount(int lModuleNo, ref int lpChannelCount);

        [DllImport(DllName)]
        public static extern int AxaSetRange(int lChannelNo, double dMinVolt, double dMaxVolt);

        [DllImport(DllName)]
        public static extern int AxaGetRange(int lChannelNo, ref double dpMinVolt, ref double dpMaxVolt);

        [DllImport(DllName)]
        public static extern int AxaReadChannelVolt(int lChannelNo, ref double dpVolt);

        [DllImport(DllName)]
        public static extern int AxaReadChannelDigit(int lChannelNo, ref uint upDigit);
    }

    /// <summary>
    /// 고속 카운터 모듈 API (N3ECAT-CNT2: Product Code 0x00000902, 2CH 28-bit Counter &amp; Position Trigger)
    /// </summary>
    public static class CAXC
    {
        private const string DllName = "AXL.dll";

        [DllImport(DllName)]
        public static extern int AxcInfoIsCNTModule(ref uint upStatus);

        [DllImport(DllName)]
        public static extern int AxcInfoGetModuleCount(ref int lpModuleCount);

        [DllImport(DllName)]
        public static extern int AxcStatusReadCount(int lChannelNo, ref double dpCount);

        [DllImport(DllName)]
        public static extern int AxcStatusSetCount(int lChannelNo, double dCount);

        [DllImport(DllName)]
        public static extern int AxcSignalSetEncInputMethod(int lChannelNo, uint uMethod);

        [DllImport(DllName)]
        public static extern int AxcSignalGetEncInputMethod(int lChannelNo, ref uint upMethod);

        [DllImport(DllName)]
        public static extern int AxcSignalSetEncReverse(int lChannelNo, uint uReverse);

        [DllImport(DllName)]
        public static extern int AxcSignalGetEncReverse(int lChannelNo, ref uint upReverse);

        [DllImport(DllName)]
        public static extern int AxcCounterSetMultiplier(int lChannelNo, uint uMultiplier);

        [DllImport(DllName)]
        public static extern int AxcTriggerSetCompPosition(int lChannelNo, double dPosition);

        [DllImport(DllName)]
        public static extern int AxcTriggerSetTime(int lChannelNo, double dTrigTime);

        [DllImport(DllName)]
        public static extern int AxcTriggerSetLevel(int lChannelNo, uint uLevel);

        [DllImport(DllName)]
        public static extern int AxcTriggerEnable(int lChannelNo, uint uOnOff);
    }
}
