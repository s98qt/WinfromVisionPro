using System;
using System.Runtime.InteropServices;

namespace iMG
{
    // iCam wrapper class 3.1.6
    public class iCam
    {
        public enum CamType
        {
            Unknown = 0x00000,

            MU204C = 0x10000,
            MU204M = 0x10001,
            MU267C = 0x10100,
            MU267M = 0x10101,
            MU285C = 0x10110,
            MU285M = 0x10111,
            MU412C = 0x10300,
            MU282C = 0x10500,

            CF255M = 0x20000,
            CF285AM = 0x20100,
            CF285BC = 0x20104,
            CF285BM = 0x20105,
            CF285CM = 0x20108,
            CF674M = 0x20300,
            CF4022M = 0x20400,
            CF413C = 0x20600,
            CF694C = 0x20610,
            CF694M = 0x20611,
            CF8300AM = 0x20800,
            CF8300BM = 0x20804,
            CF814M = 0x20900,

            SC035C = 0x30100,
            SC120C = 0x30110,
            SC230M = 0x30200,
            SC600C = 0x30600,
            SC600M = 0x30601,
            SC1200C = 0x31200,
            SC2000C = 0x32000,

            AP120C = 0x40100,
            AP120M = 0x40101,
            AP130M = 0x40110,
            AP300C = 0x40300,
            AP500C = 0x40500,
            AP500M = 0x40501,

            MD285C = 0x50100,
            MD285M = 0x50101,
            MD674C = 0x50300,
            MD674M = 0x50301,
            MD694C = 0x50600,
            MD694M = 0x50601,
            MD814C = 0x50900,
            MD814M = 0x50901,

            SL130M = 0x60100,
            SL200M = 0x60200,
            SL500M = 0x60500,
            SL8300M = 0x60800,

            IC340M = 0x70000,

            IE500C = 0x80500,
            IE500M = 0x80501,
            IE600C = 0x80600,
            IE1400C = 0x81400,

            PD694M = 0x90600,

            MH655C = 0xA0500,
            MH694AC = 0xA0600,
            MH694AM = 0xA0601,
            MH694BC = 0xA0604,

            SD1600AC = 0xB1600,
            SD1600AM = 0xB1601,
            SD1600BC = 0xB1604,

            NI1000M = 0xC1000,
            NI1600M = 0xC1600,
        }

        public enum Depth
        {
            Bit8 = 10,
            Bit16 = 20,
        }

        public enum ReadoutSpeed
        {
            High = 10,
            Medium = 20,
            Low = 30,
        }

        public enum Mode
        {
            Full = 10,
            Bin2x2 = 20,
            Bin3x3 = 21,
            Bin4x4 = 22,
        }

        public enum Scene
        {
            General = 10,
            Microscopy = 20,
            MicroscopyEnh = 21,
            Fluorescence = 30,
            FluorescenceEnh = 31,
            Metallography = 40,
        }

        public enum AwbMode
        {
            Global = 10,
            Spot = 20,
        }

        public enum AuthLevel
        {
            Premier = 0x00,
            BiologyAdv = 0xD0,
            BiologyPro = 0xD1,
            IndustryAdv = 0xE0,
            IndustryPro = 0xE1,
            Basic = 0xF0,
            Trial = 0xFF,
        }

        [DllImport("iCam.dll", EntryPoint = "iCamInit")]
        static extern public void Init(IntPtr[] camHandleList, out int camCount);
        [DllImport("iCam.dll", EntryPoint = "iCamUninit")]
        static extern public void Uninit();

        [DllImport("iCam.dll", EntryPoint = "iCamGetCamType")]
        static extern public CamType GetCamType(IntPtr camHandle);
        [DllImport("iCam.dll", EntryPoint = "iCamGetFrameBufferSize")]
        static extern public int GetFrameBufferSize(IntPtr camHandle);

        [DllImport("iCam.dll", EntryPoint = "iCamSetScene")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetScene(IntPtr camHandle, Scene scene);

        [DllImport("iCam.dll", EntryPoint = "iCamSetMode")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetMode(IntPtr camHandle, Mode mode);
        [DllImport("iCam.dll", EntryPoint = "iCamSetResolution")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetResolution(IntPtr camHandle, int x, int y);
        [DllImport("iCam.dll", EntryPoint = "iCamSetBinning")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetBinning(IntPtr camHandle, byte h, byte v);
        [DllImport("iCam.dll", EntryPoint = "iCamSetDepth")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetDepth(IntPtr camHandle, Depth depth);
        [DllImport("iCam.dll", EntryPoint = "iCamSetReadoutSpeed")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetReadoutSpeed(IntPtr camHandle, ReadoutSpeed readoutSpeed);
        [DllImport("iCam.dll", EntryPoint = "iCamSetUsbTraffic")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetUsbTraffic(IntPtr camHandle, byte traffic);
        [DllImport("iCam.dll", EntryPoint = "iCamSetAutoExposure")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetAutoExposure(IntPtr camHandle, bool ae);
        [DllImport("iCam.dll", EntryPoint = "iCamSetAeCompensation")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetAeCompensation(IntPtr camHandle, int aeCompensation);
        [DllImport("iCam.dll", EntryPoint = "iCamSetExposure")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetExposure(IntPtr camHandle, double exp);
        [DllImport("iCam.dll", EntryPoint = "iCamGetExposure")]
        static extern public double GetExposure(IntPtr camHandle);
        [DllImport("iCam.dll", EntryPoint = "iCamSetGain")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetGain(IntPtr camHandle, byte gain);
        [DllImport("iCam.dll", EntryPoint = "iCamGetGain")]
        static extern public byte GetGain(IntPtr camHandle);
        [DllImport("iCam.dll", EntryPoint = "iCamSetLineNoiseRmv")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetLineNoiseRmv(IntPtr camHandle, bool lnr);
        [DllImport("iCam.dll", EntryPoint = "iCamSetHighGainBoost")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetHighGainBoost(IntPtr camHandle, bool hgb);

        [DllImport("iCam.dll", EntryPoint = "iCamBeginCapture")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool BeginCapture(IntPtr camHandle, bool live);
        [DllImport("iCam.dll", EntryPoint = "iCamSetLiveFrameSize")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetLiveFrameSize(IntPtr camHandle, int w, int h);
        [DllImport("iCam.dll", EntryPoint = "iCamGetFrame")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool GetFrame(IntPtr camHandle, byte[] data, out int w, out int h, out int bpp);
        [DllImport("iCam.dll", EntryPoint = "iCamStopCapture")]
        static extern public void StopCapture(IntPtr camHandle);

        [DllImport("iCam.dll", EntryPoint = "iCamSetThermalNoiseRmv")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetThermalNoiseRmv(IntPtr camHandle, bool tnr);

        [DllImport("iCam.dll", EntryPoint = "iCamAutoWhiteBalance")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool AutoWhiteBalance(IntPtr camHandle, int roiLeft, int roiTop, int roiWidth, int roiHeight);
        [DllImport("iCam.dll", EntryPoint = "iCamSetWhiteBalanceParams")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetWhiteBalanceParams(IntPtr camHandle, double paramR, double paramB);
        [DllImport("iCam.dll", EntryPoint = "iCamGetWhiteBalanceParams")]
        static extern public void GetWhiteBalanceParams(IntPtr camHandle, out double paramR, out double paramB);

        [DllImport("iCam.dll", EntryPoint = "iCamSetSaturation")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetSaturation(IntPtr camHandle, double saturation);

        [DllImport("iCam.dll", EntryPoint = "iCamSetContrast")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetContrast(IntPtr camHandle, double contrast);

        [DllImport("iCam.dll", EntryPoint = "iCamSetSharpness")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetSharpness(IntPtr camHandle, double sharpness);

        [DllImport("iCam.dll", EntryPoint = "iCamSetFlip")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetFlip(IntPtr camHandle, bool horz, bool vert);

        [DllImport("iCam.dll", EntryPoint = "iCamSetTargetTemperature")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool SetTargetTemperature(IntPtr camHandle, double temp);
        [DllImport("iCam.dll", EntryPoint = "iCamBeginCooling")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool BeginCooling(IntPtr camHandle);
        [DllImport("iCam.dll", EntryPoint = "iCamStopCooling")]
        static extern public void StopCooling(IntPtr camHandle);
        [DllImport("iCam.dll", EntryPoint = "iCamGetCoolerStatus")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool GetCoolerStatus(IntPtr camHandle, out double temp, out byte power);

        [DllImport("iCam.dll", EntryPoint = "iCamGetAuthLevel")]
        static extern public AuthLevel GetAuthLevel(IntPtr camHandle);
        [DllImport("iCam.dll", EntryPoint = "iCamGetAuthToken")]
        static extern public UInt64 GetAuthToken(IntPtr camHandle);
    }
}
