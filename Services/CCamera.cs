using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace USBSDK_CMOS_Demo
{
    class CCamera : Control
    {
        //8bit command type
            public static readonly int  _4BITCMD_RGAIN   =  0x0000;
            public static readonly int   _4BITCMD_BGAIN  =   0x1000;
            public static readonly int   _4BITCMD_GGAIN  =   0x2000;

            public static readonly int   _4BITCMD_GETDATA  = 0x0FFF;  //��ȡ��ǰֵʱ����set���ֵ


//4bit command type
            public static readonly int   _8BITCMD_GLOBALGAIN  =  0x0000;
            public static readonly int   _8BITCMD_AEBEST    =    0x0100;
            public static readonly int   _8BITCMD_SATURATION  =  0x0200;
            public static readonly int   _8BITCMD_SHARPEN   =    0x0300;
            public static readonly int   _8BITCMD_HDR      =     0x0400;
            public static readonly int   _8BITCMD_CONTRAST   =   0x0500;
            public static readonly int   _8BITCMD_BWMODE   =     0x0600;
            public static readonly int   _8BITCMD_MIRROR     =   0x0700;
            public static readonly int   _8BITCMD_AWB       =    0x0800;
            public static readonly int   _8BITCMD_AE    =        0x0900;
            public static readonly int   _8BITCMD_REAL    =      0xFE00;
//#define  _8BITCMD_AWB           0x0a00
//#define  _8BITCMD_AWB           0x0b00

            public static readonly int _8BITCMD_GETDATA = 0x00FF;  
        

        public IMAGETYPR m_ImageType;

     
        public TagResolution[] m_Resolution;    
        public FLIP_MODE m_FlipMode;
        public COLOR_MODE m_ColorMode;
        public int m_nCameraCount;
        public string m_strFriendlyName;

        public int m_Xoff, m_Yoff;      

        protected IntPtr m_hDevice;
        public CAMERATYPE m_CamType;
        protected bool m_bPlay;
        
        // 公共属性用于外部访问
        public IntPtr DeviceHandle => m_hDevice;
        public uint CollectWidth => m_nCollectWidth;
        public uint CollectHeight => m_nCollectHeight;		  
        protected bool m_bCrossline;	   
        protected bool m_bstretch;	       
        protected bool m_bAE;
        protected bool m_bAWB;
        protected bool m_bCap;
        protected IntPtr m_hWnd;		      

        protected IntPtr m_pRawData;
        protected IntPtr m_pRgbData;
        protected IntPtr m_pSnapBuffer;

        protected string m_bCapFilePath;

        protected CapInfoStruct m_CapInfo;

        protected int m_vSizeX, m_vSizeY;	


        protected DL_FRAMECALLBACK m_pFrameCallBack;
        protected DL_AUTOCALLBACK m_pAWBCallback;
        protected DL_AUTOCALLBACK m_pAECallback;
        public TagRange[] m_nRange = new TagRange[20];
        public String m_strName;
        public int m_nResCount;		
        public uint m_nCollectWidth;
        public uint m_nCollectHeight;



        public int m_nContrast;
        public int m_nLight;
        public int m_nGain;
        public int m_nSaturation;
        public int m_nSharpness;
        public int m_nGamma;

        public int  m_Exposure;
        public int m_nR, m_nG, m_nB;
        public COLOR_MODE m_nColorMode;
        public int m_nFlipMode;

        //BOOL m_bAWB;
        //BOOL m_bAE;
        public bool m_bAWBing;
        public bool m_bAEing;

        //public TagResolution_Dlg[] m_lpszResolution = new TagResolution_Dlg[8];	//��֧�ֵķֱ���
        public TagResolution_Dlg[] m_lpszResolution;	//��֧�ֵķֱ���

        public CCamera(IntPtr hWnd, int deviceIndex = 1)
        {
            CAM_Initialize(hWnd, deviceIndex);
        }

        public CCamera()
        {
            CAM_Initialize(IntPtr.Zero);
        }

        ~CCamera()
        {
            if (m_pRawData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_pRawData);
                m_pRawData = IntPtr.Zero;
            }

            if (m_pRgbData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_pRgbData);
                m_pRgbData = IntPtr.Zero;
            }

            if (m_pSnapBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_pSnapBuffer);
                m_pSnapBuffer = IntPtr.Zero;
            }

            if (m_hDevice != IntPtr.Zero)
                DllFunction.UVC_Uninitialize(m_hDevice);

            m_CapInfo.Buffer = new IntPtr();
        }

        public CapInfoStruct GetCapInfo()
        {
            return m_CapInfo;
        }
        public CAMERATYPE GetCamType()
        {
            return m_CamType;
        }
        /// <summary>
        /// ֡�ص�����,���ڻ���ʮ����
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="lpReserve"></param>
        /// <param name="lpContext">���ݵ�ǰ����ľ��</param>
        private static void FrameCallback(IntPtr pData, IntPtr lpReserve, IntPtr lpContext)
        {
            CCamera camera = (CCamera)Control.FromHandle(lpContext);
            if (camera != null)
            {
                camera.DrawCrossLine(pData);
            }
        }

        /// <summary>
        /// �Զ���ƽ��ص�����
        /// </summary>
        /// <param name="dw1"></param>
        /// <param name="dw2"></param>
        /// <param name="lpContext"></param>
        public static void AWBCallback(uint dw1, uint dw2, IntPtr lpContext)
        {
            CCamera camera = (CCamera)Control.FromHandle(lpContext);
            camera.FinishAWB(dw1, dw2);
        }

        /// <summary>
        /// �Զ��ع�ص�����
        /// </summary>
        /// <param name="dw1"></param>
        /// <param name="dw2"></param>
        /// <param name="lpContext"></param>
        public static void AECallback(uint dw1, uint dw2, IntPtr lpContext)
        {
            CCamera camera = (CCamera)Control.FromHandle(lpContext);

            uint dwResult = dw1;
            camera.FinishAE(dwResult);
        }


        public void FinishAWB(uint dw1, uint dw2)
        {
            /*
            if (m_CamType == CAMERA.MicroUH1200C)
            {
                m_CapInfo.Width = 4000;
                m_CapInfo.Height = 3000;
            }
            else
            {
                m_nR = (int)((dw1 >> 16) & 0x0000FFFF);
                m_nG = (int)(dw1 & 0x0000FFFF);
                m_nB = (int)((dw2 >> 16) & 0x0000FFFF);
                m_bAWB = false;
            }
             */
            //ˢ�½�����ʾ
            //MainForm.s_mainForm.RefBox();
        }


        public void FinishAE(uint dwResult)
        {
            uint Exposure = dwResult;
            m_CapInfo.Exposure = Exposure;
            m_bAE = false;

            //MainForm.s_mainForm.RefBox();
        }

        private void CAM_Initialize(IntPtr hWnd, int deviceIndex = 1)
        {
            m_bCrossline = false;
            m_bstretch = false;
            m_bAE = false;
            m_bAWB = false;
            m_bCap = false;
            m_bPlay = false;

            m_bCapFilePath = "";
            m_ImageType = IMAGETYPR.BMP;
            m_hWnd = hWnd;

            // m_pRawData = new IntPtr();
            // m_pRawData = Marshal.AllocHGlobal(2048 * 1536 + 2048);
            //
            // m_pRgbData = new IntPtr();
            // m_pRgbData = Marshal.AllocHGlobal(2048 * 1536 * 3 + 512);
            //
            // m_pSnapBuffer = new IntPtr();

            m_CapInfo = new CapInfoStruct();
            m_CapInfo.Gain = new byte[3];
            m_CapInfo.ColorOff = new byte[4];
            m_CapInfo.Reserved = new byte[3];

            m_CapInfo.Buffer = m_pRawData;
            m_CapInfo.Width = 3840;
            m_CapInfo.Height = 2160;
            m_CapInfo.Exposure = 100;
            m_CapInfo.Gain[0] = 32;
            m_CapInfo.Gain[1] = 0;
            m_CapInfo.Gain[2] = 0;
            m_nContrast = 16;
            m_ColorMode = COLOR_MODE.YUV2;
            m_FlipMode = FLIP_MODE.FLIP_NATURAL;
            m_nR = m_nG = m_nB = 500;
            m_hDevice = new IntPtr();
            int nIndex = deviceIndex;
            if (CapReturnValul.ResSuccess != DllFunction.UVC_Initialize("UVC_DEMO", ref nIndex, ref m_hDevice))
            {
                DllFunction.UVC_Uninitialize(m_hDevice);
                m_hDevice = IntPtr.Zero;
                // MessageBox.Show("Initial Error"); // Disable popup for auto-detection
                return;
            }

            int camtype = 0;
            int n;
            n = DllFunction.UVC_GetCameraType(m_hDevice, ref camtype);
            m_CamType = (CAMERATYPE)camtype;
            //n = DllFunction.UVC_SetParam(IntPtr hCamera, DLPARAM DLParam, Int64 Value);
            m_nResCount = 0;
            uint nwidth = 0;
            uint nheight = 0;
            n = DllFunction.UVC_GetResolution(m_hDevice,  ref m_nResCount, ref nwidth, ref nheight);

            m_lpszResolution = new TagResolution_Dlg[m_nResCount];
	        int x =0;
	        for (x = 0;x<m_nResCount;x++)
	        {
		        int mm = x;
                DllFunction.UVC_GetResolution(m_hDevice, ref mm, ref nwidth, ref nheight);
		        m_lpszResolution[x].width = (int)nwidth;
                m_lpszResolution[x].height = (int)nheight;
	            String strname;
		        strname=String.Format("{0} x {1}",nwidth,nheight);

		        m_lpszResolution[x].lpszDesc = strname;
	        }
	
	        m_nRange[(int)ENUM_Param.idExposure].nMin = 7;
	        m_nRange[(int)ENUM_Param.idExposure].nMax = 16630;//
	        m_nRange[(int)ENUM_Param.idAEBest].nMin = 20;
	        m_nRange[(int)ENUM_Param.idAEBest].nMax = 200;
	        m_nRange[(int)ENUM_Param.idGain].nMin = 0;
	        m_nRange[(int)ENUM_Param.idGain].nMax = 80;
	        m_nRange[(int)ENUM_Param.idGain_R].nMin = 0;		
	        m_nRange[(int)ENUM_Param.idGain_R].nMax = 2047;	
	        m_nRange[(int)ENUM_Param.idGain_G].nMin = 0;	
	        m_nRange[(int)ENUM_Param.idGain_G].nMax = 2047;	
	        m_nRange[(int)ENUM_Param.idGain_B].nMin = 0;	
	        m_nRange[(int)ENUM_Param.idGain_B].nMax = 2047;	
	        m_nRange[(int)ENUM_Param.idSatuation].nMin = 0;
	        m_nRange[(int)ENUM_Param.idSatuation].nMax = 254;
	        m_nRange[(int)ENUM_Param.idContrast].nMin = 0;
	        m_nRange[(int)ENUM_Param.idContrast].nMax = 254;	
	        m_nRange[(int)ENUM_Param.idSharpness].nMin = 0;
	        m_nRange[(int)ENUM_Param.idSharpness].nMax = 254;
	        m_nRange[(int)ENUM_Param.idGamma].nMin = 0;
            m_nRange[(int)ENUM_Param.idGamma].nMax = 254;
 
            switch (m_CamType)
            {
                case CAMERATYPE.CAMERA_PYH_4K:
                    {

                        m_strName = ("CAMERA_4K");

                        m_nRange[(int)ENUM_Param.idExposure].nMin = 133;
                        m_nRange[(int)ENUM_Param.idExposure].nMax = 33329;//
                        m_nRange[(int)ENUM_Param.idAEBest].nMin = 40;
                        m_nRange[(int)ENUM_Param.idAEBest].nMax = 600;
                        m_nRange[(int)ENUM_Param.idGain].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain].nMax = 80;
                        m_nRange[(int)ENUM_Param.idGain_R].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain_R].nMax = 1023;
                        m_nRange[(int)ENUM_Param.idGain_G].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain_G].nMax = 1023;
                        m_nRange[(int)ENUM_Param.idGain_B].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain_B].nMax = 1023;
                        m_nRange[(int)ENUM_Param.idSatuation].nMin = 0;
                        m_nRange[(int)ENUM_Param.idSatuation].nMax = 100;
                        m_nRange[(int)ENUM_Param.idContrast].nMin = 0;		//
                        m_nRange[(int)ENUM_Param.idContrast].nMax = 100;
                        m_nRange[(int)ENUM_Param.idSharpness].nMin = 0;
                        m_nRange[(int)ENUM_Param.idSharpness].nMax = 127;
                        m_nRange[(int)ENUM_Param.idGamma].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGamma].nMax = 255;
                        GetParamPYH();
                    }
                    break;
                case CAMERATYPE.CAMERA_PYH_2K:
                    {

                        m_strName = ("CAMERA_2K");
                        m_nRange[(int)ENUM_Param.idExposure].nMin = 7;
                        m_nRange[(int)ENUM_Param.idExposure].nMax = 16630;//
                        m_nRange[(int)ENUM_Param.idAEBest].nMin = 80;
                        m_nRange[(int)ENUM_Param.idAEBest].nMax = 1200;
                        m_nRange[(int)ENUM_Param.idGain].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain].nMax = 80;
                        m_nRange[(int)ENUM_Param.idGain_R].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain_R].nMax = 1023;
                        m_nRange[(int)ENUM_Param.idGain_G].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain_G].nMax = 1023;
                        m_nRange[(int)ENUM_Param.idGain_B].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGain_B].nMax = 1023;
                        m_nRange[(int)ENUM_Param.idSatuation].nMin = 0;
                        m_nRange[(int)ENUM_Param.idSatuation].nMax = 100;
                        m_nRange[(int)ENUM_Param.idContrast].nMin = 0;		//
                        m_nRange[(int)ENUM_Param.idContrast].nMax = 100;
                        m_nRange[(int)ENUM_Param.idSharpness].nMin = 0;
                        m_nRange[(int)ENUM_Param.idSharpness].nMax = 127;
                        m_nRange[(int)ENUM_Param.idGamma].nMin = 0;
                        m_nRange[(int)ENUM_Param.idGamma].nMax = 255;
                        GetParamPYH();
                    }
                    break;
                case CAMERATYPE.CAMERA_4K_V:
                    {

                        m_strName = ("CAMERA_4K");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_4K_VI:
                    {

                        m_strName = ("CAMERA_4K");
                        GetParam4K6();
                    }
                    break;
                case CAMERATYPE.CAMERA_HBOX2:
                    {

                        m_strName = ("HBOX2");
                    }
                    break;
                case CAMERATYPE.CAMERA_HBOX4:
                    {

                        m_strName = ("HBOX4");
                    }
                    break;
                case CAMERATYPE.CAMERA_MICRUH500CG:
                    {

                        m_strName = ("MicroUH500CG");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_MICRUH1200CG:
                    {

                        m_strName = ("MicroUH1200CG");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_MICRUH1600CG:
                    {

                        m_strName = ("MicroUH1600CG");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_MICRUH600C:
                    {

                        m_strName = ("MicroUH600C");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_MICRUH1200C:
                    {

                        m_strName = ("MicroUH1200C");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_MICRUH2000C:
                    {

                        m_strName = ("MicroUH2000C");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_PYH_2K_II:
                    {

                        m_strName = ("CAMERA_2K");
                        GetParam4K5();
                    }
                    break;
                case CAMERATYPE.CAMERA_4K_V_WF:
                    {

                        m_strName = ("CAMERA_4K_WF");
                        GetParam4K5();
                    }
                    break;
                default:
                    return;
            }
            //m_Resolution = new TagResolution[m_nResCount];
            nwidth = 0;
            nheight = 0;
            for (x = 0; x < m_nResCount; x++)
            {
                if (nwidth < (uint)m_lpszResolution[x].width)
                {
                    nwidth = (uint)m_lpszResolution[x].width;
                }
                if (nheight < (uint)m_lpszResolution[x].height)
                {
                    nheight = (uint)m_lpszResolution[x].height;
                }
            }

            m_nCollectWidth = nwidth;
            m_nCollectHeight = nheight;

            //m_pRawData = (BYTE*)new BYTE[m_nCollectWidth * m_nCollectHeight * 3 + m_nCollectWidth];
           // m_pRgbData = (BYTE*)new BYTE[m_nCollectWidth * m_nCollectHeight * 3 + m_nCollectWidth];
            //::MessageBox( NULL, _T("I123"), _T("Demo"), 0 );
            m_pRawData = new IntPtr();
            m_pRawData = Marshal.AllocHGlobal((int)m_nCollectWidth * (int)m_nCollectHeight + (int)m_nCollectWidth);
            m_pRgbData = new IntPtr();
            m_pRgbData = Marshal.AllocHGlobal((int)m_nCollectWidth * (int)m_nCollectHeight * 3 + (int)m_nCollectWidth);
            DllFunction.UVC_SetResolution(m_hDevice, m_nCollectWidth, m_nCollectHeight);
            int colormode = 0;
            DllFunction.UVC_GetParam(m_hDevice, DLPARAM.ColorMode, ref colormode);
            m_nColorMode = (COLOR_MODE)colormode;
            m_pFrameCallBack = new DL_FRAMECALLBACK(FrameCallback);
            //m_Resolution[0].m_width = (int)m_nCollectWidth;
            //m_Resolution[0].m_height = (int)m_nCollectHeight;

            m_CapInfo.Width = (uint)m_nCollectWidth;// (uint)m_Resolution[0].m_width;
            m_CapInfo.Height = (uint)m_nCollectHeight;// (uint)m_Resolution[0].m_height;
            m_CapInfo.Buffer = m_pRawData;
        }
            private void GetParamPYH()
            {                        
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Brightness, ref m_Exposure);
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Gain, ref m_nGain);              
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.BacklightCompensation, ref m_nR);
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.WhiteBalance, ref m_nB);
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Saturation, ref m_nSaturation);
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Contrast, ref m_nContrast);
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Sharpness, ref m_nSharpness);
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Gamma, ref m_nGamma);	
            }

             private void GetParam4K5()
            {

                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Saturation, ref m_Exposure);
	            m_nLight =get8BitValueFunc(_8BITCMD_AEBEST); 
	            m_nGain = get8BitValueFunc(_8BITCMD_GLOBALGAIN);
	            m_nSaturation = get8BitValueFunc(_8BITCMD_SATURATION);
	            m_nContrast = get8BitValueFunc(_8BITCMD_CONTRAST);
	            m_nSharpness = get8BitValueFunc(_8BITCMD_SHARPEN);
	            m_nGamma = get8BitValueFunc(_8BITCMD_HDR);
	            m_nFlipMode = get8BitValueFunc(_8BITCMD_MIRROR);

                
	            //4bit command
	            m_nR = get4BitValueFunc(_4BITCMD_RGAIN);
	            m_nG = get4BitValueFunc(_4BITCMD_GGAIN);
	            m_nB = get4BitValueFunc(_4BITCMD_BGAIN);

                //Console.WriteLine("-----{0} {1}----", m_nContrast, m_nB);

	            int n = get8BitValueFunc(_8BITCMD_AE);
	            if(n==0)
		            m_bAEing=false;
	            else
		            m_bAEing = true;


            }
            private void GetParam4K6()
            {
                DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Saturation, ref m_Exposure);
	            m_nLight =get8BitValueFunc(_8BITCMD_AEBEST); 
	            m_nGain = get8BitValueFunc(_8BITCMD_GLOBALGAIN);
	            m_nSaturation = get8BitValueFunc(_8BITCMD_SATURATION);
	            m_nContrast = get8BitValueFunc(_8BITCMD_CONTRAST);
	            m_nSharpness = get8BitValueFunc(_8BITCMD_SHARPEN);
	            m_nGamma = get8BitValueFunc(_8BITCMD_HDR);
	            m_nFlipMode = get8BitValueFunc(_8BITCMD_MIRROR);


	            //4bit command
	            m_nR = get4BitValueFunc(_4BITCMD_RGAIN);
	            m_nG = get4BitValueFunc(_4BITCMD_GGAIN);
	            m_nB = get4BitValueFunc(_4BITCMD_BGAIN);

	            int n = get8BitValueFunc(_8BITCMD_AE);
	            if(n==0)
		            m_bAEing=false;
	            else
		            m_bAEing = true;
            }

        public bool set4BitCmdFunc(int _nType, int _nValue)
        {
	        int tmp = _nType + _nValue;
            DllFunction.UVC_SetParam(m_hDevice, DLPARAM.BacklightCompensation, (int)tmp);
	        return true;
        }
        public int get4BitValueFunc(int _nType)
        {
	        int tmp = _nType + _4BITCMD_GETDATA;
            DllFunction.UVC_SetParam(m_hDevice, DLPARAM.BacklightCompensation, (int)tmp);
	        Thread.Sleep(1);
    
	        int value = 0;
            DllFunction.UVC_GetParam(m_hDevice, DLPARAM.BacklightCompensation, ref value);
	        value = value & 0x0fff;
	        return (int)value;
        }

        public bool set8BitCmdFunc(int _nType, int _nValue)
        {
	        int tmp = _nType + _nValue;
            DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Contrast, tmp);
	        return true;
        }
        public int get8BitValueFunc(int _nType)
        {
	        int tmp = _nType + _8BITCMD_GETDATA;
            DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Contrast, tmp);
            Thread.Sleep(1); //1ms
            int value = 0;
            DllFunction.UVC_GetParam(m_hDevice, DLPARAM.Contrast, ref value);
	        value = value & 0x00ff;
	        return (int)value;
        }


        
        public void SetParam(ENUM_Param type, long value, long value2)
        {
	        switch(type)
	        {
                case ENUM_Param.idResolution:
		        m_nCollectWidth = (uint)value;
                m_nCollectHeight = (uint)value2;
                DllFunction.UVC_SetResolution(m_hDevice, m_nCollectWidth, m_nCollectHeight);
		        return;
            case ENUM_Param.idContrast:
		        {
                    m_nContrast = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
				        set8BitCmdFunc(_8BITCMD_CONTRAST, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Contrast, (int)value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_CONTRAST, (int)value);
			        }
		        }
		
		
		        return;
            case ENUM_Param.idAEBest:
		        {
                    m_nLight = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_AEBEST, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
				        //UVC_SetParam(m_hDevice,Contrast,value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_AEBEST, (int)value);
			        }
		        }


		        return;
            case ENUM_Param.idExposure:
		
		        {
			        //m_CapInfo.Exposure=value;
                    m_Exposure = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Saturation, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Brightness, (int)value);
			        }
			        else
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Saturation, (int)value);
			        }
		        }
		
		        return;
            case ENUM_Param.idFlipMode:
		        {
                    m_nFlipMode = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_MIRROR, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
				        ;//UVC_SetParam(m_hDevice,Brightness,value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_MIRROR, (int)value);
			        }
			
			        //VideoAutoSize();
		        }
		        return;
            case ENUM_Param.idColorMode:
		        //m_nColorMode = COLOR_MODE(value);
                //UVC_SetParam(m_hDevice, DLPARAM.ColorMode, COLOR_MODE(value));
		        return;
            case ENUM_Param.idSatuation:
		        {
                    m_nSaturation = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_SATURATION, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Saturation, (int)value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_SATURATION, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idSharpness:
		        {
                    m_nSharpness = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_SHARPEN, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Sharpness, (int)value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_SHARPEN, (int)value);
			        }
		        }
		
		        return;
            case ENUM_Param.idGamma:
		        {
                    m_nGamma = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_HDR, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Gamma, (int)value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_HDR, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idAEing:
		        {
			        m_bAEing = value ==0 ? false:true;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_AE, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
				        ;//UVC_SetParam(m_hDevice,Gamma,value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_AE, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idAWBing:
		        {
			        m_bAWBing = value ==0 ? false:true;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_AWB, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
				        ;//UVC_SetParam(m_hDevice,Gamma,value);
			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_AWB, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idGain_R:
		        {
                    m_nR = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set4BitCmdFunc(_4BITCMD_RGAIN, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.BacklightCompensation, (int)value);

				
			        }
			        else
			        {
                        set4BitCmdFunc(_4BITCMD_RGAIN, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idGain_G:
		        {
                    m_nG = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set4BitCmdFunc(_4BITCMD_GGAIN, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
				        ;//UVC_SetParam(m_hDevice,WhiteBalance,value);


			        }
			        else
			        {
                        set4BitCmdFunc(_4BITCMD_GGAIN, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idGain_B:
		        {
                    m_nB = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set4BitCmdFunc(_4BITCMD_BGAIN, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.WhiteBalance, (int)value);

				
			        }
			        else
			        {
                        set4BitCmdFunc(_4BITCMD_BGAIN, (int)value);
			        }
		        }
		        return;
            case ENUM_Param.idGain:
		        {
                    m_nGain = (int)value;
                    if (m_CamType == CAMERATYPE.CAMERA_4K_V || m_CamType == CAMERATYPE.CAMERA_4K_VI)
			        {
                        set8BitCmdFunc(_8BITCMD_GLOBALGAIN, (int)value);
			        }
                    else if (m_CamType == CAMERATYPE.CAMERA_PYH_2K || m_CamType == CAMERATYPE.CAMERA_PYH_4K)
			        {
                        DllFunction.UVC_SetParam(m_hDevice, DLPARAM.Gain, (int)value);


			        }
			        else
			        {
                        set8BitCmdFunc(_8BITCMD_GLOBALGAIN, (int)value);
			        }
			
		        }
		        return;
	        default:
		        return;
	        }
        }
        
        private void DrawCrossLine(IntPtr pData)
        {
            if (!m_bCrossline)
                return;

            int actual_w, actual_h;
            actual_w = (int)m_CapInfo.Width;
            actual_h = (int)m_CapInfo.Height;

            //unsafe
            //{
            //    byte* pFirstAdd = (byte*)pData;
            //    byte* pLine = pFirstAdd + actual_w * 3 * (actual_h / 2);

            //    {
            //        for (int i = 0; i < actual_w; i++)
            //        {
            //            *(pLine + 3 * i) = 0;
            //            *(pLine + 3 * i + 1) = 0;
            //            *(pLine + 3 * i + 2) = 255;
            //        }

            //        for (int j = 0; j < actual_h; j++)
            //        {
            //            pLine = pFirstAdd + actual_w * 3 * j;
            //            *(pLine + (actual_w / 2) * 3) = 0;
            //            *(pLine + (actual_w / 2) * 3 + 1) = 0;
            //            *(pLine + (actual_w / 2) * 3 + 2) = 255;
            //        }         
            //    }//*/
            //}
        }

        private void Play(bool bPlay)
        {
            if (m_hDevice == IntPtr.Zero) return;
            m_bPlay = bPlay;
            Rectangle rect = new Rectangle(0, 0, (int)m_nCollectWidth, (int)m_nCollectHeight);

            //try
            //{
            //    Form.FromHandle(m_hWnd).RectangleToClient(rect);
            //}
            //catch (System.Exception e)
            //{
            //    m_hWnd = IntPtr.Zero;
            //    rect = new Rectangle();
            //}

            try
            {
                var host = Control.FromHandle(m_hWnd);
                if (host != null && !host.IsDisposed)
                {
                    rect = host.ClientRectangle;
                }
            }
            catch
            {
            }

            if (m_bPlay)
            {
                DllFunction.UVC_SetFrameCallback(m_hDevice, m_pFrameCallBack, this.Handle);
                DllFunction.UVC_StartView(m_hDevice, "Digital Lab", StylesValul.WS_CHILD | StylesValul.WS_VISIBLE,
                    0, 0, rect.Width, rect.Height, m_hWnd, IntPtr.Zero);
            }
            else
            {
                DllFunction.UVC_StopView(m_hDevice);
            }
        }

        public Size GetCapResolution()
        {
            return new Size((int)m_CapInfo.Width, (int)m_CapInfo.Height);
        }

        public void VideoAutoSize(bool bChange)
        {
            try
            {            
                if (bChange)
                    m_bstretch = !m_bstretch;

                if (!m_bPlay)
                {
                    Form.FromHandle(m_hWnd).Width = 1;
                    Form.FromHandle(m_hWnd).Height = 1;
                    return;
                }

                //if (!m_bstretch)
                //{
                //    Form.FromHandle(m_hWnd).Width = (int)m_CapInfo.Width;
                //    Form.FromHandle(m_hWnd).Height = (int)m_CapInfo.Height;

                //    int width, height;
                //    width = (Form.FromHandle(m_hWnd)).Parent.Width;
                //    height = Form.FromHandle(m_hWnd).Parent.Height;

                //    int left, top;
                //    left = (width - Form.FromHandle(m_hWnd).Width) / 2;
                //    top = (height - Form.FromHandle(m_hWnd).Height) / 2;

                //    left = left > 0 ? left : 0;
                //    top = top > 0 ? top : 0;

                //    Form.FromHandle(m_hWnd).Left = left;
                //    Form.FromHandle(m_hWnd).Top = top;

                //    m_vSizeX = (int)m_CapInfo.Width;
                //    m_vSizeY = (int)m_CapInfo.Height;
                   
                //}
                //else
                //{
                //    int width, height;
                //    width = Form.FromHandle(m_hWnd).Parent.ClientRectangle.Width;
                //    height = Form.FromHandle(m_hWnd).Parent.ClientRectangle.Height;

                //    width = width - width % 4;
                //    height = height - height % 4;

                //    Form.FromHandle(m_hWnd).Width = width;
                //    Form.FromHandle(m_hWnd).Height = height;
                //    Form.FromHandle(m_hWnd).Left = 0;
                //    Form.FromHandle(m_hWnd).Top = 0;

                //    m_vSizeX = width;
                //    m_vSizeY = height;
                //}

                //    DLVIDEORECT rect;
                //rect.Left=0;
                //rect.Top=0;
                //rect.Width = (short)m_vSizeX;
                //rect.Height = (short)m_vSizeY;

                ////Console.WriteLine("-----{0} {1} {2} {3}----m_vSizeX", rect.Left, rect.Top, m_vSizeX, m_vSizeY);
                ////DllFunction.USB_SetScrollOffset(m_hDevice, 0, 0);
                //DllFunction.UVC_SetViewWin(m_hDevice, ref rect);

            }
            catch (System.Exception e)
            {
            }
        }

        public bool StartView()
        {
            if (m_hDevice == IntPtr.Zero) return false;
            if (m_bPlay) return false;
            Play(true);
            VideoAutoSize(false);
            return true;
        }

        public bool StopView()
        {
            if (m_hDevice == IntPtr.Zero) return false;
            if (!m_bPlay) return false;

            if (m_bCap)
            {
                Capture2AVIStop();
            }
            Play(false);
            //VideoAutoSize(false);
            return true;
        }

        /// <summary>
        /// ������ر�ʮ����
        /// </summary>
        /// <param name="bOn"></param>
        public void EnabeCrossline(bool bOn)
        {
            m_bCrossline = bOn;
        }

        /// <summary>
        /// ��ǰԤ����ʮ����״̬
        /// </summary>
        /// <returns></returns>
        public bool IsShowCorssLine()
        {
            return m_bCrossline;
        }

        /// <summary>
        /// ���ص�ǰԤ����״̬
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            return m_bPlay;
        }

        public bool IsVideoStretch()
        {
            return m_bstretch;
        }

        /// <summary>
        /// ����¼����Ƶ
        /// </summary>
        /// <param name="szPath"></param>
        /// <returns></returns>
        public bool Capture2AVI(string szPath)
        {
            if (m_bCap)
            {
                return false;
            }
            m_bCap = true;
            m_bCapFilePath = szPath;
           // DllFunction.USB_CaptureToAvi(m_hDevice, m_bCap, m_bCapFilePath);
            return true;
        }

        /// <summary>
        /// ֹͣ¼��
        /// </summary>
        /// <returns></returns>
        public bool Capture2AVIStop()
        {
            if (!m_bCap)
            {
                return false;
            }
            m_bCap = false;
            //DllFunction.USB_CaptureToAvi(m_hDevice, m_bCap, m_bCapFilePath);
            return true;
        }

        /// <summary>
        /// �Զ���ƽ��
        /// </summary>
        /// <param name="bAWB"></param>
        /// <param name="lpContext"></param>
        public void SetDoAWB()
        {
            //DllFunction.USB_SetDoAWB(m_hDevice, true, 180, 
            //                m_pAWBCallback, this.Handle);
        }

        /// <summary>
        /// �Զ��ع�
        /// </summary>
        /// <param name="bAE"></param>
        /// <param name="lpContext"></param>
        public void SetDoAE(bool bAE)
        {
            //DllFunction.USB_SetDoAE(m_hDevice, bAE, 180, m_pAECallback, this.Handle);
            m_bAE = bAE;
        }

        /// <summary>
        /// �Ƿ��������Զ��ع�
        /// </summary>
        /// <returns></returns>
        public bool IsDoingAE()
        {
            return m_bAE;
        }

        /// <summary>
        /// �Ƿ��������Զ���ƽ��
        /// </summary>
        /// <returns></returns>
        public bool IsDoingAWB()
        {
            return m_bAWB;
        }

        /// <summary>
        /// �Ƿ��������Զ���ƽ��
        /// </summary>
        /// <returns></returns>
        public bool IsDoingCap()
        {
            return m_bCap;
        }

        /// <summary>
        /// ��ͼ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="szPath"></param>
        public void Snap(string szPath)
        {
            string strPath = szPath;
            int nRet = 0;
            switch (m_ImageType)
            {
                case IMAGETYPR.BMP:
                    strPath += ".bmp";
                    nRet = DllFunction.UVC_GetRgbFrameToBmp(m_hDevice, m_pRgbData, strPath, true);
                    break;
                case IMAGETYPR.JPEG:
                    strPath += ".jpg";
                    nRet = DllFunction.UVC_GetRgbFrameToJpeg(m_hDevice, IntPtr.Zero, strPath, 100);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// ��õ�ǰ��֡��
        /// </summary>
        /// <returns></returns>
        public float GetFrameRate()
        {
            float frame = 0.0F;
            DllFunction.UVC_GetFrameRate(m_hDevice, ref frame);
            return frame;
        }


        public void GetrgbFrame()
        {
            //����1
            unsafe
            {
                byte[,] Data = new byte[m_CapInfo.Height, m_CapInfo.Width];
                void* p;
                IntPtr ptr;
                fixed (byte* pc = Data)
                {
                    p = (void*)pc;
                    ptr = new IntPtr(p);
                }
                DllFunction.UVC_GetRgbFrame(m_hDevice, ptr);
            }
            //����2
            IntPtr ptr1 = new IntPtr();
            ptr1 = Marshal.AllocHGlobal((int)m_nCollectWidth * (int)m_nCollectHeight * 3 + (int)m_nCollectWidth);

            DllFunction.UVC_GetRgbFrame(m_hDevice, ptr1);
        }
    }
}
