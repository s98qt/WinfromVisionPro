using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Audio900.Models;
using Audio900.Services;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;

namespace Audio900
{
    public partial class Form1 : Form
    {
        // 服务实例
        private CameraService _cameraService;
        private MultiCameraManager _multiCameraManager;
        private TemplateStorageService _templateStorageService;
        private VideoRecordingService _videoRecordingService;
        
        // 当前模板和步骤
        private WorkTemplate _currentTemplate;
        private List<CogDisplay> _cogDisplays = new List<CogDisplay>();
        
        // 状态标志
        private bool _isWorkflowRunning = false;

        public Form1()
        {
            InitializeComponent();
            
            // 初始化服务
            _templateStorageService = new TemplateStorageService();
            _videoRecordingService = new VideoRecordingService();
            _multiCameraManager = new MultiCameraManager();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 更新状态
                UpdateStatus("正在初始化...");
                
                // 加载模板列表
                LoadTemplates();
                
                // 初始化相机
                await InitializeCameras();
                
                UpdateStatus("初始化完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "Form1初始化失败");
            }
        }

        /// <summary>
        /// 加载模板列表
        /// </summary>
        private void LoadTemplates()
        {
            try
            {
                var templateNames = _templateStorageService.GetAllTemplateNames();
                cmbTemplates.Items.Clear();
                
                foreach (var name in templateNames)
                {
                    cmbTemplates.Items.Add(name);
                }
                
                if (cmbTemplates.Items.Count > 0)
                {
                    cmbTemplates.SelectedIndex = 0;
                }
                
                LoggerService.Info($"已加载 {templateNames.Count} 个模板");
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "加载模板列表失败");
            }
        }

        /// <summary>
        /// 初始化相机
        /// </summary>
        private async Task InitializeCameras()
        {
            try
            {
                UpdateStatus("正在初始化相机...");
                
                // 初始化第一个相机
                _cameraService = new CameraService(0);
                _cameraService.SetVideoRecordingService(_videoRecordingService);
                
                bool success = await _cameraService.InitializeCamera(this);
                
                if (success)
                {
                    // 创建相机显示控件
                    CreateCameraDisplay();
                    
                    // 订阅图像更新事件
                    _cameraService.ImageCaptured += OnCameraImageCaptured;
                    
                    // 启动相机采集
                    _cameraService.StartCapture();
                    
                    lblNoCamera.Visible = false;
                    UpdateStatus("相机初始化成功");
                    LoggerService.Info("相机初始化成功");
                }
                else
                {
                    lblNoCamera.Visible = true;
                    UpdateStatus("未检测到相机");
                    LoggerService.Warn("未检测到相机");
                }
            }
            catch (Exception ex)
            {
                lblNoCamera.Visible = true;
                UpdateStatus($"相机初始化失败: {ex.Message}");
                LoggerService.Error(ex, "相机初始化失败");
            }
        }

        /// <summary>
        /// 创建相机显示控件
        /// </summary>
        private void CreateCameraDisplay()
        {
            try
            {
                panelCameraDisplay.Controls.Clear();
                _cogDisplays.Clear();
                
                // 创建CogDisplay控件
                var cogDisplay = new CogDisplay
                {
                    Dock = DockStyle.Fill
                };
                
                panelCameraDisplay.Controls.Add(cogDisplay);
                _cogDisplays.Add(cogDisplay);
                
                LoggerService.Info("相机显示控件创建成功");
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "创建相机显示控件失败");
            }
        }

        /// <summary>
        /// 相机图像更新事件处理
        /// </summary>
        private void OnCameraImageCaptured(object sender, ICogImage image)
        {
            try
            {
                if (_cogDisplays.Count > 0 && image != null)
                {
                    _cogDisplays[0].Image = image;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "更新相机图像失败");
            }
        }

        /// <summary>
        /// 模板选择改变事件
        /// </summary>
        private void cmbTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbTemplates.SelectedItem == null)
                    return;
                
                string templateName = cmbTemplates.SelectedItem.ToString();
                _currentTemplate = _templateStorageService.LoadTemplate(templateName);
                
                if (_currentTemplate != null)
                {
                    UpdateStepsDisplay();
                    UpdateStatus($"已加载模板: {templateName}");
                    LoggerService.Info($"已加载模板: {templateName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载模板失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "加载模板失败");
            }
        }

        /// <summary>
        /// 更新步骤显示
        /// </summary>
        private void UpdateStepsDisplay()
        {
            try
            {
                listViewSteps.Items.Clear();
                
                if (_currentTemplate == null || _currentTemplate.Steps == null)
                    return;
                
                foreach (var step in _currentTemplate.Steps)
                {
                    var item = new ListViewItem($"步骤 {step.StepNumber}");
                    item.SubItems.Add("待检测");
                    item.SubItems.Add("--");
                    item.Tag = step;
                    listViewSteps.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "更新步骤显示失败");
            }
        }

        /// <summary>
        /// 模板管理按钮点击
        /// </summary>
        private void btnTemplateManage_Click(object sender, EventArgs e)
        {
            MessageBox.Show("模板管理功能待实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 开始检测按钮点击
        /// </summary>
        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入
                if (string.IsNullOrWhiteSpace(txtProductSN.Text))
                {
                    MessageBox.Show("请输入产品SN码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProductSN.Focus();
                    return;
                }
                
                if (_currentTemplate == null)
                {
                    MessageBox.Show("请先选择作业模板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // 更新UI状态
                _isWorkflowRunning = true;
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                cmbTemplates.Enabled = false;
                lblResult.Text = "检测中...";
                lblResult.ForeColor = Color.Blue;
                
                // 设置参数
                Params_OUMIT_.Params.Instance.SN = txtProductSN.Text;
                // Params_OUMIT_.Params.Instance.EmployeeID = txtEmployeeId.Text; // TODO: 添加EmployeeID属性到Params类
                
                UpdateStatus($"开始检测产品: {txtProductSN.Text}");
                LoggerService.Info($"开始检测 - SN: {txtProductSN.Text}, 模板: {_currentTemplate.Name}");
                
                // 启动视频录制
                _videoRecordingService.StartRecording(txtProductSN.Text, _currentTemplate.Name);
                
                // TODO: 实现工作流逻辑
                await Task.Delay(2000); // 模拟检测过程
                
                // 模拟检测结果
                lblResult.Text = "PASS";
                lblResult.ForeColor = Color.Green;
                
                UpdateStatus("检测完成");
                LoggerService.Info("检测完成");
                
                // 停止视频录制
                _videoRecordingService.StopRecording();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"检测过程出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "检测过程出错");
                
                lblResult.Text = "ERROR";
                lblResult.ForeColor = Color.Red;
            }
            finally
            {
                // 恢复UI状态
                _isWorkflowRunning = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                cmbTemplates.Enabled = true;
            }
        }

        /// <summary>
        /// 停止检测按钮点击
        /// </summary>
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                _isWorkflowRunning = false;
                _videoRecordingService.StopRecording();
                
                UpdateStatus("已停止检测");
                LoggerService.Info("用户停止检测");
                
                lblResult.Text = "已停止";
                lblResult.ForeColor = Color.Gray;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "停止检测失败");
            }
        }

        /// <summary>
        /// 更新状态栏
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), message);
                return;
            }
            
            toolStripStatusLabel1.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // 停止相机
                _cameraService?.StopCapture();
                _cameraService?.Dispose();
                
                // 停止视频录制
                _videoRecordingService?.Dispose();
                
                LoggerService.Info("应用程序正常关闭");
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "关闭应用程序时出错");
            }
            
            base.OnFormClosing(e);
        }
    }
}
