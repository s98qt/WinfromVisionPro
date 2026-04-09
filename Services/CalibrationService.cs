using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace Audio900.Services
{
    /// <summary>
    /// 标定服务 - 管理和应用相机标定
    /// 核心思想：所有图像必须先通过标定转换为物理坐标系
    /// </summary>
    public class CalibrationService : IDisposable
    {
        // VisionPro 标定已移除，阶段3将使用 OpenCV 重新实现
        private Dictionary<int, bool> _calibrations = new Dictionary<int, bool>();
        private const string CALIB_FOLDER = "Calibrations";
        
        /// <summary>
        /// 加载所有标定文件
        /// </summary>
        public void LoadAllCalibrations()
        {
            if (!Directory.Exists(CALIB_FOLDER))
            {
                LoggerService.Warn("标定文件夹不存在，跳过加载");
                return;
            }
            
            // VisionPro 标定功能已移除
            LoggerService.Info("标定功能已移除，将在阶段3使用 OpenCV 重新实现");
        }
        
        /// <summary>
        /// 检查指定相机是否已标定
        /// </summary>
        public bool IsCalibrated(int cameraIndex)
        {
            return _calibrations.ContainsKey(cameraIndex) && _calibrations[cameraIndex];
        }
        
        /// <summary>
        /// 获取标定对象（供外部使用）
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>标定对象</returns>
        //public ICogCalibration GetCalibration(int cameraIndex)
        //{
        //    if (!IsCalibrated(cameraIndex))
        //        return null;
            
        //    return _calibrations[cameraIndex].Calibration;
        //}
        
        /// <summary>
        /// 应用标定到图像
        /// 标准流程：加载 Transform -> Add 到图片的 CoordinateSpaceTree -> 后续工具自动使用
        /// </summary>
        /// <param name="image">原始图像（像素坐标）</param>
        /// <param name="cameraIndex">相机索引</param>
        /// <returns>关联了标定信息的图像</returns>
        public Bitmap ApplyCalibration(Bitmap image, int cameraIndex)
        {
            if (image == null)
                return null;
                
            // VisionPro 标定功能已移除，直接返回原始图像
            return image;
        }
        
        /// <summary>
        /// 获取标定的RMS误差
        /// </summary>
        public double GetRMSError(int cameraIndex)
        {
            // VisionPro 标定功能已移除
            return -1;
        }
             
        /// <summary>
        /// 获取标定信息摘要
        /// </summary>
        public string GetCalibrationSummary(int cameraIndex)
        {
            if (!IsCalibrated(cameraIndex))
                return $"相机{cameraIndex}: 未标定";
            
            double rmsError = GetRMSError(cameraIndex);
            string quality = rmsError < 0.25 ? "优秀" : (rmsError < 0.5 ? "良好" : "一般");
            
            return $"相机{cameraIndex}: 已标定 (RMS: {rmsError:F4}px, 质量: {quality})";
        }
        
        /// <summary>
        /// 清除指定相机的标定
        /// </summary>
        public void ClearCalibration(int cameraIndex)
        {
            if (_calibrations.ContainsKey(cameraIndex))
            {
                _calibrations.Remove(cameraIndex);
                LoggerService.Info($"相机{cameraIndex}标定已清除");
            }
        }
        
        /// <summary>
        /// 获取标定文件路径
        /// </summary>
        public string GetCalibrationFilePath(int cameraIndex)
        {
            return Path.Combine(CALIB_FOLDER, $"Camera{cameraIndex}_Calibration.vpp");
        }
        
        /// <summary>
        /// 检查标定文件是否存在
        /// </summary>
        public bool CalibrationFileExists(int cameraIndex)
        {
            return File.Exists(GetCalibrationFilePath(cameraIndex));
        }
        
        public void Dispose()
        {
            _calibrations.Clear();
        }
    }
}
