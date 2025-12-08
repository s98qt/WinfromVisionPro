//DllFunction.cs该文件中方法映射USBAPI.h中方法
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace USBSDK_CMOS_Demo
{
    #region----------------声明两个委托,相当于VC里面的回调函数------------------
    public delegate void DL_AUTOCALLBACK(uint dw1, uint dw2, IntPtr lpContext);
    /// <summary>
    /// 帧回调函数	lpParam指向帧数据的指针, 
    /// 			lpPoint 保留, 
    /// 			lpContext在设置帧回调函数时传递的上下文
    /// </summary>
    public delegate void DL_FRAMECALLBACK(IntPtr lpParam1, IntPtr lpPoint, IntPtr lpContext);

    #endregion


    
    #region 对应dll中的函数原型转换
    /// <summary>
    /// USBSDK_CCD函数原型
    /// </summary>
    public class DllFunction
    {
        /// <summary>
        ///函数:UVC_Initialize	
        ///功能:初始化设备，返回摄像头句柄，用于其它函数的调用

        ///参数:	pIndex			摄像头索引
		///pFilterName		保留
		///hCamera			摄像头句柄
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_Initialize(string pFilterName, ref int pIndex,  ref IntPtr hCamera);

        /// <summary>
        ///函数:	UVC_Uninitialize
        ///功能:	反初始化设备
        ///参数:   
        ///说明:   必须在程序退出时调用,用于释放内存分配空间
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_Uninitialize(IntPtr hCamera);

        /// <summary>
        ///  函数:	UVC_StartView
        ///功能:	打开预览窗口,并启动视频流
        ///参数:   
        ///说明: 
        /// </summary>               
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_StartView(IntPtr hCamera,
              string lpszWindowName,
              uint dwStyle,
              int x,
              int y,
              int nWidth,
              int nHeight,
              IntPtr hwndParent,
              IntPtr nIDorHMenu
              );


        /// <summary>
        /// 函数:	UVC_StopView
        ///功能:	停止视频流
        ///参数:   
        ///说明: 
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_StopView(IntPtr hCamera);

        /// <summary>
        /// 函数:	UVC_SetParam
        ///功能:	设置图像参数
        ///参数:  
        ///说明:   
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_SetParam(IntPtr hCamera, DLPARAM DLParam,  int Value);
       
        /// <summary>
        ///函数:	UVC_GetParam
        ///功能:	设置图像参数
        ///参数:   
        ///说明:
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetParam(IntPtr hCamera, DLPARAM DLParam, ref int Value);
       
        /// <summary>
        ///函数:	UVC_SetResolution
        ///功能:	设置图像分辨率
        ///参数:  
        ///说明: 
        /// </summary> 
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_SetResolution(IntPtr hCamera, uint nWidth, uint nHeight);
       
        /// <summary>
        ///函数:	UVC_GetResolution
        ///功能:	获得图像分辨率
        ///参数:   
        ///说明:
        ///</summary> 
         [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetResolution(IntPtr hCamera, ref int nIndex, ref uint nWidth, ref uint nHeight);        

         /// <summary>
        ///函数:	UVC_GetRgbFrame
        ///功能:	采集一帧24bitRGB数据到pDest中。
        ///参数:   
        ///说明: 
        ///</summary> 
         [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetRgbFrame(IntPtr hCamera, IntPtr pDest);
        
         /// <summary>
        ///函数:	UVC_GetRgbFrameToBmp
        ///功能:	采集一帧图像到BMP文件。
        ///参数:   
        ///说明: 
        ///</summary> 
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]      
        public static extern int UVC_GetRgbFrameToBmp(IntPtr hCamera,  /*byte *b*/IntPtr pDest, string strFileName, bool SaveData);//C#中貌似不支持参数默认值
                       
        /// <summary>
        /// 函数:	UVC_GetRgbFrameToJpeg
        ///功能:	采集一帧图像到Jpeg文件。
        ///参数:   nQuality 为压缩质量 1 - 100
        ///说明:   
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetRgbFrameToJpeg(IntPtr hCamera,   /*ref byte[] pDest*/IntPtr pDest, string strFileName, int nQuality);
        
        /// <summary>
        /// 函数:	UVC_SetFrameCallback
        ///功能:	用于鼠标在预览窗口上点击的回调，lpParam为用户上下文
        ///参数:           
        ///lpContext- 上下文
        ///说明:   
        /// </summary>   
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_SetFrameCallback(IntPtr hCamera, DL_FRAMECALLBACK FrameCB, IntPtr lpContext);
       
        /// <summary>
        ///函数:	UVC_SetViewWin
        ///功能:	设置预览窗口的位置
        ///参数:   
        ///说明:   
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_SetViewWin(IntPtr hCamera, ref DLVIDEORECT Rect);
        
        /// <summary>
        /// 函数:	UVC_GetTotalDeviceNum
        ///功能:	得到共有几个摄像头
        ///参数:   
        ///说明:   
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetTotalDeviceNum(IntPtr hCamera, ref int pDeviceNum);
        
        /// <summary>
        ///函数:	UVC_GetCameraType
        ///功能:	获得当前相机类型.
        ///参数:   nCameraType
        ///说明:  
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetCameraType(IntPtr hCamera, ref int nCameraType);

        /// <summary>
        ///函数:	UVC_GetFrameRate
        ///功能:	得到摄像头的当前帧率
        ///参数:   
        ///说明: 
         /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_GetFrameRate(IntPtr hCamera, ref float pfFrameRate);

       
        /// <summary>
        ///函数:   UVC_ShowPropertyPage
	    ///功能:	显示用户自定义属性对话框
	    ///参数:
	    ///说明:   必须在有视频流时才能有效
        /// </summary>
        [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_ShowPropertyPage(IntPtr hCamera);
        
       
        /// <summary>
        ///函数:	UVC_SetDPIValue
	    ///功能:	设置JPG图片的DPI值
	    ///参数:   nDPIx 为宽的DPI值
		///	            nDPIy 为高的DPI值
	    ///说明:   
        /// </summary>  
         [DllImport("UVCCamAPIu.dll", CharSet = CharSet.Auto)]
        public static extern int UVC_SetDPIValue(IntPtr hCamera, int nDPIx, int nDPIy);
      
    }
    #endregion
}