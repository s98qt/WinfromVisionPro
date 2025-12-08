//StructsBody.cs该类中的定义，是映射DataType.h中定义的相应的结构，回调函数及枚举类型
using System;
using System.Runtime.InteropServices;
using System.Text;

#region-摄像头相关参数结构
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct CapInfoStruct
{
    public IntPtr Buffer;       //!!!!!!!!!!!!!!!!!!!!!!!

    public uint Height;			// 采集高度
    public uint Width;			// 采集宽度
    public uint OffsetX;		// CCD用于调节AD增益
    public uint OffsetY;		// 
    public uint Exposure;		// 曝光值 1-5000MS

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Gain;//  R G B 增益 1-63  初始化为3个长度的数组

    public byte Control;		// 控制位
    public byte InternalUse;	// 用户不要对此字节进行操作

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] ColorOff;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Reserved;

};
#endregion

#region
public struct DLVIDEORECT
{
    public int Left;		// 相对于父窗口的水平偏移
    public int Top;		    // 相对于父窗口的垂直偏移
    public int Width;		// 视频窗口宽度
    public int Height;		// 视频窗口高度
};
#endregion

#region//CMOS相机类型

#endregion
/// <summary>
/// /////////////////////////////////////////////////////////////////////////////
/// </summary>
#region
public enum CAMERATYPE
{
    UNKNOWN = 0,
    CAMERA_PYH_4K = 0x0001,
    CAMERA_PYH_2K = 0x0002,
    CAMERA_4K_V = 0x0003,
    CAMERA_4K_VI = 0x0004,
    CAMERA_HBOX2 = 0x0005,
    CAMERA_HBOX4 = 0x0006,
    CAMERA_MICRUH500CG = 0x0007,
	CAMERA_MICRUH1200CG = 0x0008,
	CAMERA_MICRUH1600CG = 0x0009,
	CAMERA_MICRUH600C = 0x000A,
	CAMERA_MICRUH1200C = 0x000B,
	CAMERA_MICRUH2000C = 0x000C,
    CAMERA_PYH_2K_II = 0x000D,
    CAMERA_4K_V_WF = 0x000E

};
#endregion


#region
public enum DLPARAM
{
    Brightness = 0,
    Contrast = 1,
    Hue = 2,
    Saturation = 3,
    Sharpness = 4,
    ColorEnable = 5,
    WhiteBalance = 6,
    BacklightCompensation = 7,
    Gain = 8,
    Roll = 9,
    Zoom = 10,
    Iris = 11,
    Focus = 12,
    Expouse = 15,
    Gamma = 16,
    ColorMode = 17,
    FlipMode = 18,
    AEing = 19,
    AWBing = 20

};
#endregion
#region
public enum ENUM_Param//
{
		idResolution,	//
		idColorMode,
		idExposure,		//
		idAEBest,
		idGain,
		idGain_R,				
		idGain_G,				
		idGain_B,	
		idSatuation,
		idContrast,		//
		idSharpness,
		idGamma,
		idFlipMode,
		idAEing,					
		idAWBing,			

};

#endregion
#region
enum	COLOR_MODE{
			YUV2	= 0,
			MJPEG	= 1,
            NV12    = 2
};
#endregion
#region
public struct TagRange
{
    public int nMax;
    public int nMin;
};

#endregion

#region
public struct TagResolution_Dlg
{
	public String lpszDesc;
    public int width;
    public int height;
};
#endregion

/// <summary>
/// ////////////////////////////////////////////////////////////
/// </summary>

#region 翻转模式 
public enum FLIP_MODE
{
    FLIP_NATURAL,		// 正常显示
    FLIP_LEFTRIGHT,	    // 左右翻转
    FLIP_UPDOWN,		// 上下翻转
    FLIP_ROTATE180		// 旋转180
};
#endregion

#region // 保存图像格式设置
public enum IMAGETYPR
{
    BMP,		//
    JPEG,	    // 
};
#endregion

#region
public enum COLOR_TYPE
{
    COLOR,
    GRAY
};
#endregion

#region	返回值定义
public struct CapReturnValul
{
    public const uint ResSuccess = (uint)0x0000;	                // 返回成功
    public const uint ResNullHandleErr = (uint)0x0001;		        // 无效句柄
    public const uint ResNullPointerErr = (uint)0x0002;		        // 指针为空
    public const uint ResFileOpenErr = (uint)0x0003;		        // 文件创建/打开失败
    public const uint ResNoDeviceErr = (uint)0x0004;		        // 没有可用设备
    public const uint ResInvalidParameterErr = (uint)0x0005;		// 内存分配不足
    public const uint ResOutOfMemoryErr = (uint)0x0006;		        // 没有开启预览
    public const uint ResNoPreviewRunningErr = (uint)0x0007;		// 预览没有开启
    public const uint ResOSVersionErr = (uint)0x0008;
    public const uint ResUsbNotAvailableErr = (uint)0x0009;
    public const uint ResNotSupportedErr = (uint)0x000a;
    public const uint ResNoSerialString = (uint)0x000b;
    public const uint ResVerificationErr = (uint)0x000c;
    public const uint ResTimeoutErr = (uint)0x000d;
    public const uint ResScaleModeErr = (uint)0x000f;
    public const uint ResUnknownErr = (uint)0x00ff;

    public const uint ResDisplayWndExist = (uint)0x0011;		// 应该关闭预览窗口
    public const uint ResAllocated = (uint)0x0012;		        // 内存已经分配
    public const uint ResAllocateFail = (uint)0x0013;		    // 内存分配失败
    public const uint ResReadError = (uint)0x0014;              // USB读取失败
    public const uint ResWriteError = (uint)0x0015;		        // USB命令发出失败
    public const uint ResUsbOpen = (uint)0x0016;                // USB端口已经打开
    public const uint ResCreateStreamErr = (uint)0x0017;		// 创建avi流失败
    public const uint ResSetStreamFormatErr = (uint)0x0018;		// 设置AVI流格式失败


};
#endregion

#region	Window Styles value
public struct StylesValul
{
    public const uint WS_OVERLAPPED = (uint)0x00000000L;
    public const uint WS_POPUP = (uint)0x80000000L;
    public const uint WS_CHILD = (uint)0x40000000L;
    public const uint WS_MINIMIZE = (uint)0x20000000L;
    public const uint WS_VISIBLE = (uint)0x10000000L;
    public const uint WS_DISABLED = (uint)0x08000000L;
    public const uint WS_CLIPSIBLINGS = (uint)0x04000000L;
    public const uint WS_CLIPCHILDREN = (uint)0x02000000L;
    public const uint WS_MAXIMIZE = (uint)0x01000000L;
    public const uint WS_CAPTION = (uint)0x00C00000L;    /* WS_BORDER | WS_DLGFRAME  */
    public const uint WS_BORDER = (uint)0x00800000L;
    public const uint WS_DLGFRAME = (uint)0x00400000L;
    public const uint WS_VSCROLL = (uint)0x00200000L;
    public const uint WS_HSCROLL = (uint)0x00100000L;
    public const uint WS_SYSMENU = (uint)0x00080000L;
    public const uint WS_THICKFRAME = (uint)0x00040000L;
    public const uint WS_GROUP = (uint)0x00020000L;
    public const uint WS_TABSTOP = (uint)0x00010000L;

    public const uint WS_MINIMIZEBOX = (uint)0x00020000L;
    public const uint WS_MAXIMIZEBOX = (uint)0x00010000L;
};
#endregion

#region	分辨率的结构体
public class TagResolution
{
    public string m_szDesc;
    public int m_width;
    public int m_height;
    public TagResolution()
    {
        m_szDesc = "";
        m_width = 0;
        m_height = 0;
    }
    public TagResolution(string szDesc, int width, int height)
    {
        m_szDesc = szDesc;
        m_width = width;
        m_height = height;
    }
};
#endregion
