using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using iMG;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using USBSDK_CMOS_Demo;

namespace Audio900.Services
{
    /// <summary>
    /// 相机类型枚举
    /// </summary>
    public enum CameraType
    {
        Oumit1000,  // iCam SDK相机
        Oumit1960   // UVC SDK相机(CCamera)
    }
    /// <summary>
    /// 相机服务 - 负责 Oumit 相机的初始化、图像采集等
    /// </summary>
    public class CameraService
    {
        private bool _isRunning;
        private Thread _captureThread;
        private IntPtr _camHandle;
        private bool _capture;
        private int _width;
        private int _height;
        private int _bpp;

        // 相机类型标识
        private CameraType _cameraType;
        // 1960型号相机实例
        private CCamera _camera1960;
        // 1960相机的窗口句柄(需要外部设置)
        private IntPtr _windowHandle1960 = IntPtr.Zero;

        // iCam SDK 全局句柄缓存
        private static IntPtr[] _camHandleList = null;
        private static int _camCount = 0;
        private static bool _isSdkInitialized = false;
        private static object _sdkLock = new object();


        // VisionPro 图像用于 WinForms 显示
        private ICogImage _cogImage;
        private Control _parentControl;  // WinForms控件，用于Invoke
        private int _cameraIndex;  // 相机索引
        
        // 线程锁和最新帧缓存
        private readonly object _frameLock = new object();
        private ICogImage _latestFrame = null;  // 缓存最新的帧
        
        // 视频录制服务
        private VideoRecordingService _videoRecordingService;

        public event EventHandler<ICogImage> ImageCaptured;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CameraService(int cameraIndex = 0)
        {
            _cameraIndex = cameraIndex;
        }
        
        /// <summary>
        /// 设置视频录制服务
        /// </summary>
        public void SetVideoRecordingService(VideoRecordingService service)
        {
            _videoRecordingService = service;
        }

        /// <summary>
        /// 设置1960相机的窗口句柄(用于预览显示)
        /// </summary>
        public void SetWindowHandle(IntPtr handle)
        {
            _windowHandle1960 = handle;
        }

        /// <summary>
        /// 初始化相机 - 优先尝试1960型号,失败则尝试1000型号
        /// </summary>
        public async Task<bool> InitializeCamera(Control parentControl)
        {
            try
            {
                _parentControl = parentControl;
                
                // 优先尝试初始化1960型号相机
                //bool init1960Success = TryInitialize1960Camera();
                //if (init1960Success)
                //{
                //    _cameraType = CameraType.Oumit1960;
                //    return true;
                //}
                
                // 1960初始化失败,尝试1000型号
                bool init1000Success = TryInitialize1000Camera();
                if (init1000Success)
                {
                    _cameraType = CameraType.Oumit1000;
                    return true;
                }
                
                MessageBox.Show("未检测到相机！\n请检查相机连接。", "警告");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"相机初始化失败：{ex.Message}", "错误");
                return false;
            }
        }

        /// <summary>
        /// 获取连接的相机数量
        /// </summary>
        public static int GetCameraCount()
        {
            try
            {
                lock (_sdkLock)
                {
                    if (!_isSdkInitialized)
                    {
                        _camHandleList = new IntPtr[16];
                        iCam.Init(_camHandleList, out _camCount);
                        _isSdkInitialized = true;
                    }
                }
                
                // 如果是iCam相机，返回检测到的数量
                if (_camCount > 0) return _camCount;
                
                // 如果没有检测到iCam，可能是UVC相机，默认尝试1个
                // 或者如果有更高级的UVC检测逻辑可以在这里添加
                return 1;
            }
            catch (Exception)
            {
                // 发生异常时保守返回1
                return 1;
            }
        }

        /// <summary>
        /// 尝试初始化1960型号相机
        /// </summary>
        private bool TryInitialize1960Camera()
        {
            try
            {
                // 传入相机索引（假设UVC索引从1开始，或者根据实际情况调整）
                // 如果 _cameraIndex 是 0, 1, 2... 则传入 1, 2, 3...
                _camera1960 = new CCamera(_windowHandle1960, _cameraIndex + 1);
                
                if (_camera1960.DeviceHandle != IntPtr.Zero)
                {
                    CapInfoStruct capInfo = _camera1960.GetCapInfo();
                    _width = (int)capInfo.Width;
                    _height = (int)capInfo.Height;
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"1960相机初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 尝试初始化1000型号相机
        /// </summary>
        private bool TryInitialize1000Camera()
        {
            try
            {
                lock (_sdkLock)
                {
                    if (!_isSdkInitialized)
                    {
                        _camHandleList = new IntPtr[16];
                        iCam.Init(_camHandleList, out _camCount);
                        _isSdkInitialized = true;
                    }
                }

                if (_cameraIndex < _camCount)
                {
                    _camHandle = _camHandleList[_cameraIndex];
                    
                    // 设置曝光
                    iCam.SetExposure(_camHandle, 1);
                    
                    // 设置增益
                    iCam.SetGain(_camHandle, 1);
                    
                    // 开启自动白平衡
                    iCam.AutoWhiteBalance(_camHandle, 0, 0, 0, 0);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"1000相机初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 开始采集实时图像
        /// </summary>
        public void StartCapture()
        {
            if (_isRunning) return;

            _isRunning = true;
            _capture = true;

            if (_cameraType == CameraType.Oumit1960)
            {
                // 1960相机:启动预览和采集线程
                if (_camera1960 != null)
                {
                    _camera1960.StartView();
                    _camera1960.EnabeCrossline(false); // 默认关闭十字线
                }
                
                _captureThread = new Thread(CaptureThreadProc1960);
                _captureThread.IsBackground = true;
                _captureThread.Start();
            }
            else
            {
                // 1000相机:使用原有逻辑
                if (_camHandle != IntPtr.Zero)
                {
                    iCam.BeginCapture(_camHandle, true);
                }

                _captureThread = new Thread(CaptureThreadProc1000);
                _captureThread.IsBackground = true;
                _captureThread.Start();
            }
        }

        /// <summary>
        /// 采集线程主函数 - 1000型号相机
        /// </summary>
        private void CaptureThreadProc1000()
        {
            byte[] data = new byte[iCam.GetFrameBufferSize(_camHandle)];
            int getImageErrorCount = 0;

            while (_capture && _isRunning)
            {
                try
                {
                    int w, h, bpp;

                    // 从 Oumit 相机获取帧
                    if (iCam.GetFrame(_camHandle, data, out w, out h, out bpp))
                    {
                        getImageErrorCount = 0; // 重置错误计数

                        // 转换为 24bpp
                        if (bpp != 24)
                        {
                            iImg.AdaptBpp(data, w, h, bpp, data, 24);
                            bpp = 24;
                        }

                        // 转换为 VisionPro 图像
                        ICogImage cogImage = ConvertBGRDataToCogImage(data, w, h);
                        
                        if (cogImage == null) continue;
                        
                        // 更新最新帧缓存（供单次拍照使用）
                        lock (_frameLock)
                        {
                            _latestFrame = CopyImage(cogImage); 
                        }

                        // 如果正在录制视频，直接从相机数据创建Bitmap（避免ICogImage→Bitmap转换）
                        if (_videoRecordingService != null && _videoRecordingService.IsRecording)
                        {
                            Bitmap bitmap = CreateBitmapFromCameraData(data, w, h, bpp);
                            if (bitmap != null)
                            {
                                _videoRecordingService.WriteFrameDirect(bitmap);
                                bitmap.Dispose();
                            }
                        }

                        // 更新 VisionPro 图像 - 使用Control.BeginInvoke代替Dispatcher
                        if (_parentControl != null && !_parentControl.IsDisposed)
                        {
                            _parentControl.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    if (cogImage != null)
                                    {
                                        // 触发图像更新事件                       
                                        ImageCaptured?.Invoke(this, cogImage);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerService.Error(ex, $"图像事件触发失败: {ex.Message}");
                                }
                            }));
                        }

                        // 约30fps
                        Thread.Sleep(33);
                    }
                    else
                    {
                        getImageErrorCount++;
                        
                        if (getImageErrorCount >= 100)
                        {
                            if (_parentControl != null && !_parentControl.IsDisposed)
                            {
                                _parentControl.Invoke(new Action(() =>
                                {
                                    MessageBox.Show("相机掉线，请检查连接！", "警告");
                                }));
                            }
                            
                            _capture = false;
                            break;
                        }
                        
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 采集线程主函数 - 1960型号相机
        /// </summary>
        private void CaptureThreadProc1960()
        {
            int getImageErrorCount = 0;
            IntPtr rgbBuffer = IntPtr.Zero;

            try
            {
                if (_camera1960 == null) return;

                // 分配RGB缓冲区
                int w = (int)_camera1960.CollectWidth;
                int h = (int)_camera1960.CollectHeight;
                int bufferSize = w * h * 3;
                
                rgbBuffer = Marshal.AllocHGlobal(bufferSize + 1024);

                while (_capture && _isRunning)
                {
                    try
                    {
                        // 从1960相机获取RGB帧
                        int result = DllFunction.UVC_GetRgbFrame(_camera1960.DeviceHandle, rgbBuffer);             
                        
                        getImageErrorCount = 0;

                        // 转换为 VisionPro 图像
                        ICogImage cogImage = ConvertRgbBufferToCogImage(rgbBuffer, w, h);
                        
                        if (cogImage == null) continue;
                        
                        // 更新最新帧缓存
                        lock (_frameLock)
                        {
                            _latestFrame = CopyImage(cogImage);
                        }

                        // 如果正在录制视频
                        if (_videoRecordingService != null && _videoRecordingService.IsRecording)
                        {
                            Bitmap bitmap = CreateBitmapFromRgbBuffer(rgbBuffer, w, h);
                            if (bitmap != null)
                            {
                                _videoRecordingService.WriteFrameDirect(bitmap);
                                bitmap.Dispose();
                            }
                        }

                        // 更新UI显示 - 使用Control.BeginInvoke
                        if (_parentControl != null && !_parentControl.IsDisposed)
                        {
                            _parentControl.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    if (cogImage != null)
                                    {
                                        ImageCaptured?.Invoke(this, cogImage);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerService.Error(ex, $"图像事件触发失败: {ex.Message}");
                                }
                            }));
                        }

                        // 控制帧率，约30fps
                        Thread.Sleep(33);
                    }
                    catch (Exception ex)
                    {            
                        getImageErrorCount++;
                        if (getImageErrorCount >= 100)
                        {
                            if (_parentControl != null && !_parentControl.IsDisposed)
                            {
                                _parentControl.Invoke(new Action(() =>
                                {
                                    MessageBox.Show("相机采集出错，请检查连接！", "警告");
                                }));
                            }
                            _capture = false;
                            break;
                        }
                        
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"1960线程异常: {ex.Message}");
            }
            finally
            {
                // 释放缓冲区
                if (rgbBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(rgbBuffer);
                }
            }
        }

        private Bitmap CreateBitmapFromCameraData(byte[] data, int width, int height, int bpp)
        {
            try
            {
                if (bpp != 24)
                {
                    return null; // 只支持24bpp BGR格式
                }

                // 创建Bitmap，相机数据已经是24bpp BGR格式
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
                // 直接拷贝相机数据到Bitmap
                Marshal.Copy(data, 0, bmpData.Scan0, width * height * 3);
                bitmap.UnlockBits(bmpData);
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建Bitmap失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从RGB缓冲区创建Bitmap (1960相机)
        /// </summary>
        private Bitmap CreateBitmapFromRgbBuffer(IntPtr rgbBuffer, int width, int height)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);
                
                // 拷贝RGB数据到Bitmap
                int size = width * height * 3;
                unsafe
                {
                    byte* src = (byte*)rgbBuffer.ToPointer();
                    byte* dst = (byte*)bmpData.Scan0.ToPointer();

                    for (int i = 0; i < size; i++)
                    {
                        dst[i] = src[i];
                    }
                }

                bitmap.UnlockBits(bmpData);
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建Bitmap失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 停止采集图像
        /// </summary>
        public void StopCapture()
        {
            _isRunning = false;
            _capture = false;

            if (_captureThread != null && _captureThread.IsAlive)
            {
                _captureThread.Join(1000); // 等待线程结束
            }

            if (_cameraType == CameraType.Oumit1960)
            {
                // 停止1960相机
                _camera1960?.StopView();
            }
            else
            {
                // 停止1000相机
                if (_camHandle != IntPtr.Zero)
                {
                    try
                    {
                        iCam.StopCapture(_camHandle);
                    }
                    catch { }
                }
            }
            
            // 清理最新帧缓存
            lock (_frameLock)
            {
                if (_latestFrame is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _latestFrame = null;
            }
        }

        /// <summary>
        /// 获取当前 VisionPro 图像（用于绑定到 UI）
        /// </summary>
        public ICogImage GetCogImage()
        {
            return _cogImage;
        }

        /// <summary>
        /// 单次拍照（用于模板创建）
        /// </summary>
        public ICogImage CaptureSnapshotAsync()
        {
            try
            {
                // 如果实时采集正在运行，从缓存中获取最新帧
                if (_isRunning && _latestFrame != null)
                {
                    lock (_frameLock)
                    {
                        var copiedImage = CopyImage(_latestFrame);
                        return copiedImage;
                    }
                }
                
                // 如果实时采集未运行，执行单次拍照
                if (_cameraType == CameraType.Oumit1960)
                {
                    // 1960相机单次拍照逻辑
                    if (_camera1960 == null || _camera1960.DeviceHandle == IntPtr.Zero)
                        return null;

                    IntPtr rgbBuffer = IntPtr.Zero;
                    try
                    {
                        _camera1960.StartView();
                        Thread.Sleep(300); 

                        int w = (int)_camera1960.CollectWidth;
                        int h = (int)_camera1960.CollectHeight;
                        int bufferSize = w * h * 3;
                        rgbBuffer = Marshal.AllocHGlobal(bufferSize);

                        int result = -1;
                        for (int i = 0; i < 5; i++)
                        {
                            result = DllFunction.UVC_GetRgbFrame(_camera1960.DeviceHandle, rgbBuffer);
                            if (result == 0) break;
                            Thread.Sleep(50);
                        }

                        if (result != 0)
                        {
                            return null;
                        }

                        ICogImage snapshot = ConvertRgbBufferToCogImage(rgbBuffer, w, h);
                        return snapshot;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"1960单次拍照失败: {ex.Message}");
                        return null;
                    }
                    finally
                    {
                        _camera1960.StopView();
                        
                        if (rgbBuffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(rgbBuffer);
                    }
                }
                else
                {
                    // 1000相机单次拍照逻辑
                    iCam.BeginCapture(_camHandle, false);
                    Thread.Sleep(100); 

                    byte[] data = new byte[iCam.GetFrameBufferSize(_camHandle)];
                    int w, h, bpp;
                    if (iCam.GetFrame(_camHandle, data, out w, out h, out bpp))
                    {
                        if (bpp != 24)
                        {
                            iImg.AdaptBpp(data, w, h, bpp, data, 24);
                        }

                        ICogImage snapshot = ConvertBGRDataToCogImage(data, w, h);
                        return snapshot;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 设置曝光
        /// </summary>
        public void SetExposure(double exposure)
        {
            if (_cameraType == CameraType.Oumit1960)
            {
                if (_camera1960 != null && _camera1960.DeviceHandle != IntPtr.Zero)
                {
                    _camera1960.SetParam(ENUM_Param.idExposure, (long)exposure, 0);
                }
            }
            else
            {
                if (_camHandle != IntPtr.Zero)
                {
                    iCam.SetExposure(_camHandle, exposure);
                }
            }
        }

        /// <summary>
        /// 设置增益
        /// </summary>
        public void SetGain(byte gain)
        {
            if (_cameraType == CameraType.Oumit1960)
            {
                if (_camera1960 != null && _camera1960.DeviceHandle != IntPtr.Zero)
                {
                    _camera1960.SetParam(ENUM_Param.idGain, gain, 0);
                }
            }
            else
            {
                if (_camHandle != IntPtr.Zero)
                {
                    iCam.SetGain(_camHandle, gain);
                }
            }
        }

        /// <summary>
        /// 将 BGR 数据转换为 VisionPro 彩色图像
        /// </summary>
        private ICogImage ConvertBGRDataToCogImage(byte[] bgrData, int width, int height)
        {
            GCHandle handle = default;
            try
            {
                // 固定数组内存，获取指针
                handle = GCHandle.Alloc(bgrData, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                
                int stride = width * 3; // 24位 BGR 的步长 (假设数据是紧密排列的)
                
                // 1. 用 C# 标准 Bitmap 封装指针 (零拷贝)
                using (Bitmap bitmap = new Bitmap(width, height, stride, PixelFormat.Format24bppRgb, ptr))
                {
                    // 2. 将 Bitmap 转为 VisionPro 图像
                    return new CogImage24PlanarColor(bitmap);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"BGR数据转换失败: {ex.Message}");
                return null;
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        /// <summary>
        /// 将 RGB 缓冲区转换为 VisionPro 彩色图像
        /// </summary>
        private ICogImage ConvertRgbBufferToCogImage(IntPtr rgbBuffer, int width, int height)
        {
            try
            {
                // 使用用户提供的零拷贝方法：用 Bitmap 封装指针
                int stride = width * 3; // 24位 BGR 的步长 (假设数据是紧密排列的)
                
                // 1. 用 C# 标准 Bitmap 封装指针
                // PixelFormat.Format24bppRgb 在 Windows 下默认就是 BGR 顺序
                using (Bitmap bitmap = new Bitmap(width, height, stride, PixelFormat.Format24bppRgb, rgbBuffer))
                {
                    // 2. 将 Bitmap 转为 VisionPro 图像（VisionPro 会处理内存拷贝和重排）
                    return new CogImage24PlanarColor(bitmap);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"RGB缓冲区转换失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 复制 VisionPro 图像
        /// </summary>
        private ICogImage CopyImage(ICogImage source)
        {
            if (source == null) return null;
            
            try
            {
                if (source is CogImage8Grey gray)
                {
                    return (ICogImage)gray.Copy(CogImageCopyModeConstants.CopyPixels);
                }
                else if (source is CogImage24PlanarColor color)
                {
                    return (ICogImage)color.Copy(CogImageCopyModeConstants.CopyPixels);
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"图像复制失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 释放相机资源
        /// </summary>
        public void Dispose()
        {
            StopCapture();

            if (_cameraType == CameraType.Oumit1960)
            {
                _camera1960 = null;
            }
            else
            {
                if (_camHandle != IntPtr.Zero)
                {
                    try
                    {
                        iCam.StopCapture(_camHandle);
                    }
                    catch { }
                    
                    _camHandle = IntPtr.Zero;
                }
            }
        }
    }
}
