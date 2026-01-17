using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Audio900.Services;
using Audio.Services;
using Cognex.VisionPro;

namespace Audio900.Views
{
    public partial class DataCollectionWindow : Form
    {
        private CameraService _cameraService;
        private int _collectedCount = 0;
        private int _targetCount = 100;
        private string _savePath;
        private Timer _autoCollectTimer;
        private Timer _previewTimer;
        private bool _isAutoMode = false;
        private int _autoIntervalMs = 500;

        public DataCollectionWindow(CameraService cameraService)
        {
            InitializeComponent();
            _cameraService = cameraService;
            
            // 初始化自动采集定时器
            _autoCollectTimer = new Timer();
            _autoCollectTimer.Tick += AutoCollectTimer_Tick;
            
            // 初始化预览定时器（实时显示相机画面）
            _previewTimer = new Timer();
            _previewTimer.Interval = 100; // 100ms刷新一次预览
            _previewTimer.Tick += PreviewTimer_Tick;
            
            // 设置窗口快捷键
            this.KeyPreview = true;
            this.KeyDown += DataCollectionWindow_KeyDown;
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 设置默认值
            numTargetCount.Value = _targetCount;
            numAutoInterval.Value = _autoIntervalMs;
            
            // 生成默认保存路径
            txtSavePath.Text = Path.Combine(@"D:\TrainingData", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            
            // 初始状态
            UpdateProgress();
            UpdateButtonStates(false);
            
            // 默认选择手动模式
            rbManualMode.Checked = true;
        }

        private void DataCollectionWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // 空格键触发拍照（仅在手动模式且已开始采集时有效）
            if (e.KeyCode == Keys.Space && !_isAutoMode && _previewTimer.Enabled)
            {
                CaptureImage();
                e.Handled = true;
            }
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 获取实时相机画面（直接获取Bitmap，不需要VisionPro控件）
                if (_cameraService != null && _cameraService.IsConnected)
                {
                    var liveBitmap = _cameraService.GetLatestFrameAsBitmap();
                    if (liveBitmap != null)
                    {
                        // 更新预览图（需要克隆，避免GDI+错误）
                        if (pictureBoxPreview.Image != null)
                        {
                            pictureBoxPreview.Image.Dispose();
                        }
                        pictureBoxPreview.Image = new Bitmap(liveBitmap);
                        liveBitmap.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "异常：预览更新失败");
            }
        }

        private void AutoCollectTimer_Tick(object sender, EventArgs e)
        {
            CaptureImage();
        }

        private void btnStartCollect_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证相机连接
                if (_cameraService == null || !_cameraService.IsConnected)
                {
                    MessageBox.Show("相机未连接，请先连接相机！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 创建保存文件夹
                _savePath = txtSavePath.Text;
                if (string.IsNullOrWhiteSpace(_savePath))
                {
                    MessageBox.Show("请设置保存路径！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }

                // 重置计数
                _collectedCount = 0;
                _targetCount = (int)numTargetCount.Value;
                UpdateProgress();

                // 启动预览
                _previewTimer.Start();

                // 根据模式启动采集
                _isAutoMode = rbAutoMode.Checked;
                if (_isAutoMode)
                {
                    _autoIntervalMs = (int)numAutoInterval.Value;
                    _autoCollectTimer.Interval = _autoIntervalMs;
                    _autoCollectTimer.Start();
                    lblStatus.Text = $"自动采集中... (间隔 {_autoIntervalMs}ms)";
                }
                else
                {
                    lblStatus.Text = "手动采集中... (按空格键拍照)";
                }

                UpdateButtonStates(true);
                LoggerService.Info($"开始采集 - 模式: {(_isAutoMode ? "自动" : "手动")}, 目标: {_targetCount}张, 路径: {_savePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动采集失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex,"异常：启动采集失败");
            }
        }

        private void btnStopCollect_Click(object sender, EventArgs e)
        {
            StopCollection();
        }

        private void StopCollection()
        {
            _autoCollectTimer.Stop();
            _previewTimer.Stop();
            
            if (pictureBoxPreview.Image != null)
            {
                pictureBoxPreview.Image.Dispose();
                pictureBoxPreview.Image = null;
            }

            UpdateButtonStates(false);
            lblStatus.Text = $"已停止 - 共采集 {_collectedCount} 张图片";
            LoggerService.Info($"停止采集 - 共采集 {_collectedCount} 张图片");
        }

        private void btnManualCapture_Click(object sender, EventArgs e)
        {
            CaptureImage();
        }

        private void CaptureImage()
        {
            try
            {
                // 检查是否已达到目标数量
                if (_collectedCount >= _targetCount)
                {
                    StopCollection();
                    MessageBox.Show($"采集完成！\n共采集 {_collectedCount} 张图片\n保存路径: {_savePath}", 
                        "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 从相机采集图像
                var image = _cameraService.CaptureSnapshotAsync();
                if (image != null)
                {
                    // 生成文件名
                    _collectedCount++;
                    string filename = $"img_{_collectedCount:D4}.bmp";
                    string fullPath = Path.Combine(_savePath, filename);

                    // 保存图片
                    using (var bmp = image.ToBitmap())
                    {
                        bmp.Save(fullPath, ImageFormat.Bmp);
                    }

                    // 更新进度
                    UpdateProgress();
                    lblLastImage.Text = $"最新: {filename}";

                    // 检查是否完成
                    if (_collectedCount >= _targetCount)
                    {
                        StopCollection();
                        MessageBox.Show($"采集完成！\n共采集 {_collectedCount} 张图片\n保存路径: {_savePath}", 
                            "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    LoggerService.Warn("采集图像失败：返回null");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"采集图像失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "异常：采集图片失败");
            }
        }

        private void UpdateProgress()
        {
            lblProgress.Text = $"已采集: {_collectedCount} / {_targetCount} 张";
            
            if (_targetCount > 0)
            {
                int percentage = (int)((_collectedCount * 100.0) / _targetCount);
                progressBar.Value = Math.Min(percentage, 100);
            }
            else
            {
                progressBar.Value = 0;
            }
        }

        private void UpdateButtonStates(bool isCollecting)
        {
            btnStartCollect.Enabled = !isCollecting;
            btnStopCollect.Enabled = isCollecting;
            btnManualCapture.Enabled = isCollecting && !_isAutoMode;
            
            // 采集中禁用模式切换和参数修改
            rbAutoMode.Enabled = !isCollecting;
            rbManualMode.Enabled = !isCollecting;
            numTargetCount.Enabled = !isCollecting;
            numAutoInterval.Enabled = !isCollecting;
            txtSavePath.Enabled = !isCollecting;
            btnBrowse.Enabled = !isCollecting;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择图片保存路径";
                dialog.SelectedPath = txtSavePath.Text;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // 在选择的路径下创建时间戳子文件夹
                    string timestampFolder = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    txtSavePath.Text = Path.Combine(dialog.SelectedPath, timestampFolder);
                }
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(_savePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", _savePath);
            }
            else
            {
                MessageBox.Show("文件夹不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void rbAutoMode_CheckedChanged(object sender, EventArgs e)
        {
            numAutoInterval.Enabled = rbAutoMode.Checked;
            lblAutoIntervalHint.Enabled = rbAutoMode.Checked;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoCollectTimer?.Dispose();
                _previewTimer?.Dispose();
                
                if (pictureBoxPreview.Image != null)
                {
                    pictureBoxPreview.Image.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
