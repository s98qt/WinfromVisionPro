using Audio900.Services;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Windows.Forms;

namespace Audio900.Views
{
    /// <summary>
    /// 标定助手窗口 - 使用VisionPro棋盘格标定工具
    /// </summary>
    public partial class CalibrationAssistantWindow : Form
    {
        private CameraService _cameraService;
        private CogCalibCheckerboardTool _calibTool;
        private ICogImage _capturedImage;
        private int _cameraIndex;
        private Timer _previewTimer;
        
        public CalibrationAssistantWindow(CameraService cameraService, int cameraIndex)
        {
            InitializeComponent();
            _cameraService = cameraService;
            _cameraIndex = cameraIndex;
            
            _previewTimer = new Timer();
            _previewTimer.Interval = 100;
            _previewTimer.Tick += PreviewTimer_Tick;
            
            //InitializeCalibrationTool();
            UpdateInstructions();
        }
        
        /// <summary>
        /// 初始化棋盘格标定工具
        /// </summary>
        //private void InitializeCalibrationTool()
        //{
        //    _calibTool = new CogCalibCheckerboardTool();
            
        //    cogToolBlockEditV2.Subject = _calibTool ;
            
        //    LoggerService.Info("标定工具初始化完成 - 请在右侧VisionPro界面中设置参数");
        //}
        
        /// <summary>
        /// 更新操作说明
        /// </summary>
        private void UpdateInstructions()
        {
            lblInstructions.Text = 
                "【标定步骤】\n" +
                "1. 准备棋盘格标定板（建议9×9，格子边长1mm）\n" +
                "2. 在右侧VisionPro界面设置参数（块尺寸X、块尺寸Y等）\n" +
                "3. 点击开始预览，调整标定板位置和焦距\n" +
                "4. 点击抓取图像，拍摄清晰的标定板照片\n" +
                "5. 点击执行标定，系统自动计算\n" +
                "6. 查看RMS Error（< 0.25像素为优秀）\n" +
                "7. 点击保存标定，完成标定";
        }
        
        /// <summary>
        /// 开始实时预览
        /// </summary>
        private void btnStartPreview_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_cameraService.IsConnected)
                {
                    MessageBox.Show("相机未连接！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                _previewTimer.Start();
                btnStartPreview.Enabled = false;
                btnStopPreview.Enabled = true;
                btnCapture.Enabled = true;
                
                LoggerService.Info("开始实时预览");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动预览失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "启动预览失败");
            }
        }
        
        /// <summary>
        /// 停止实时预览
        /// </summary>
        private void btnStopPreview_Click(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            btnStartPreview.Enabled = true;
            btnStopPreview.Enabled = false;
            
            LoggerService.Info("停止实时预览");
        }
        
        /// <summary>
        /// 预览定时器 - 显示实时画面
        /// </summary>
        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var liveImage = _cameraService.GetLatestFrame();
                if (liveImage != null)
                {
                    cogRecordDisplay.Image = liveImage;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "预览更新失败");
            }
        }
        
        /// <summary>
        /// 抓取标定图像
        /// </summary>
        private void btnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                _previewTimer.Stop();
                
                _capturedImage = _cameraService.CaptureSnapshotAsync();
                if (_capturedImage == null)
                {
                    MessageBox.Show("图像采集失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                cogRecordDisplay.Image = _capturedImage;
                btnCalibrate.Enabled = true;
                lblStatus.Text = "图像已抓取，请设置参数后执行标定";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                
                LoggerService.Info("标定图像采集成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"采集失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "采集标定图像失败");
            }
        }
        
        /// <summary>
        /// 执行标定计算
        /// </summary>
        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_capturedImage == null)
                {
                    MessageBox.Show("请先采集标定图像！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                lblStatus.Text = "正在执行标定计算...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;
                Application.DoEvents();
                
                _calibTool.InputImage = _capturedImage as CogImage8Grey;
                
                _calibTool.Run();
                
                if (_calibTool.RunStatus.Result == CogToolResultConstants.Accept)
                {
                    double rmsError = _calibTool.Calibration.ComputedRMSError;
                    
                    lblRMSError.Text = $"RMS Error: {rmsError:F4} 像素";
                    
                    if (rmsError < 0.25)
                    {
                        lblRMSError.ForeColor = System.Drawing.Color.Green;
                        lblStatus.Text = "标定成功！精度优秀（RMS < 0.25）";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                        btnSave.Enabled = true;
                    }
                    else if (rmsError < 0.5)
                    {
                        lblRMSError.ForeColor = System.Drawing.Color.Orange;
                        lblStatus.Text = "标定成功，但精度一般（0.25 < RMS < 0.5）";
                        lblStatus.ForeColor = System.Drawing.Color.Orange;
                        btnSave.Enabled = true;
                    }
                    else
                    {
                        lblRMSError.ForeColor = System.Drawing.Color.Red;
                        lblStatus.Text = "标定精度不足（RMS > 0.5），建议重新拍摄";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        btnSave.Enabled = false;
                    }
                    
                    cogRecordDisplay.Image = _calibTool.OutputImage;
                    
                    LoggerService.Info($"标定完成 - RMS Error: {rmsError:F4}");
                }
                else
                {
                    lblStatus.Text = $"标定失败: {_calibTool.RunStatus.Message}";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    
                    MessageBox.Show($"标定失败: {_calibTool.RunStatus.Message}\n\n可能原因：\n1. 棋盘格不清晰\n2. 参数设置错误\n3. 光照不均匀", 
                        "标定失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    LoggerService.Warn($"标定失败: {_calibTool.RunStatus.Message}");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "标定计算异常";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                
                MessageBox.Show($"标定计算失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "标定计算失败");
            }
        }
        
        /// <summary>
        /// 保存标定文件
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string calibFolder = "Calibrations";
                if (!System.IO.Directory.Exists(calibFolder))
                {
                    System.IO.Directory.CreateDirectory(calibFolder);
                }
                
                string calibFile = System.IO.Path.Combine(calibFolder, $"Camera{_cameraIndex}_Calibration.vpp");
                
                CogSerializer.SaveObjectToFile(_calibTool, calibFile);
                
                double rmsError = _calibTool.Calibration.ComputedRMSError;
                
                MessageBox.Show(
                    $"标定文件已保存:\n{calibFile}\n\n" +
                    $"RMS Error: {rmsError:F4} 像素\n" +
                    $"标定质量: {(rmsError < 0.25 ? "优秀" : (rmsError < 0.5 ? "良好" : "一般"))}", 
                    "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                LoggerService.Info($"标定文件已保存: {calibFile}, RMS Error: {rmsError:F4}");
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "保存标定文件失败");
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewTimer?.Stop();
                _previewTimer?.Dispose();
                _calibTool?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
