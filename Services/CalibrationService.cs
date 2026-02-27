using System;
using System.Collections.Generic;
using System.IO;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;

namespace Audio900.Services
{
    /// <summary>
    /// 标定服务 - 管理和应用相机标定
    /// 核心思想：所有图像必须先通过标定转换为物理坐标系
    /// </summary>
    public class CalibrationService : IDisposable
    {
        private Dictionary<int, CogCalibCheckerboardTool> _calibrations = new Dictionary<int, CogCalibCheckerboardTool>();
        private Dictionary<int, CogCalibNPointToNPointTool> _calibApplyTools = new Dictionary<int, CogCalibNPointToNPointTool>();
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
            
            var files = Directory.GetFiles(CALIB_FOLDER, "Camera*_Calibration.vpp");
            
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string indexStr = fileName.Replace("Camera", "").Replace("_Calibration", "");
                    
                    if (int.TryParse(indexStr, out int cameraIndex))
                    {
                        var calibTool = CogSerializer.LoadObjectFromFile(file) as CogCalibCheckerboardTool;
                        
                        if (calibTool != null && calibTool.Calibration != null)
                        {
                            _calibrations[cameraIndex] = calibTool;
                            
                            double rmsError = calibTool.Calibration.ComputedRMSError;
                            LoggerService.Info($"相机{cameraIndex}标定已加载 - RMS Error: {rmsError:F4} 像素");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Error(ex, $"加载标定文件失败: {file}");
                }
            }
        }
        
        /// <summary>
        /// 检查指定相机是否已标定
        /// </summary>
        public bool IsCalibrated(int cameraIndex)
        {
            return _calibrations.ContainsKey(cameraIndex) && 
                   _calibrations[cameraIndex].Calibration != null;
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
        public ICogImage ApplyCalibration(ICogImage image, int cameraIndex)
        {
            if (image == null)
                return null;
                
            if (!IsCalibrated(cameraIndex))
            {
                LoggerService.Debug($"相机{cameraIndex}未标定，返回原始图像");
                return image;
            }
            
            try
            {
                var calibration = _calibrations[cameraIndex].Calibration;

                // VisionPro 9.0 CR2: 使用 AddSpace 方法将标定添加到坐标空间树
                // 标准流程：加载 Transform -> AddSpace 到图片的 CoordinateSpaceTree -> 后续工具自动使用

                // 从标定对象获取变换矩阵（从标定空间到像素空间的变换）
                ICogTransform2D transform = calibration.GetComputedUncalibratedFromCalibratedTransform();

                // 定义标定空间名称（自定义）
                string calibSpaceName = $"Camera{cameraIndex}_CalibrationSpace";

                // 将标定空间添加到坐标树（父空间是像素空间 "#"）
                image.CoordinateSpaceTree.AddSpace(
                    "#",                                    // 父空间名称（像素空间）
                    calibSpaceName,                         // 新空间名称（标定空间）
                    transform,                              // 变换矩阵（从标定空间到像素空间）
                    true,                                   // copyTransform
                    CogAddSpaceConstants.IgnoreDuplicate    // 如果已存在则忽略
                );

                // 设置当前选中的坐标空间为标定空间
                image.SelectedSpaceName = calibSpaceName;

                LoggerService.Debug($"相机{cameraIndex}标定已应用 - 坐标空间: {calibSpaceName}");

                return image;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, $"应用标定失败 - 相机{cameraIndex}");
                return image;
            }
        }
        
        /// <summary>
        /// 获取标定的RMS误差
        /// </summary>
        public double GetRMSError(int cameraIndex)
        {
            if (!IsCalibrated(cameraIndex))
                return -1;
            
            return _calibrations[cameraIndex].Calibration.ComputedRMSError;
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
                _calibrations[cameraIndex]?.Dispose();
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
            foreach (var calib in _calibrations.Values)
            {
                calib?.Dispose();
            }
            _calibrations.Clear();
        }
    }
}
