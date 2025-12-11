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
using Audio900.Views;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Newtonsoft.Json;
using Audio.Services;
using Params_OUMIT_;
using System.Threading;
using static Audio.Services.PostMes;

namespace Audio900
{
    public partial class MainForm : Form
    {
        // 服务实例
        private CameraService _cameraService;
        private MultiCameraManager _multiCameraManager;
        private TemplateStorageService _templateStorageService;
        private VideoRecordingService _videoRecordingService;
        private WorkflowService _workflowService;
        
        // 当前模板和步骤
        private WorkTemplate _currentTemplate;
        private List<CogRecordDisplay> _cogDisplays = new List<CogRecordDisplay>();
        
        // 状态标志
        private bool _isWorkflowRunning = false;

        public MainForm()
        {
            InitializeComponent();
            
            // 初始化服务
            _templateStorageService = new TemplateStorageService();
            _videoRecordingService = new VideoRecordingService();
            _multiCameraManager = new MultiCameraManager();

            // 绑定扫码枪事件
            txtProductSN.KeyDown += txtProductSN_KeyDown;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 移动到OnShown以确保ActiveX控件初始化时窗口句柄已创建
            InitializeMultiCameraUI();
        }

        /// <summary>
        /// 扫码枪输入产品SN后自动触发作业流程
        /// </summary>
        private async void txtProductSN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 阻止Enter键的默认行为（如发出提示音）
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (string.IsNullOrWhiteSpace(txtProductSN.Text))
                {
                    MessageBox.Show("请扫描产品SN码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string sn = txtProductSN.Text.Trim();
                // 简单的防抖动（可选）
                await Task.Delay(10);
                
                Params.Instance.SN = sn;
                checkSN(Params.Instance.SN);

                if (_currentTemplate == null)
                {
                    MessageBox.Show("请先选择作业模板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnStart_Click(sender, e);
                
            }
        }

        public void checkSN(string sn)
        {
            try
            {
                string strMesResult = PostMes.CreateInstance().PostCheckSN(sn);
                MesResult mesResult = new MesResult();
                mesResult = JsonConvert.DeserializeAnonymousType<MesResult>(strMesResult, mesResult);
                
                // Thread.Sleep(10); // UI线程最好不要Sleep
                
                if (mesResult.Result)
                {
                    // 验证通过，禁用输入框防止误触
                    // this.txtProductSN.Enabled = false; // WinForms下禁用可能会导致无法再次扫码，视需求而定。通常扫码后会清空或全选。
                    // 这里保持WPF逻辑：验证成功则认为SN有效
                }
                else
                {
                    // 验证失败，清空
                    this.txtProductSN.Clear();
                    Params.Instance.SN = "";
                    MessageBox.Show($"SN校验失败: {mesResult.RetMsg}", "MES错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "SN校验异常");
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 更新状态
                UpdateStatus("正在初始化...");
                
                // 加载模板列表
                LoadTemplates();
                
                // 初始化多相机界面 (替代旧的 InitializeCameras)
                // InitializeMultiCameraUI 已经在构造函数中调用，这里不需要再次调用，或者可以移到这里
                // 但由于它是 async 的，构造函数里调用是 fire-and-forget。
                // 稳妥起见，我们在构造函数里不做，改在这里做，或者保持现状。
                // 之前的代码在构造函数里加了 InitializeMultiCameraUI()调用。
                // 让我们确认一下构造函数。
                
                // 初始化工作流服务
                InitializeWorkflow();

                UpdateStatus("初始化完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "Form1初始化失败");
            }
        }

        private async void InitializeMultiCameraUI()
        {
            try
            {
                // 初始化相机（自动检测）
                int cameraCount = await _multiCameraManager.InitializeCameras(this);
                
                if (cameraCount == 0)
                {
                    lblNoCamera.Visible = true;
                    lblNoCamera.Text = "未检测到相机";
                    return;
                }
                
                lblNoCamera.Visible = false;
                panelCameraDisplay.Controls.Clear();
                _cogDisplays.Clear();

                // 创建布局容器
                var tlp = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = cameraCount,
                    RowCount = 1
                };

                panelCameraDisplay.Controls.Add(tlp);
                for (int i = 0; i < cameraCount; i++)
                {
                    tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cameraCount));
                    
                    var display = new CogRecordDisplay
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.Black
                    };
                    
                    // 先添加到父容器，确保ActiveX控件初始化
                    tlp.Controls.Add(display, i, 0);
                   
                    display.HorizontalScrollBar = false;
                    display.VerticalScrollBar = false;
                    _cogDisplays.Add(display);
                }
                
                
                
                // 订阅多相机事件
                _multiCameraManager.ImageCaptured += OnCameraImageCaptured;
                _multiCameraManager.StartAllCameras();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "初始化多相机界面失败");
                MessageBox.Show($"相机初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 相机图像捕获事件处理
        /// </summary>
        private void OnCameraImageCaptured(object sender, CameraImageEventArgs e)
        {
            try
            {
                if (e.CameraIndex >= 0 && e.CameraIndex < _cogDisplays.Count && e.Image != null)
                {
                    // 在 UI 线程更新图像
                    if (InvokeRequired)
                    {
                        Invoke(new Action<object, CameraImageEventArgs>(OnCameraImageCaptured), sender, e);
                        return;
                    }

                    var display = _cogDisplays[e.CameraIndex];
                    // 使用 RecordDisplay 的 Image 属性，或者 Record 属性
                    // CogRecordDisplay 显示图像通常设置 Image 属性，或设置 Record
                    // 为了简单显示，设置 Image 即可
                    display.Image = e.Image;
                    display.Fit(true); // 适应窗口大小
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
        /// 更新步骤显示
        /// </summary>
        private void UpdateStepsDisplay()
        {
            try
            {
                flpMainSteps.Controls.Clear();
                
                if (_currentTemplate == null || _currentTemplate.Steps == null || _currentTemplate.Steps.Count == 0)
                {
                    return;
                }
                
                foreach(var step in _currentTemplate.Steps)
                {
                    // Create panel for each step
                    var stepPanel = new Panel
                    {
                        Width = flpMainSteps.Width - 25,
                        Height = 150,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.WhiteSmoke,
                        Margin = new Padding(0, 0, 0, 5),
                        Tag = step // Store step object in Tag
                    };
                    
                    var lbl = new Label
                    {
                        Text = $"步骤 {step.StepNumber}\r\n{step.Name}",
                        Dock = DockStyle.Left,
                        Width = 100,
                        TextAlign = ContentAlignment.TopLeft,
                        Padding = new Padding(5, 10, 0, 0),
                        Font = new Font("微软雅黑", 10, FontStyle.Bold),
                        BackColor = Color.Transparent
                    };
                    
                    var pic = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = step.ImageSource,
                        BackColor = Color.FromArgb(230, 230, 230)
                    };
                    
                    stepPanel.Controls.Add(pic);
                    stepPanel.Controls.Add(lbl);
                    
                    flpMainSteps.Controls.Add(stepPanel);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "更新步骤显示失败");
            }
        }

        /// <summary>
        /// 初始化工作流服务
        /// </summary>
        private void InitializeWorkflow()
        {
            _workflowService = new WorkflowService(_cameraService);
            _workflowService.StatusMessageChanged += OnWorkflowStatusMessageChanged;
            _workflowService.StateChanged += OnWorkflowStateChanged;
            _workflowService.OverallResultChanged += OnWorkflowOverallResultChanged;
            _workflowService.OnStepCompleted += OnWorkflowStepCompleted;
            _workflowService.RecordingStatusChanged += OnWorkflowRecordingStatusChanged;
            
            // 设置初始相机连接状态
            _workflowService.SetCameraConnectionStatus(_cameraService != null);
        }

        private void OnWorkflowStatusMessageChanged(object sender, string message)
        {
            UpdateStatus(message);
        }

        private void OnWorkflowStateChanged(object sender, WorkflowService.WorkflowState state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, WorkflowService.WorkflowState>(OnWorkflowStateChanged), sender, state);
                return;
            }
            
            // 根据状态更新UI
            lblMesStatus.Text = $"状态: {state}";
        }

        private void OnWorkflowOverallResultChanged(string result)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnWorkflowOverallResultChanged), result);
                return;
            }
            
            lblResult.Text = result;
            if (result == "PASS")
            {
                lblResult.BackColor = Color.FromArgb(76, 175, 80); // Green
                DrawResultGraphic(true);
            }
            else if (result == "FAIL")
            {
                lblResult.BackColor = Color.Red;
                DrawResultGraphic(false);
            }
            else
            {
                lblResult.BackColor = Color.Gray;
            }
        }

        private void OnWorkflowStepCompleted(WorkStep step)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<WorkStep>(OnWorkflowStepCompleted), step);
                return;
            }
            
            foreach(Control ctrl in flpMainSteps.Controls)
            {
                if (ctrl is Panel p && p.Tag == step)
                {
                    p.BackColor = Color.FromArgb(76, 175, 80); // 检测通过绿色,和主界面总结果保持一样
                    flpMainSteps.ScrollControlIntoView(p);
                    break;
                }
            }
        }
        
        private void OnWorkflowRecordingStatusChanged(string status, Color color)
        {
             if (InvokeRequired)
            {
                Invoke(new Action<string, Color>(OnWorkflowRecordingStatusChanged), status, color);
                return;
            }

            // 显示录制状态
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), status);
                return;
            }

            lblCameraVideoStatus.Text = $"{status}";
            lblCameraVideoStatus.BackColor = color;
        }

        /// <summary>
        /// 绘制结果框
        /// </summary>
        private void DrawResultGraphic(bool isPass)
        {
            try 
            {
                var color = isPass ? CogColorConstants.Green : CogColorConstants.Red;
                
                foreach(var display in _cogDisplays)
                {
                    // 清除之前的图形
                    display.StaticGraphics.Clear();
                    
                    // 创建一个覆盖全图的矩形框
                    // 注意：这需要有图像才能获取宽高。如果没图像，就不画。
                    if (display.Image != null)
                    {
                        var rect = new CogRectangle();
                        // VisionPro 坐标系原点通常在左上角，CenterX/Y 是矩形中心
                        rect.SetCenterWidthHeight(
                            display.Image.Width / 2.0, 
                            display.Image.Height / 2.0, 
                            display.Image.Width, 
                            display.Image.Height);
                        rect.Color = color;
                        rect.LineWidthInScreenPixels = 10; // 线宽加粗以便明显
                        
                        // 添加到显示控件
                        display.StaticGraphics.Add(rect, "ResultBox");
                    }
                }
            }
            catch(Exception ex)
            {
                LoggerService.Error(ex, "绘制结果框失败");
            }
        }

        /// <summary>
        /// 打开相机按钮点击
        /// </summary>
        private void btnOpenCamera_Click(object sender, EventArgs e)
        {
            Task.Run(async () => 
            {
                if (InvokeRequired)
                    Invoke(new Action(InitializeMultiCameraUI));
                else
                    InitializeMultiCameraUI();
            });
        }

        /// <summary>
        /// 模板管理按钮点击
        /// </summary>
        private void btnTemplateManage_Click(object sender, EventArgs e)
        {
            WorkTemplate templateToEdit = null;
            if (_currentTemplate != null)
            {
                var result = MessageBox.Show(
                    $"是否编辑当前模板 '{_currentTemplate.Name}'？\n点击'是'编辑当前模板，点击'否'创建新模板，点击'取消'返回。",
                    "模板管理",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel) return;
                if (result == DialogResult.Yes)
                {
                    templateToEdit = _currentTemplate;
                }
            }

            var createTemplateWindow = new CreateTemplateWindow(_cameraService, templateToEdit);
            if (createTemplateWindow.ShowDialog() == DialogResult.OK)
            {
                // 先保存模板数据，确保磁盘上有最新数据
                if (createTemplateWindow.CreatedTemplate != null)
                {
                    _templateStorageService.SaveTemplate(createTemplateWindow.CreatedTemplate);
                    UpdateStatus($"模板 '{createTemplateWindow.CreatedTemplate.Name}' 已保存");
                }

                // 刷新模板列表
                LoadTemplates();

                // 选中模板 (这将触发 LoadTemplate 从磁盘加载)
                if (createTemplateWindow.CreatedTemplate != null)
                {
                    int index = cmbTemplates.Items.IndexOf(createTemplateWindow.CreatedTemplate.Name);
                    if (index != -1)
                    {
                        cmbTemplates.SelectedIndex = index;
                    }
                }
            }
        }

        /// <summary>
        /// 开始检测按钮点击
        /// </summary>
        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入
                //if (string.IsNullOrWhiteSpace(txtProductSN.Text))
                //{
                //    MessageBox.Show("请输入产品SN码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    txtProductSN.Focus();
                //    return;
                //}
                
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
                
                // 启动工作流
                await _workflowService.StartWorkflow(
                    _currentTemplate, 
                    txtProductSN.Text, 
                    txtEmployeeId.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动检测失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "启动检测失败");
                
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
        private async void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                await _workflowService.StopWorkflow();
                
                // 恢复UI状态
                _isWorkflowRunning = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                cmbTemplates.Enabled = true;
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
