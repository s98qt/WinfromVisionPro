using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Audio900.Services
{
    /// <summary>
    /// 多相机管理器 - 支持最多3个相机
    /// </summary>
    public class MultiCameraManager
    {
        private List<CameraService> _cameras = new List<CameraService>();
        public int ConnectedCameraCount { get; private set; }
        
        /// <summary>
        /// 相机图像更新事件
        /// </summary>
        public event EventHandler<CameraImageEventArgs> ImageCaptured;
        
        /// <summary>
        /// 自动检测并初始化所有相机（最多3个）
        /// </summary>
        public async Task<int> InitializeCameras(Control parentControl)
        {
            try
            {
                // 尝试初始化最多3个相机
                for (int i = 0; i < 3; i++)
                {
                    var camera = new CameraService(i);
                    bool success = await camera.InitializeCamera(parentControl);
                    
                    if (success)
                    {
                        _cameras.Add(camera);
                        
                        // 订阅相机图像事件
                        int cameraIndex = i;
                        camera.ImageCaptured += (s, image) =>
                        {
                            ImageCaptured?.Invoke(this, new CameraImageEventArgs
                            {
                                CameraIndex = cameraIndex,
                                Image = image
                            });
                        };
                        
                        LoggerService.Info($"相机 {i} 初始化成功");
                    }
                    else
                    {
                        // 如果第一个相机都失败，直接退出
                        if (i == 0)
                        {
                            break;
                        }
                    }
                }
                
                ConnectedCameraCount = _cameras.Count;
                return ConnectedCameraCount;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        
        /// <summary>
        /// 获取指定索引的相机
        /// </summary>
        public CameraService GetCamera(int index)
        {
            return index >= 0 && index < _cameras.Count ? _cameras[index] : null;
        }
        
        /// <summary>
        /// 启动所有相机采集
        /// </summary>
        public void StartAllCameras()
        {
            foreach (var camera in _cameras)
            {
                camera.StartCapture();
            }
            LoggerService.Info($"已启动 {_cameras.Count} 个相机");
        }
        
        /// <summary>
        /// 停止所有相机采集
        /// </summary>
        public void StopAllCameras()
        {
            foreach (var camera in _cameras)
            {
                camera.StopCapture();
            }
            LoggerService.Info("已停止所有相机");
        }
        
        /// <summary>
        /// 释放所有相机资源
        /// </summary>
        public void Dispose()
        {
            foreach (var camera in _cameras)
            {
                camera.Dispose();
            }
            _cameras.Clear();
        }
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
