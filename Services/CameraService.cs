using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using iMG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
    /// 统一的相机服务 - 支持单相机和多相机管理
    /// </summary>
    public class CameraService
    {
        #region 单相机实例字段
        private bool _isRunning;
        private Thread _captureThread;
        private IntPtr _camHandle;
        private bool _capture;
        private int _width;
        private int _height;
        private int _bpp;
        private CameraType _cameraType;
        private CCamera _camera1960;
        private IntPtr _windowHandle1960 = IntPtr.Zero;
        private ICogImage _cogImage;
        private Control _parentControl;
        private int _cameraIndex;
        private readonly object _frameLock = new object();
        private ICogImage _latestFrame = null;
        private VideoRecordingService _videoRecordingService;
        private bool _isInitialized = false;
        #endregion

        #region 多相机管理字段
        private List<CameraService> _cameras = new List<CameraService>();
        private bool _isMultiCameraMode = false;
        #endregion

        #region 静态SDK管理
        private static IntPtr[] _camHandleList = null;
        private static int _camCount = 0;
        private static bool _isSdkInitialized = false;
        private static object _sdkLock = new object();
        #endregion

        #region 事件
        /// <summary>
        /// 单相机图像采集事件
        /// </summary>
        public event EventHandler<ICogImage> ImageCaptured;

        /// <summary>
        /// 多相机图像采集事件（带相机索引）
        /// </summary>
        public event EventHandler<CameraImageEventArgs> MultiCameraImageCaptured;
        #endregion

        #region 属性
        /// <summary>
        /// 相机是否已连接并初始化成功
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_isMultiCameraMode)
                {
                    // 多相机模式：至少有一个相机连接
                    return _cameras.Any(c => c.IsConnected);
                }
                else
                {
                    // 单相机模式：检查初始化状态和句柄
                    return _isInitialized && (_camHandle != IntPtr.Zero || (_camera1960 != null && _camera1960.DeviceHandle != IntPtr.Zero));
                }
            }
        }

        /// <summary>
        /// 已连接的相机数量
        /// </summary>
        public int ConnectedCameraCount
        {
            get
            {
                if (_isMultiCameraMode)
                {
                    return _cameras.Count(c => c.IsConnected);
                }
                else
                {
                    return IsConnected ? 1 : 0;
                }
            }
        }

        /// <summary>
        /// 是否正在运行采集
        /// </summary>
        public bool IsRunning => _isRunning;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数 - 单相机模式
        /// </summary>
        public CameraService(int cameraIndex = 0)
        {
            _cameraIndex = cameraIndex;
            _isMultiCameraMode = false;
        }

        /// <summary>
        /// 私有构造函数 - 用于多相机模式的内部实例
        /// </summary>
        private CameraService(int cameraIndex, bool isSubCamera)
        {
            _cameraIndex = cameraIndex;
            _isMultiCameraMode = false;
        }
        #endregion

        #region 多相机管理方法
        /// <summary>
        /// 自动检测并初始化所有相机（多相机模式）
        /// </summary>
        public async Task<int> InitializeMultiCameras(Control parentControl)
        {
            try
            {
                _isMultiCameraMode = true;
                _parentControl = parentControl;

                // 先获取实际检测到的相机数量
                int cameraCount = GetCameraCount();

                // 尝试初始化检测到的相机
                for (int i = 0; i < cameraCount; i++)
                {
                    var camera = new CameraService(i, true);
                    bool success = await camera.InitializeCamera(parentControl);

                    if (success)
                    {
                        _cameras.Add(camera);

                        // 订阅相机图像事件并转发
                        int cameraIndex = i;
                        camera.ImageCaptured += (s, image) =>
                        {
                            MultiCameraImageCaptured?.Invoke(this, new CameraImageEventArgs
                            {
                                CameraIndex = cameraIndex,
                                Image = image
                            });
                        };

                        LoggerService.Info($"相机 {i} 初始化成功");
                    }
                    else
                    {
                        LoggerService.Warn($"相机 {i} 初始化失败");
                        // 如果第一个相机都失败，直接退出
                        if (i == 0)
                        {
                            break;
                        }
                    }
                }

                return _cameras.Count;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "初始化多相机失败");
                return 0;
            }
        }

        /// <summary>
        /// 获取指定索引的相机（多相机模式）
        /// </summary>
        public CameraService GetCamera(int index)
        {
            if (!_isMultiCameraMode)
            {
                throw new InvalidOperationException("当前不是多相机模式");
            }
            return index >= 0 && index < _cameras.Count ? _cameras[index] : null;
        }

        /// <summary>
        /// 启动所有相机采集（多相机模式）
        /// </summary>
        public void StartAllCameras()
        {
            if (!_isMultiCameraMode)
            {
                throw new InvalidOperationException("当前不是多相机模式，请使用 StartCapture()");
            }

            foreach (var camera in _cameras)
            {
                camera.StartCapture();
            }
            LoggerService.Info($"已启动 {_cameras.Count} 个相机");
        }

        /// <summary>
        /// 停止所有相机采集（多相机模式）
        /// </summary>
        public void StopAllCameras()
        {
            if (!_isMultiCameraMode)
            {
                throw new InvalidOperationException("当前不是多相机模式，请使用 StopCapture()");
            }

            foreach (var camera in _cameras)
            {
                camera.StopCapture();
            }
            LoggerService.Info("已停止所有相机");
        }
        #endregion

        #region 相机检测
        /// <summary>
        /// 获取连接的相机数量
        /// </summary>
        public static int GetCameraCount()
        {
            int totalCount = 0;

            // 1. 检测 iCam (1000系列)
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
                if (_camCount > 0) totalCount += _camCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"iCam detection failed: {ex.Message}");
            }

            // 2. 检测 UVC (1960系列)
            try
            {
                int uvcCount = 0;
                DllFunction.UVC_GetTotalDeviceNum(IntPtr.Zero, ref uvcCount);
                if (uvcCount > 0) totalCount += uvcCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UVC detection failed: {ex.Message}");
            }

            // 如果两种都没检测到，默认返回1（保底）
            return totalCount > 0 ? totalCount : 1;
        }
        #endregion

        #region 单相机初始化
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
                //    _isInitialized = true;
                //    return true;
                //}

                // 1960初始化失败,尝试1000型号
                bool init1000Success = TryInitialize1000Camera();
                if (init1000Success)
                {
                    _cameraType = CameraType.Oumit1000;
                    _isInitialized = true;
                    return true;
                }

                if (_cameraIndex == 0)
                {
                    MessageBox.Show("未检测到相机！\n请检查相机连接。", "警告");
                }
                _isInitialized = false;
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"相机初始化失败：{ex.Message}", "错误");
                _isInitialized = false;
                return false;
            }
        }

        /// <summary>
        /// 尝试初始化1960型号相机
        /// </summary>
        private bool TryInitialize1960Camera()
        {
            try
            {
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
        #endregion

        #region 图像采集
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
                if (_camera1960 != null)
                {
                    _camera1960.StartView();
                    _camera1960.EnabeCrossline(false);
                }

                _captureThread = new Thread(CaptureThreadProc1960);
                _captureThread.IsBackground = true;
                _captureThread.Start();
            }
            else
            {
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

                    if (iCam.GetFrame(_camHandle, data, out w, out h, out bpp))
                    {
                        getImageErrorCount = 0;

                        if (bpp != 24)
                        {
                            iImg.AdaptBpp(data, w, h, bpp, data, 24);
                            bpp = 24;
                        }

                        ICogImage cogImage = ConvertBGRDataToCogImage(data, w, h);

                        if (cogImage == null) continue;

                        lock (_frameLock)
                        {
                            _latestFrame = CopyImage(cogImage);
                        }

                        if (_videoRecordingService != null && _videoRecordingService.IsRecording)
                        {
                            Bitmap bitmap = CreateBitmapFromCameraData(data, w, h, bpp);
                            if (bitmap != null)
                            {
                                _videoRecordingService.WriteFrameDirect(bitmap);
                                bitmap.Dispose();
                            }
                        }

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

                        Thread.Sleep(33);
                    }
                    else
                    {
                        getImageErrorCount++;

                        if (getImageErrorCount >= 100)
                        {
                            if (_parentControl != null && !_parentControl.IsDisposed)
                            {
                                _parentControl.BeginInvoke(new Action(() =>
                                {
                                    LoggerService.Warn("相机连接可能断开");
                                }));
                            }
                            break;
                        }

                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Error(ex, $"采集线程异常: {ex.Message}");
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

            while (_capture && _isRunning)
            {
                try
                {
                    if (_camera1960 == null || _camera1960.DeviceHandle == IntPtr.Zero)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    int w = (int)_camera1960.CollectWidth;
                    int h = (int)_camera1960.CollectHeight;
                    int bufferSize = w * h * 3;
                    IntPtr rgbBuffer = Marshal.AllocHGlobal(bufferSize);

                    try
                    {
                        int result = DllFunction.UVC_GetRgbFrame(_camera1960.DeviceHandle, rgbBuffer);

                        if (result == 0)
                        {
                            getImageErrorCount = 0;

                            ICogImage cogImage = ConvertRgbBufferToCogImage(rgbBuffer, w, h);

                            if (cogImage != null)
                            {
                                lock (_frameLock)
                                {
                                    _latestFrame = CopyImage(cogImage);
                                }

                                if (_videoRecordingService != null && _videoRecordingService.IsRecording)
                                {
                                    Bitmap bitmap = CreateBitmapFromRgbBuffer(rgbBuffer, w, h);
                                    if (bitmap != null)
                                    {
                                        _videoRecordingService.WriteFrameDirect(bitmap);
                                        bitmap.Dispose();
                                    }
                                }

                                if (_parentControl != null && !_parentControl.IsDisposed)
                                {
                                    _parentControl.BeginInvoke(new Action(() =>
                                    {
                                        try
                                        {
                                            ImageCaptured?.Invoke(this, cogImage);
                                        }
                                        catch (Exception ex)
                                        {
                                            LoggerService.Error(ex, $"图像事件触发失败: {ex.Message}");
                                        }
                                    }));
                                }
                            }

                            Thread.Sleep(33);
                        }
                        else
                        {
                            getImageErrorCount++;

                            if (getImageErrorCount >= 100)
                            {
                                if (_parentControl != null && !_parentControl.IsDisposed)
                                {
                                    _parentControl.BeginInvoke(new Action(() =>
                                    {
                                        LoggerService.Warn("1960相机连接可能断开");
                                    }));
                                }
                                break;
                            }

                            Thread.Sleep(50);
                        }
                    }
                    finally
                    {
                        if (rgbBuffer != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(rgbBuffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Error(ex, $"1960采集线程异常: {ex.Message}");
                    Thread.Sleep(100);
                }
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
                _captureThread.Join(1000);
            }

            if (_cameraType == CameraType.Oumit1960)
            {
                _camera1960?.StopView();
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
                }
            }

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
                if (_isRunning && _latestFrame != null)
                {
                    lock (_frameLock)
                    {
                        var copiedImage = CopyImage(_latestFrame);
                        return copiedImage;
                    }
                }

                if (_cameraType == CameraType.Oumit1960)
                {
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
        #endregion

        #region 相机参数设置
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
        #endregion

        #region 图像转换辅助方法
        private Bitmap CreateBitmapFromCameraData(byte[] data, int width, int height, int bpp)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

                try
                {
                    // 计算每行实际字节数
                    int sourceStride = width * 3; // 源数据每行字节数
                    int destStride = bmpData.Stride; // Bitmap每行字节数（可能对齐到4字节）

                    if (sourceStride == destStride)
                    {
                        // 如果 stride 相同，直接拷贝
                        int copySize = Math.Min(data.Length, height * destStride);
                        Marshal.Copy(data, 0, bmpData.Scan0, copySize);
                    }
                    else
                    {
                        // 如果 stride 不同，逐行拷贝
                        for (int y = 0; y < height; y++)
                        {
                            IntPtr destRow = bmpData.Scan0 + (y * destStride);
                            int sourceOffset = y * sourceStride;
                            int copyLength = Math.Min(sourceStride, data.Length - sourceOffset);
                            if (copyLength > 0)
                            {
                                Marshal.Copy(data, sourceOffset, destRow, copyLength);
                            }
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建Bitmap失败: {ex.Message}");
                return null;
            }
        }

        private Bitmap CreateBitmapFromRgbBuffer(IntPtr rgbBuffer, int width, int height)
        {
            try
            {
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

                try
                {
                    int sourceStride = width * 3;
                    int destStride = bmpData.Stride;

                    unsafe
                    {
                        byte* src = (byte*)rgbBuffer.ToPointer();
                        byte* dst = (byte*)bmpData.Scan0.ToPointer();

                        if (sourceStride == destStride)
                        {
                            // stride相同，直接拷贝
                            int totalSize = height * sourceStride;
                            Buffer.MemoryCopy(src, dst, totalSize, totalSize);
                        }
                        else
                        {
                            // stride不同，逐行拷贝
                            for (int y = 0; y < height; y++)
                            {
                                byte* srcRow = src + (y * sourceStride);
                                byte* dstRow = dst + (y * destStride);
                                Buffer.MemoryCopy(srcRow, dstRow, sourceStride, sourceStride);
                            }
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建Bitmap失败: {ex.Message}");
                return null;
            }
        }

        private ICogImage ConvertBGRDataToCogImage(byte[] bgrData, int width, int height)
        {
            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(bgrData, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();

                int stride = width * 3;

                using (Bitmap bitmap = new Bitmap(width, height, stride, PixelFormat.Format24bppRgb, ptr))
                {
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

        private ICogImage ConvertRgbBufferToCogImage(IntPtr rgbBuffer, int width, int height)
        {
            try
            {
                int stride = width * 3;

                using (Bitmap bitmap = new Bitmap(width, height, stride, PixelFormat.Format24bppRgb, rgbBuffer))
                {
                    return new CogImage24PlanarColor(bitmap);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"RGB缓冲区转换失败: {ex.Message}");
                return null;
            }
        }

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
        #endregion

        #region 资源释放
        /// <summary>
        /// 释放相机资源
        /// </summary>
        public void Dispose()
        {
            if (_isMultiCameraMode)
            {
                foreach (var camera in _cameras)
                {
                    camera.Dispose();
                }
                _cameras.Clear();
            }
            else
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

            _isInitialized = false;
        }
        #endregion
    }

    /// <summary>
    /// 相机图像事件参数
    /// </summary>
    public class CameraImageEventArgs : EventArgs
    {
        public int CameraIndex { get; set; }
        public ICogImage Image { get; set; }
    }
}
