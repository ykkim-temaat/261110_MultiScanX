using System;
using System.Runtime.InteropServices;

namespace MultiScanX.Hardware.Ajinextek
{
    /// <summary>
    /// 아진엑스텍 AXL(EzSoftwareUC) 표준 P/Invoke C# 래퍼 선언부
    /// </summary>
    public static class CAXL
    {
        [DllImport("AXL.dll")]
        public static extern int AxlOpen(int lIrqNo);

        [DllImport("AXL.dll")]
        public static extern int AxlClose();

        [DllImport("AXL.dll")]
        public static extern int AxlIsOpened();
    }

    public static class CAXM
    {
        [DllImport("AXM.dll")]
        public static extern int AxmInfoGetAxisCount(ref int lpAxisCount);

        [DllImport("AXM.dll")]
        public static extern int AxmSignalServoOn(int lAxisNo, uint uOnOff);

        [DllImport("AXM.dll")]
        public static extern int AxmSignalIsServoOn(int lAxisNo, ref uint upOnOff);

        [DllImport("AXM.dll")]
        public static extern int AxmStatusReadInMotion(int lAxisNo, ref uint upInMotion);

        [DllImport("AXM.dll")]
        public static extern int AxmStatusReadActPos(int lAxisNo, ref double dpPosition);

        [DllImport("AXM.dll")]
        public static extern int AxmStatusReadCmdPos(int lAxisNo, ref double dpPosition);

        [DllImport("AXM.dll")]
        public static extern int AxmMovePos(int lAxisNo, double dPosition, double dVelocity, double dAccel, double dDecel);

        [DllImport("AXM.dll")]
        public static extern int AxmMoveSStop(int lAxisNo);

        [DllImport("AXM.dll")]
        public static extern int AxmMoveEStop(int lAxisNo);

        [DllImport("AXM.dll")]
        public static extern int AxmHomeSetStart(int lAxisNo);

        [DllImport("AXM.dll")]
        public static extern int AxmHomeGetResult(int lAxisNo, ref uint upMainStepNumber, ref uint upStepNumber, ref uint upResult);
    }

    public static class CAXD
    {
        [DllImport("AXD.dll")]
        public static extern int AxdoWriteOutportBit(int lModuleNo, int lOffset, uint uValue);

        [DllImport("AXD.dll")]
        public static extern int AxdoReadOutportBit(int lModuleNo, int lOffset, ref uint upValue);

        [DllImport("AXD.dll")]
        public static extern int AxdiReadInportBit(int lModuleNo, int lOffset, ref uint upValue);
    }

    public static class CAXC
    {
        [DllImport("AXC.dll")]
        public static extern int AxcStatusReadCount(int lChannelNo, ref double dpCount);

        [DllImport("AXC.dll")]
        public static extern int AxcCounterSetMultiplier(int lChannelNo, uint uMultiplier);

        [DllImport("AXC.dll")]
        public static extern int AxcTriggerSetCompPosition(int lChannelNo, double dPosition);

        [DllImport("AXC.dll")]
        public static extern int AxcTriggerEnable(int lChannelNo, uint uOnOff);
    }
}
