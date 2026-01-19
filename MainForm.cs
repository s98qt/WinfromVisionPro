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
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;
using Newtonsoft.Json;
using Audio.Services;
using Params_OUMIT_;
using System.Configuration;
using System.Threading;
using static Audio.Services.PostMes;

namespace Audio900
{
    public partial class MainForm : Form
    {
        // 服务实例
        private CameraService _cameraService;
        private TemplateStorageService _templateStorageService;
        private VideoRecordingService _videoRecordingService;
        private WorkflowService _workflowService;
        private CalibrationService _calibrationService;

        private const int _fallbackCameraCountWhenDetectFailsDefault = 2;
        private EventHandler<ICogImage> _singleCameraImageCapturedHandler;
        
        // 当前模板和步骤
        private WorkTemplate _currentTemplate;
        private List<CogRecordDisplay> _cogDisplays = new List<CogRecordDisplay>();
        private readonly Dictionary<int, DateTime> _freezeUntilByCameraIndex = new Dictionary<int, DateTime>();
        
        // 调试窗口管理
        private readonly Dictionary<int, Form> _debugWindowsByStep = new Dictionary<int, Form>();
        private static readonly List<Form> _allDebugWindows = new List<Form>();
        
        // 状态标志
        private bool _isWorkflowRunning = false;
        
        // 实时AR跟踪相关（支持双相机独立跟踪）
        private bool[] _isLiveTrackingByCamera = new bool[2]; // 每个相机独立的跟踪状态
        private CancellationTokenSource[] _trackingCancellationByCamera = new CancellationTokenSource[2]; // 每个相机独立的取消令牌
        private CogPMAlignTool[] _liveTrackToolByCamera = new CogPMAlignTool[2]; // 每个相机对应的核心工具缓存

        public MainForm()
        {
            InitializeComponent();
            
            // 初始化服务
            _templateStorageService = new TemplateStorageService();
            _videoRecordingService = new VideoRecordingService();
            _cameraService = new CameraService();
            _calibrationService = new CalibrationService();
            _calibrationService.LoadAllCalibrations();

            chkDebugMode.CheckedChanged += (s, e) =>
            {
                if (_workflowService != null)
                {
                    _workflowService.EnableDebugPopup = chkDebugMode.Checked;
                }
            };

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
        /// 从 ToolBlock 中提取核心工具（在模板加载后调用）
        /// </summary>
        private void PrepareLiveTrackingTools()
        {
            if (_currentTemplate == null || _workflowService == null)
                return;

            // 遍历模板步骤，为每个相机提取对应的工具
            foreach (var step in _currentTemplate.Steps)
            {
                int cameraIndex = step.CameraIndex;
                if (cameraIndex < 0 || cameraIndex >= _liveTrackToolByCamera.Length)
                    continue;

                if (_liveTrackToolByCamera[cameraIndex] != null)
                    continue;

                // 从 WorkflowService 获取该步骤的 ToolBlock
                if (_workflowService.GetToolBlock(step.StepNumber, out CogToolBlock toolBlock))
                {
                    // 从 ToolBlock 中查找 PMAlign 工具
                    CogPMAlignTool pmAlignTool = FindPMAlignToolInToolBlock(toolBlock);
                    
                    if (pmAlignTool != null)
                    {
                        _liveTrackToolByCamera[cameraIndex] = pmAlignTool;
                        LoggerService.Info($"相机{cameraIndex} AR跟踪工具已缓存：{pmAlignTool.Name}");
                    }
                    else
                    {
                        LoggerService.Warn($"步骤{step.StepNumber}（相机{cameraIndex}）未找到 CogPMAlignTool");
                    }
                }
            }
        }

        /// <summary>
        /// 在 ToolBlock 中递归查找 CogPMAlignTool
        /// </summary>
        private CogPMAlignTool FindPMAlignToolInToolBlock(CogToolBlock toolBlock)
        {
            if (toolBlock == null || toolBlock.Tools == null)
                return null;

            // 遍历 ToolBlock 中的所有工具
            foreach (ICogTool tool in toolBlock.Tools)
            {
                // 直接匹配 PMAlign 工具
                if (tool is CogPMAlignTool pmAlign)
                {
                    return pmAlign;
                }
                
                // 如果是嵌套的 ToolBlock，递归查找
                if (tool is CogToolBlock nestedBlock)
                {
                    var result = FindPMAlignToolInToolBlock(nestedBlock);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 扫码枪输入产品SN后自动触发作业流程
        /// </summary>
        private async void txtProductSN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 阻止Enter键的默认行为
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (string.IsNullOrWhiteSpace(txtProductSN.Text))
                {
                    MessageBox.Show("请扫描产品SN码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string sn = txtProductSN.Text.Trim();
                
                Params.Instance.SN = sn;
                //checkSN(Params.Instance.SN);

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
                
                if (mesResult.Result)
                {
                    // 验证通过，禁用输入框防止误触
                    // this.txtProductSN.Enabled = false; // WinForms下禁用可能会导致无法再次扫码
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
                // 先检测相机数量
                int detectedCameraCount = 1;/*CameraService.GetCameraCount();*/
                LoggerService.Info($"检测到 {detectedCameraCount} 个相机");

                if (detectedCameraCount == 0)
                {
                    detectedCameraCount = GetFallbackCameraCountWhenDetectFails();
                    LoggerService.Warn($"未检测到相机数量，使用配置相机数量: {detectedCameraCount}");
                }

                lblNoCamera.Visible = false;
                try
                {
                    foreach (Control c in panelCameraDisplay.Controls)
                    {
                        try { c.Dispose(); } catch { }
                    }
                }
                catch
                {
                }

                panelCameraDisplay.Controls.Clear();
                _cogDisplays.Clear();

                _freezeUntilByCameraIndex.Clear();

                try
                {
                    if (_cameraService != null)
                    {
                        _cameraService.MultiCameraImageCaptured -= OnCameraImageCaptured;

                        if (_singleCameraImageCapturedHandler != null)
                        {
                            _cameraService.ImageCaptured -= _singleCameraImageCapturedHandler;
                            _singleCameraImageCapturedHandler = null;
                        }

                        if (_cameraService.IsMultiCameraMode)
                        {
                            _cameraService.StopAllCameras();
                        }
                        else
                        {
                            _cameraService.StopCapture();
                        }
                    }
                }
                catch
                {
                }

                int actualCameraCount = 0;

                // 根据相机数量选择模式
                if (detectedCameraCount == 1)
                {
                    // 单相机模式
                    LoggerService.Info("使用单相机模式");
                    actualCameraCount = await InitializeSingleCameraMode();
                }
                else
                {
                    // 多相机模式
                    LoggerService.Info($"使用多相机模式，相机数量: {detectedCameraCount}");
                    actualCameraCount = await InitializeMultiCameraMode(detectedCameraCount);
                }

                if (actualCameraCount == 0)
                {
                    lblNoCamera.Visible = true;
                    lblNoCamera.Text = "相机初始化失败";
                    LoggerService.Warn("所有相机初始化失败");
                }
                else
                {
                    LoggerService.Info($"相机初始化完成，已连接 {actualCameraCount} 个相机，状态: {(_cameraService.IsConnected ? "在线" : "离线")}");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "初始化相机界面失败");
                MessageBox.Show($"相机初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 单相机模式初始化
        /// </summary>
        private async Task<int> InitializeSingleCameraMode()
        {
            try
            {

                //var hiddenPreviewHost = new Panel
                //{
                //    Size = new Size(640, 480),
                //    Location = new Point(-2000, -2000),
                //    Visible = false
                //};

                var display = new CogRecordDisplay
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    //AutoFit = true
                };

                
                //panelCameraDisplay.Controls.Add(hiddenPreviewHost);

                panelCameraDisplay.Controls.Add(display);
                display.BringToFront();
                display.HorizontalScrollBar = false;
                display.VerticalScrollBar = false;
                _cogDisplays.Add(display);

                //_cameraService.SetWindowHandle(hiddenPreviewHost.Handle);

                // 初始化单个相机
                bool success = await _cameraService.InitializeCamera(this);

                if (!success)
                {
                    LoggerService.Warn("单相机初始化失败");
                    return 0;
                }

                // 订阅单相机图像事件
                if (_singleCameraImageCapturedHandler != null)
                {
                    _cameraService.ImageCaptured -= _singleCameraImageCapturedHandler;
                }

                _singleCameraImageCapturedHandler = (s, image) =>
                {
                    OnCameraImageCaptured(s, new CameraImageEventArgs
                    {
                        CameraIndex = 0,
                        Image = image
                    });
                };

                _cameraService.ImageCaptured += _singleCameraImageCapturedHandler;

                // 启动采集
                _cameraService.StartCapture();

                LoggerService.Info("单相机模式初始化成功");
                return 1;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "单相机模式初始化失败");
                return 0;
            }
        }

        /// <summary>
        /// 多相机模式初始化
        /// </summary>
        private async Task<int> InitializeMultiCameraMode(int cameraCount)
        {
            try
            {
                if (cameraCount <= 0)
                {
                    return 0;
                }

                var previewHandles = new List<IntPtr>();

                // 为每个1960相机创建隐藏的预览宿主，仅用于句柄，不占用显示区域
                //for (int i = 0; i < cameraCount; i++)
                //{
                //    var hiddenPreviewHost = new Panel
                //    {
                //        Size = new Size(640, 640),
                //        Location = new Point(-2000 - (i * 10), -2000),
                //        Visible = false
                //    };
                //    panelCameraDisplay.Controls.Add(hiddenPreviewHost);
                //    previewHandles.Add(hiddenPreviewHost.Handle);
                //}

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
                        BackColor = Color.Black,
                        //
                    };

                    tlp.Controls.Add(display, i, 0);

                    display.HorizontalScrollBar = false;
                    display.VerticalScrollBar = false;
                    display.Fit(true);
                    //display.AutoFit = true;
                    _cogDisplays.Add(display);
                }

                int actualCount = await _cameraService.InitializeMultiCameras(this, previewHandles, cameraCount);
                if (actualCount == 0)
                {
                    LoggerService.Warn("多相机初始化失败");
                    return 0;
                }

                _cameraService.MultiCameraImageCaptured += OnCameraImageCaptured;
                _cameraService.StartAllCameras();

                LoggerService.Info($"多相机模式初始化成功，已连接 {actualCount} 个相机");
                return actualCount;
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "多相机模式初始化失败");
                return 0;
            }
        }

        private void PrepareUiForNewRun()
        {
            _freezeUntilByCameraIndex.Clear();

            foreach (var display in _cogDisplays)
            {
                if (display == null) continue;
                try
                {
                    display.StaticGraphics.Clear();
                    display.InteractiveGraphics.Clear();
                    display.Record = null;
                    //display.Image = null; // 图像不清空
                }
                catch
                {
                }
            }

            if (_currentTemplate?.Steps != null)
            {
                foreach (var step in _currentTemplate.Steps)
                {
                    if (step == null) continue;
                    step.Status = "";
                    step.CompletedTime = null;
                }
            }

            foreach (Control ctrl in flpMainSteps.Controls)
            {
                if (ctrl is Panel p)
                {
                    p.BackColor = Color.White;

                    WorkStep step = p.Tag as WorkStep;
                    foreach (Control child in p.Controls)
                    {
                        if (child is Label lbl)
                        {
                            if (step != null)
                            {
                                lbl.Text = $"步骤 {step.StepNumber}\r\n{step.Name}";
                            }
                            lbl.ForeColor = Color.Black;
                        }
                        //else if (child is PictureBox pic)
                        //{
                        //    pic.Image = null;
                        //}
                    }
                }
            }

            lblResult.Text = "";
            lblResult.BackColor = Color.White;
        }

        private int GetFallbackCameraCountWhenDetectFails()
        {
            try
            {
                string raw = ConfigurationManager.AppSettings["CameraCountWhenDetectFails"];
                if (int.TryParse(raw, out int value) && value > 0)
                {
                    return value;
                }
            }
            catch
            {
            }

            return _fallbackCameraCountWhenDetectFailsDefault;
        }

        // 用于限制UI刷新频率的字典
        private Dictionary<int, DateTime> _lastUiUpdateByCameraIndex = new Dictionary<int, DateTime>();
        // AR模式看门狗：记录每个相机最后一次AR更新的时间
        private DateTime[] _lastArUpdateTime = { DateTime.MinValue, DateTime.MinValue };

        /// <summary>
        /// 相机图像捕获事件处理
        /// </summary>
        private void OnCameraImageCaptured(object sender, CameraImageEventArgs e)
        {
            try
            {
                //if (_workflowService.IsArModeRunning)
                //{
                //    return;
                //}

                // 限制 UI 刷新频率为约 15 FPS (60ms)
                if (!_lastUiUpdateByCameraIndex.ContainsKey(e.CameraIndex))
                {
                    _lastUiUpdateByCameraIndex[e.CameraIndex] = DateTime.MinValue;
                }

                if ((DateTime.Now - _lastUiUpdateByCameraIndex[e.CameraIndex]).TotalMilliseconds < 60)
                {
                    return; // 距离上次刷新不足60ms，跳过
                }
                _lastUiUpdateByCameraIndex[e.CameraIndex] = DateTime.Now;

                // AR模式看门狗检查
                if (e.CameraIndex >= 0 && e.CameraIndex < _lastArUpdateTime.Length)
                {
                    if ((DateTime.Now - _lastArUpdateTime[e.CameraIndex]).TotalMilliseconds < 500)
                    {
                        return;
                    }
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new EventHandler<CameraImageEventArgs>(OnCameraImageCaptured), sender, e);
                    return;
                }

                if (e.CameraIndex < 0 || e.CameraIndex >= _cogDisplays.Count)
                {
                    return;
                }

                // 检查该相机是否处于冻结状态（正在显示检测结果）
                if (_freezeUntilByCameraIndex.ContainsKey(e.CameraIndex))
                {
                    if (DateTime.Now < _freezeUntilByCameraIndex[e.CameraIndex])
                    {
                        return; // 冻结期间不更新实时画面
                    }
                    else
                    {
                        _freezeUntilByCameraIndex.Remove(e.CameraIndex);
                    }
                }

                // 避免纯图像覆盖掉了带有检测框的 Record
                // 如果正在工作流检测 OR 该相机正在实时 AR 跟踪，都不要刷新纯图像
                bool isCameraTracking = e.CameraIndex < _isLiveTrackingByCamera.Length && _isLiveTrackingByCamera[e.CameraIndex];
                if ((_isWorkflowRunning && _currentTemplate != null) || isCameraTracking)
                {
                    return;
                }

                var display = _cogDisplays[e.CameraIndex];

                // 检查当前显示控件里是否已有图像，如果有，且不是同一张图，则必须释放旧图
                //if (display.Image != null && display.Image != e.Image)
                {
                    //var oldImage = display.Image as IDisposable;
                    //// 先断开引用
                    //display.Image = null;
                    //// 再销毁内存
                    //oldImage?.Dispose();
                    // 赋值新图
                    display.Image = e.Image;
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
                    
                    // 准备AR跟踪工具（提前提取核心工具以提升性能）
                    PrepareLiveTrackingTools();
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
                    var stepPanel = new Panel
                    {
                        Width = flpMainSteps.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 10,
                        Height = 150,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.WhiteSmoke,
                        Margin = new Padding(0, 0, 0, 5),
                        Tag = step 
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
            _workflowService = new WorkflowService(_cameraService, _calibrationService);
            _workflowService.StatusMessageChanged += OnWorkflowStatusMessageChanged;
            _workflowService.StateChanged += OnWorkflowStateChanged;
            _workflowService.OverallResultChanged += OnWorkflowOverallResultChanged;
            _workflowService.OnStepCompleted += OnWorkflowStepCompleted;
            _workflowService.RecordingStatusChanged += OnWorkflowRecordingStatusChanged;
            _workflowService.InspectionResultReady += OnInspectionResultReady;
            _workflowService.InOnYoloDetection += OnYoloDetection;
            _workflowService.ToolBlockDebugReady += OnToolBlockDebugReady;
            _workflowService.EnableDebugPopup = chkDebugMode.Checked;
            
            // 相机连接状态现在通过 CameraService.IsConnected 属性自动获取
        }


        /// <summary>
        /// 用于VisionPro的调试模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToolBlockDebugReady(object sender, ToolBlockDebugEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<ToolBlockDebugEventArgs>(OnToolBlockDebugReady), sender, e);
                return;
            }

            if (!chkDebugMode.Checked)
            {
                return;
            }

            try
            {
                Form debugForm;
                CogToolBlockEditV2 editControl;
                CogRecordDisplay recordDisplay;
                SplitContainer split;
                TableLayoutPanel bottomPanel;
                Button btnCloseAll;

                // 检查是否已有该步骤的调试窗口
                if (_debugWindowsByStep.TryGetValue(e.StepNumber, out debugForm) && !debugForm.IsDisposed)
                {
                    // 复用现有窗口，更新内容
                    debugForm.Text = $"VisionPro调试 - Step {e.StepNumber} - {e.Message}";
                    LoggerService.Info($"复用，{debugForm.Text}");
                    // 找到现有控件并更新
                    split = debugForm.Controls.OfType<TableLayoutPanel>().FirstOrDefault()?.Controls.OfType<SplitContainer>().FirstOrDefault();
                    if (split != null)
                    {
                        editControl = split.Panel1.Controls.OfType<CogToolBlockEditV2>().FirstOrDefault();
                        recordDisplay = split.Panel2.Controls.OfType<CogRecordDisplay>().FirstOrDefault();
                        
                        if (editControl != null)
                        {
                            editControl.Subject = e.ToolBlock;
                        }
                        
                        if (recordDisplay != null && e.Record != null)
                        {
                            recordDisplay.Record = e.Record;
                            recordDisplay.Fit(true);
                        }
                    }
                    
                    // 将窗口置前
                    if (debugForm.WindowState == FormWindowState.Minimized)
                    {
                        debugForm.WindowState = FormWindowState.Maximized;
                    }
                    debugForm.BringToFront();
                    debugForm.Activate();
                }
                else
                {   
                    // 创建新窗口
                    debugForm = new Form();
                    debugForm.Text = $"VisionPro调试 - Step {e.StepNumber} - {e.Message}";
                    debugForm.WindowState = FormWindowState.Maximized;
                    debugForm.StartPosition = FormStartPosition.CenterScreen;

                    // 主布局：上面是SplitContainer，下面是按钮
                    var mainLayout = new TableLayoutPanel();
                    mainLayout.Dock = DockStyle.Fill;
                    mainLayout.RowCount = 2;
                    mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                    mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

                    split = new SplitContainer();
                    split.Dock = DockStyle.Fill;
                    split.Orientation = Orientation.Vertical;
                    split.SplitterDistance = 500;

                    editControl = new CogToolBlockEditV2();
                    editControl.Dock = DockStyle.Fill;
                    editControl.Subject = e.ToolBlock;

                    recordDisplay = new CogRecordDisplay();
                    recordDisplay.Dock = DockStyle.Fill;
                    recordDisplay.BackColor = Color.Black;
                    

                    split.Panel1.Controls.Add(editControl);
                    split.Panel2.Controls.Add(recordDisplay);
                    //recordDisplay.HorizontalScrollBar = false;
                    //recordDisplay.VerticalScrollBar = false;

                    if (e.Record != null)
                    {
                        recordDisplay.Record = e.Record;
                        //recordDisplay.Fit(true);
                    }

                    // 底部按钮面板
                    bottomPanel = new TableLayoutPanel();
                    bottomPanel.Dock = DockStyle.Fill;
                    bottomPanel.ColumnCount = 3;
                    bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
                    bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));

                    btnCloseAll = new Button();
                    btnCloseAll.Text = "关闭所有调试窗";
                    btnCloseAll.Dock = DockStyle.Fill;
                    btnCloseAll.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
                    btnCloseAll.Click += (s, args) =>
                    {
                        CloseAllDebugWindows();
                    };

                    bottomPanel.Controls.Add(new Label(), 0, 0); // 占位
                    bottomPanel.Controls.Add(btnCloseAll, 1, 0);

                    mainLayout.Controls.Add(split, 0, 0);
                    mainLayout.Controls.Add(bottomPanel, 0, 1);

                    debugForm.Controls.Add(mainLayout);

                    // 窗口关闭时清理
                    debugForm.FormClosed += (s, args) =>
                    {
                        _debugWindowsByStep.Remove(e.StepNumber);
                        _allDebugWindows.Remove(debugForm);
                    };

                    // 记录窗口
                    _debugWindowsByStep[e.StepNumber] = debugForm;
                    _allDebugWindows.Add(debugForm);

                    debugForm.Show(this);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "打开VisionPro调试窗口失败");
            }
        }

        private void CloseAllDebugWindows()
        {
            try
            {
                var windowsToClose = _allDebugWindows.ToList();
                foreach (var window in windowsToClose)
                {
                    if (window != null && !window.IsDisposed)
                    {
                        window.Close();
                    }
                }
                _debugWindowsByStep.Clear();
                _allDebugWindows.Clear();
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "关闭调试窗口失败");
            }
        }

        // 【全局变量区域】定义字体，避免重复创建导致的内存泄漏
        private readonly Font _bigFont = new Font("Microsoft Sans Serif", 48, FontStyle.Bold);
        private readonly Font _midFont = new Font("Microsoft Sans Serif", 32, FontStyle.Bold);

        // 用于Yolo进行过程检测的显示
        private void OnYoloDetection(object sender, InspectionResultEventArgs e)
        {
            // 1. 基础校验
            if (e?.Step == null) return;
            int cameraIndex = e.Step.CameraIndex;
            if (cameraIndex < 0 || cameraIndex >= _cogDisplays.Count) return;

            // 2. 喂狗 (保持 AR 独占)
            if (cameraIndex < _lastArUpdateTime.Length)
                _lastArUpdateTime[cameraIndex] = DateTime.Now;
            // 3. UI 更新
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    var display = _cogDisplays[cameraIndex];
         
                    // 清空旧图形
                    display.StaticGraphics.Clear();
                    if (display.InteractiveGraphics.Count > 0) 
                        display.InteractiveGraphics.Clear();

                    // 使用 VisionProHelper 统一处理 YOLO 过程检测的显示逻辑
                    // 包括：ROI 框、检测框颜色判断、中心点标记
                    if (e.Predictions != null && e.Predictions.Count > 0)
                    {
                        VisionProHelper.ApplyYoloResultsToDisplay(display, e.Image, e.Predictions, e.Step, e.IsInROI);
                        
                        // 如果步骤通过，播放提示音
                        if (e.IsPassed && e.IsInROI)
                        {
                            PlayBeepSound();
                        }
                    }
                    else if (e.Image != null && display.Image != e.Image)
                    {
                        // 没有检测结果时，只更新图像
                        display.Image = e.Image;
                    }

                    // 6. 绘制结果判定 (FAIL 时的红框)
                    if (!e.IsPassed && e.Image != null)
                    {
                        double w = e.Image.Width;
                        double h = e.Image.Height;
                        double x = w / 2.0;
                        double y = h / 2.0; 

                        // 显示偏差值
                        if (e.Results != null && TryGetResultValue(e.Results, out double diff, "ToleranceDiff"))
                        {
                            var diffLabel = new CogGraphicLabel();
                            diffLabel.Text = $"偏差: {diff:F3}";
                            diffLabel.Color = CogColorConstants.Red;
                            diffLabel.Font = _midFont; // 使用全局 Font
                            diffLabel.Alignment = CogGraphicLabelAlignmentConstants.BottomCenter;
                            diffLabel.SetXYText(x, h - 100, diffLabel.Text); // 放在底部
                            display.StaticGraphics.Add(diffLabel, "Status");
                        }
                    }
                    else
                    {
                        // PASS 状态 
                        // 
                        //var passLabel = new CogGraphicLabel();
                        //passLabel.Text = "PASS";
                        //passLabel.Color = CogColorConstants.Green;
                        //passLabel.Font = _bigFont;
                        //passLabel.Alignment = CogGraphicLabelAlignmentConstants.TopRight;
                        //passLabel.SetXYText(e.Image.Width /*/ 2.0*/, 100, "PASS");
                        //display.StaticGraphics.Add(passLabel, "Status");
                    }
                }
                catch { }
            }));
        }

        /// <summary>
        /// 用于VisionPro量测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInspectionResultReady(object sender, InspectionResultEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<InspectionResultEventArgs>(OnInspectionResultReady), sender, e);
                return;
            }

            try
            {
                int cameraIndex = e?.Step?.CameraIndex ?? 0;

                if (cameraIndex >= 0 && cameraIndex < _lastArUpdateTime.Length)
                {
                    _lastArUpdateTime[cameraIndex] = DateTime.Now;
                }

                // 严格验证相机索引，越界直接返回，不要重置为0
                if (cameraIndex < 0 || cameraIndex >= _cogDisplays.Count)
                {
                    LoggerService.Warn($"相机索引越界: {cameraIndex}, 显示区数量: {_cogDisplays.Count}, 步骤: {e?.Step?.StepNumber}");
                    return;
                }

                if (_cogDisplays.Count > 0)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        var display = _cogDisplays[cameraIndex];

                        LoggerService.Info($"显示检测结果 - 步骤{e?.Step?.StepNumber}, 相机{cameraIndex}, 结果:{(e.IsPassed ? "通过" : "失败")}");
                        display.StaticGraphics.Clear();
                        display.InteractiveGraphics.Clear();

                        // 优先使用 Record，因为它包含所有工具的图形结果（模板匹配框、距离线等）
                        if (e.Record != null)
                        {
                            display.Record = e.Record;
                        }
                        else if (e.Image != null)
                        {
                            // 只有当图像对象不同时才更新
                            if (display.Image != e.Image)
                            {
                                display.Image = e.Image;
                            }
                        }

                        if (!e.IsPassed && e.Image != null)
                        {
                            double x = 0;
                            double y = 300;
                            double toleranceDiff = 0;

                            if (e.Results != null)
                            {
                                if (TryGetResultValue(e.Results, out double diff, "ToleranceDiff"))
                                {
                                    toleranceDiff = diff;
                                }
                            }

                            //// 3. 失败红框：覆盖整张影像（略留边），圈住整个影像区
                            //var rect = new CogRectangleAffine();
                            //double boxWidth = e.Image.Width - 82;
                            //double boxHeight = e.Image.Height - 58;
                            //rect.SetCenterLengthsRotationSkew(x, y, boxWidth, boxHeight, 0, 0);
                            //rect.Color = CogColorConstants.Red;
                            //rect.LineWidthInScreenPixels = 5;
                            //display.StaticGraphics.Add(rect, "FailureBox");
                            ////display.InteractiveGraphics.Add(rect, "FailureBox",true);

                            //// 4. FAIL 标签（移动到图像内部顶部居中，避免 Fit 时缩小图像）
                            //var failLabel = new CogGraphicLabel();
                            //failLabel.Text = "FAIL";
                            //failLabel.Color = CogColorConstants.Red;
                            //failLabel.Font = new Font("Microsoft Sans Serif", 48, FontStyle.Bold);
                            //failLabel.Alignment = CogGraphicLabelAlignmentConstants.TopCenter;
                            //failLabel.SetXYText(x, y, failLabel.Text);
                            ////display.InteractiveGraphics.Add(failLabel, "FailureLabel", true);
                            //display.StaticGraphics.Add(failLabel, "FailureLabel");

                            // 5. 在红框中心显示超差偏差值
                            var diffLabel = new CogGraphicLabel();
                            diffLabel.Text = $"超出公差值：{toleranceDiff:F3}";
                            diffLabel.Color = CogColorConstants.Red;
                            diffLabel.Font = new Font("Microsoft Sans Serif", 32, FontStyle.Bold);
                            diffLabel.Alignment = CogGraphicLabelAlignmentConstants.BottomLeft;
                            diffLabel.SetXYText(x, y, diffLabel.Text);
                            //display.InteractiveGraphics.Add(diffLabel, "ToleranceDiffLabel", true);
                            display.StaticGraphics.Add(diffLabel, "ToleranceDiffLabel");
                        }

                    }));


                    // 移除此处的 Fit，防止 StaticGraphics 导致图像缩小
                    //display.Fit(true);
                }
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "显示检测结果失败");
            }
        }

        /// <summary>
        /// 尝试从结果字典中获取值
        /// </summary>
        private bool TryGetResultValue(Dictionary<string, double> results, out double value, params string[] keys)
        {
            value = 0;
            foreach (var key in keys)
            {
                // 优先精确匹配
                if (results.TryGetValue(key, out value)) return true;
                
                // 忽略大小写匹配
                var matchKey = results.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (matchKey != null)
                {
                    value = results[matchKey];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 播放提示音
        /// </summary>
        private void PlayBeepSound()
        {
            try
            {
                System.Media.SystemSounds.Beep.Play();
            }
            catch (Exception ex)
            {
                LoggerService.Warn($"播放提示音失败: {ex.Message}");
            }
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
            }
            else if (result == "FAIL")
            {
                lblResult.BackColor = Color.Red;
            }
            else
            {
                lblResult.BackColor = Color.White;
            }
        }

        private void OnWorkflowStepCompleted(WorkStep step)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<WorkStep>(OnWorkflowStepCompleted), step);
                return;
            }

            if (step == null) return;

            Panel targetPanel = null;
            foreach (Control ctrl in flpMainSteps.Controls)
            {
                if (ctrl is Panel p)
                {
                    if (p.Tag is WorkStep panelStep && panelStep.StepNumber == step.StepNumber)
                    {
                        targetPanel = p;
                        break;
                    }
                }
            }

            if (targetPanel == null) return;

            bool isPassed = step.Status == "检测通过" || step.Status == "检测成功";
            if (isPassed)
            {
                targetPanel.BackColor = Color.FromArgb(76, 175, 80);
            }
            else if (step.Status == "检测失败")
            {
                targetPanel.BackColor = Color.FromArgb(244, 67, 54);
            }

            foreach (Control child in targetPanel.Controls)
            {
                if (child is Label lbl)
                {
                    if (isPassed)
                    {
                        lbl.Text = $"步骤 {step.StepNumber}\r\n{step.Name}\r\n检测成功";
                        lbl.ForeColor = Color.White;
                    }
                    else if (step.Status == "检测失败")
                    {
                        lbl.Text = $"步骤 {step.StepNumber}\r\n{step.Name}\r\n检测失败";
                        lbl.ForeColor = Color.White;
                    }
                    else
                    {
                        lbl.Text = $"步骤 {step.StepNumber}\r\n{step.Name}";
                        lbl.ForeColor = Color.Black;
                    }
                }
                else if (child is PictureBox pic)
                {
                    pic.Image = step.ImageSource;
                }
            }

            flpMainSteps.ScrollControlIntoView(targetPanel);
        }
        
        private void OnWorkflowRecordingStatusChanged(string status, Color color)
        {
             if (InvokeRequired)
            {
                Invoke(new Action<string, Color>(OnWorkflowRecordingStatusChanged), status, color);
                return;
            }

            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), status);
                return;
            }

            lblCameraVideoStatus.Text = $"{status}";
            lblCameraVideoStatus.BackColor = color;
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
        /// 数据采集工具按钮点击
        /// </summary>
        private void btnDataCollection_Click(object sender, EventArgs e)
        {
            try
            {
                // 检查相机是否连接
                if (_cameraService == null || !_cameraService.IsConnected)
                {
                    MessageBox.Show("相机未连接！\n请先连接相机后再使用数据采集工具。", 
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 打开数据采集窗口
                var dataCollectionWindow = new DataCollectionWindow(_cameraService);
                dataCollectionWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开数据采集工具失败: {ex.Message}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "异常：打开数据采集工具失败");
            }
        }

        /// <summary>
        /// 相机标定按钮点击
        /// </summary>
        private void btnCalibration_Click(object sender, EventArgs e)
        {
            try
            {
                // 检查相机是否连接
                if (_cameraService == null || !_cameraService.IsConnected)
                {
                    MessageBox.Show("相机未连接！\n请先连接相机后再进行标定。", 
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 选择要标定的相机（目前默认相机0，后续可扩展为选择对话框）
                int cameraIndex = 0;

                // 打开标定助手窗口
                var calibWindow = new CalibrationAssistantWindow(_cameraService, cameraIndex);
                if (calibWindow.ShowDialog() == DialogResult.OK)
                {
                    // 重新加载标定
                    _calibrationService.LoadAllCalibrations();
                    
                    // 显示标定信息
                    string summary = _calibrationService.GetCalibrationSummary(cameraIndex);
                    MessageBox.Show($"标定已更新！\n\n{summary}", 
                        "标定成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    LoggerService.Info($"相机{cameraIndex}标定已更新");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开标定助手失败: {ex.Message}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggerService.Error(ex, "异常：打开标定助手失败");
            }
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
        private  void btnStart_Click(object sender, EventArgs e)
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

                PrepareUiForNewRun();
                
                // 更新UI状态
                _isWorkflowRunning = true;

                // 启动工作流
                 _workflowService.StartWorkflow(
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
            }
            finally // 无论成功还是报错，都必须重置状态
            {
                _isWorkflowRunning = false;
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
