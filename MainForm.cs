using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Audio900.Models;
using Audio900.Services;
using Audio900.Views;
using Audio900.Controls;
using Newtonsoft.Json;
using Audio.Services;
using Params_OUMIT_;
using System.Configuration;
using System.Security.Cryptography;
using System.Threading;
using static Audio.Services.PostMes;
using OpenCvSharp.Flann;
using System.IO;

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
        private EventHandler<Bitmap> _singleCameraImageCapturedHandler;
        
        // 当前模板和步骤
        private WorkTemplate _currentTemplate;
        private List<LightweightImageDisplay> _cogDisplays = new List<LightweightImageDisplay>();
        private readonly Dictionary<int, DateTime> _freezeUntilByCameraIndex = new Dictionary<int, DateTime>();
        
        // 调试窗口管理
        private readonly Dictionary<int, Form> _debugWindowsByStep = new Dictionary<int, Form>();
        private static readonly List<Form> _allDebugWindows = new List<Form>();
        
        // 状态标志
        private bool _isWorkflowRunning = false;

        // 自动采图服务
        private AutoDatasetExportService _autoDatasetExportService;
        private volatile bool _autoCaptureEnabled = false;
        private readonly Dictionary<int, DateTime> _ngCaptureTimestamps = new Dictionary<int, DateTime>();
        private readonly Dictionary<int, DateTime> _autoCaptureTimestamps = new Dictionary<int, DateTime>();
        private readonly Dictionary<int, string> _lastCaptureFingerprints = new Dictionary<int, string>();
        private const int NG_CAPTURE_INTERVAL_MS = 5000; // NG帧采样间隔（毫秒）
        private const int AUTO_CAPTURE_DUPLICATE_INTERVAL_MS = 1500; // 同步样本去重间隔（毫秒）
        private const double AUTO_CAPTURE_HIGH_CONFIDENCE = 0.85;
        private const double AUTO_CAPTURE_LOW_CONFIDENCE = 0.55;
        
        // 实时AR跟踪相关（支持双相机独立跟踪）
        private bool[] _isLiveTrackingByCamera = new bool[2]; // 每个相机独立的跟踪状态
        private CancellationTokenSource[] _trackingCancellationByCamera = new CancellationTokenSource[2]; // 每个相机独立的取消令牌

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

            chkAutoCapture.CheckedChanged += (s, e) =>
            {
                _autoCaptureEnabled = chkAutoCapture.Checked;
                LoggerService.Info($"自动采图: {(_autoCaptureEnabled ? "已开启" : "已关闭")}");
            };

            _autoDatasetExportService = new AutoDatasetExportService();

            // 强制将扫码框输入法置为关闭（纯英文状态），防止中文输入法吃掉扫码枪字符
            txtProductSN.ImeMode = ImeMode.Disable;
            txtViewNo.ImeMode = ImeMode.Disable;
            // 绑定扫码枪事件
            txtProductSN.KeyPress += txtProductSN_KeyPress;
            txtViewNo.KeyPress += txtViewNo_KeyPress;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 移动到OnShown以确保ActiveX控件初始化时窗口句柄已创建
            InitializeMultiCameraUI();
        }

        private void txtViewNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            // \r 代表回车键 (Enter)
            if (e.KeyChar == '\r')
            {
                // 阻止发出“滴”的警告声
                e.Handled = true;

                if (string.IsNullOrWhiteSpace(txtViewNo.Text))
                {
                    MessageBox.Show("请扫描产品SN码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string toolingNO = txtViewNo.Text.Trim();
                // 如果包含回车或换行符，安全去除
                toolingNO = toolingNO.Replace("\r", "").Replace("\n", "");

                //checkSN(Params.Instance.SN, toolingNO);

                if (_currentTemplate == null)
                {
                    MessageBox.Show("请先选择作业模板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnStart_Click(sender, EventArgs.Empty);
            }
                
        }


        /// <summary>
        /// 扫码枪输入产品SN后自动触发作业流程 (使用 KeyPress 防冲突)
        /// </summary>
        private void txtProductSN_KeyPress(object sender, KeyPressEventArgs e)
        {
            // \r 代表回车键 (Enter)
            if (e.KeyChar == '\r')
            {
                // 阻止发出“滴”的警告声
                e.Handled = true;

                if (string.IsNullOrWhiteSpace(txtProductSN.Text))
                {
                    MessageBox.Show("请扫描产品SN码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string sn = txtProductSN.Text.Trim();
                // 如果包含回车或换行符，安全去除
                sn = sn.Replace("\r", "").Replace("\n", "");

                string toolingNO = txtViewNo.Text.Trim();
                // 如果包含回车或换行符，安全去除
                toolingNO = toolingNO.Replace("\r", "").Replace("\n", "");
             
                string txtBackSN;
                txtBackSN = PostMes.CreateInstance().PostMesGetSN(sn);
                MesResult mesResult = new MesResult();
                mesResult = JsonConvert.DeserializeAnonymousType<MesResult>(txtBackSN, mesResult);
                if (mesResult.Result)
                {
                    sn = Sub2SN(mesResult.RetMsg)[0];               
                }

                txtBackSN = PostMes.CreateInstance().PostMesGetWO(sn);
                MesResult mesResultWO = new MesResult();
                mesResultWO = JsonConvert.DeserializeAnonymousType<MesResult>(txtBackSN, mesResultWO);
                string workOrder = string.Empty;
                if (mesResultWO.Result)
                {
                    workOrder = Sub2SN(mesResultWO.RetMsg)[0];
                }

                txtWorkOrder.Text = workOrder;
                Params.Instance.empNo = txtEmployeeId.Text;
                Params.Instance.SN = sn;
                txtProductSN.Text = sn;
                txtViewNo.Focus();
                txtViewNo.SelectAll();



                //btnStart_Click(sender, EventArgs.Empty);
            }
        }

        private List<string> Sub2SN(string str)
        {
            List<string> list = new List<string>();
            try
            {
                string[] data = str.Split(',');
                foreach (string s in data)
                {
                    list.Add(s.Split('=')[1]);
                }
            }
            catch
            {
                return list;
            }
            return list;
        }

        private List<string> Sub2Head(string str)
        {
            List<string> list = new List<string>();
            try
            {
                string[] data = str.Split(',');
                foreach (string s in data)
                {
                    list.Add(s.Split('=')[0]);
                }
            }
            catch
            {
                return list;
            }
            return list;
        }

        public void checkSN(string sn,string toolingNo)
        {
            try
            {
                string strMesResult = PostMes.CreateInstance().PostCheckSN(sn,toolingNo);
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
                int detectedCameraCount = CameraService.GetCameraCount();
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

                var display = new LightweightImageDisplay
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                panelCameraDisplay.Controls.Add(display);
                display.BringToFront();
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

                // 根据物理屏幕数量分发显示
                Screen[] screens = Screen.AllScreens;

                for (int i = 0; i < cameraCount; i++)
                {
                    var display = new LightweightImageDisplay
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.Black,
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    if (i == 0)
                    {
                        // 相机0（主控相机）显示在主界面的 panelCameraDisplay 中
                        panelCameraDisplay.Controls.Add(display);
                    }
                    else
                    {
                        // 其他相机分别对应其他物理显示器，如果没有足够显示器，默认叠加在主屏幕
                        Screen targetScreen = screens.Length > i ? screens[i] : screens[0];

                        // 创建独立的窗体
                        var displayForm = new Views.CameraDisplayForm();
                        displayForm.StartPosition = FormStartPosition.Manual;
                        
                        displayForm.WindowState = FormWindowState.Normal;
                        displayForm.Location = targetScreen.WorkingArea.Location;
                        displayForm.Bounds = targetScreen.Bounds;
                        
                        displayForm.Controls.Clear();
                        displayForm.Controls.Add(display);
                        
                        displayForm.Show();
                        displayForm.WindowState = FormWindowState.Maximized;
                    }

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
            lock (_ngCaptureTimestamps) { _ngCaptureTimestamps.Clear(); }

            foreach (var display in _cogDisplays)
            {
                if (display == null) continue;
                try
                {
                    display.ClearGraphics();
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
                //if (!_lastUiUpdateByCameraIndex.ContainsKey(e.CameraIndex))
                //{
                //    _lastUiUpdateByCameraIndex[e.CameraIndex] = DateTime.MinValue;
                //}

                //if ((DateTime.Now - _lastUiUpdateByCameraIndex[e.CameraIndex]).TotalMilliseconds < 60)
                //{
                //    return; // 距离上次刷新不足60ms，跳过
                //}
                //_lastUiUpdateByCameraIndex[e.CameraIndex] = DateTime.Now;

                //// AR模式看门狗检查
                //if (e.CameraIndex >= 0 && e.CameraIndex < _lastArUpdateTime.Length)
                //{
                //    if ((DateTime.Now - _lastArUpdateTime[e.CameraIndex]).TotalMilliseconds < 500)
                //    {
                //        return;
                //    }
                //}

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
                display.SetImage(e.Image);

               
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
                    //PrepareLiveTrackingTools();
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
            //_workflowService.ToolBlockDebugReady += OnToolBlockDebugReady;
            _workflowService.EnableDebugPopup = chkDebugMode.Checked;
            
            // 触发异步预加载全局深度学习模型
            _ = _workflowService.PreloadGlobalModelAsync();

            // 相机连接状态现在通过 CameraService.IsConnected 属性自动获取
        }


        /// <summary>
        /// ToolBlock 调试功能已移除（阶段2）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToolBlockDebugReady(object sender, EventArgs e)
        {
            // ToolBlock 功能已完全移除
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

        // 全局变量区域定义字体，避免重复创建导致的内存泄漏
        private readonly Font _bigFont = new Font("Microsoft Sans Serif", 48, FontStyle.Bold);
        private readonly Font _midFont = new Font("Microsoft Sans Serif", 32, FontStyle.Bold);

        // 用于Yolo进行过程检测的显示
        private void OnYoloDetection(object sender, InspectionResultEventArgs e)
        {
            if (e?.Step == null) return;
            int cameraIndex = e.Step.CameraIndex;
            if (cameraIndex < 0 || cameraIndex >= _cogDisplays.Count) return;

            // 后台自动采图（当前已在后台线程，直接调用）
            TryAutoCapture(e);

            if (cameraIndex < _lastArUpdateTime.Length)
                _lastArUpdateTime[cameraIndex] = DateTime.Now;

            if (!IsHandleCreated || IsDisposed) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (cameraIndex >= _cogDisplays.Count) return;
                    var display = _cogDisplays[cameraIndex];
         
                    // 使用轻量级显示控件的新 API
                    if (e.Predictions != null && e.Predictions.Count > 0)
                    {
                        display.SetDetectionResults(
                            e.Predictions, 
                            e.Step.DetectionROI, 
                            e.Step.DetectionROIRotation, 
                            e.Step.StepNumber.ToString(), 
                            e.IsInROI);
                        
                        // 如果步骤通过，播放提示音
                        if (e.IsPassed && e.IsInROI)
                        {
                            PlayBeepSound();
                        }
                    }
                    else if (e.Image != null)
                    {
                        // 没有检测结果时，只更新图像
                        display.SetImage(e.Image);
                    }
                }
                catch { }
            }));
        }

        /// <summary>
        /// 用于VisionPro量测（阶段2会完全移除 ToolBlock 功能，暂时简化处理）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInspectionResultReady(object sender, InspectionResultEventArgs e)
        {
            // 后台自动采图（首次调用在后台线程，InvokeRequired 为 true）
            if (InvokeRequired)
            {
                TryAutoCapture(e);
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

                // 严格验证相机索引，越界直接返回
                if (cameraIndex < 0 || cameraIndex >= _cogDisplays.Count)
                {
                    LoggerService.Warn($"相机索引越界: {cameraIndex}, 显示区数量: {_cogDisplays.Count}, 步骤: {e?.Step?.StepNumber}");
                    return;
                }

                if (_cogDisplays.Count > 0 && e.Image != null)
                {
                    var display = _cogDisplays[cameraIndex];
                    LoggerService.Info($"显示检测结果 - 步骤{e?.Step?.StepNumber}, 相机{cameraIndex}, 结果:{(e.IsPassed ? "通过" : "失败")}");
                    display.SetImage(e.Image);
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
        /// 后台自动采图：将检测事件中的图像和标签按OK/NG分类保存到本地
        /// 从事件回调线程调用，不阻塞UI
        /// </summary>
        private void TryAutoCapture(InspectionResultEventArgs e)
        {
            if (!_autoCaptureEnabled) return;
            if (e?.Image == null || e?.Step == null || _currentTemplate == null || _autoDatasetExportService == null) return;

            // 在当前后台线程上克隆 Bitmap
            Bitmap bmp;
            try
            {
                bmp = (Bitmap)e.Image.Clone();
            }
            catch (Exception ex)
            {
                LoggerService.Warn($"自动采图转Bitmap失败: {ex.Message}");
                return;
            }

            var decision = BuildAutoCaptureDecision(e);
            if (decision == null)
            {
                bmp.Dispose();
                return;
            }

            string fingerprint = ComputeCaptureFingerprint(bmp);
            if (ShouldSkipDuplicateCapture(e.Step.StepNumber, fingerprint))
            {
                bmp.Dispose();
                return;
            }

            var template = _currentTemplate;
            var step = e.Step;
            var isPassed = e.IsPassed;
            var predictions = e.Predictions;
            var results = BuildAutoCaptureResults(e.Results, decision, fingerprint);
            var sn = Params.Instance.SN;
            var empId = Params.Instance.empNo;

            Task.Run(() =>
            {
                try
                {
                    _autoDatasetExportService.ExportFromBitmap(
                        template, step, bmp, isPassed, sn, empId, predictions, results);
                }
                catch (Exception ex)
                {
                    LoggerService.Error(ex, $"自动采图保存失败，步骤{step.StepNumber}，分组:{decision.BucketName}");
                }
                finally
                {
                    bmp.Dispose();
                }
            });
        }

        private Dictionary<string, double> BuildAutoCaptureResults(Dictionary<string, double> results, AutoCaptureDecision decision, string fingerprint)
        {
            var merged = results != null
                ? new Dictionary<string, double>(results)
                : new Dictionary<string, double>();

            merged["AutoCaptureTopConfidence"] = decision.TopConfidence;
            merged["AutoCaptureAvgConfidence"] = decision.AverageConfidence;
            merged["AutoCapturePredictionCount"] = decision.PredictionCount;
            merged["AutoCaptureQualityScore"] = decision.QualityScore;
            merged["AutoCaptureBucketCode"] = decision.BucketCode;
            merged["AutoCaptureIsUncertain"] = decision.IsUncertain ? 1 : 0;
            merged["AutoCaptureDuplicateHash"] = ComputeHashScore(fingerprint);
            return merged;
        }

        private AutoCaptureDecision BuildAutoCaptureDecision(InspectionResultEventArgs e)
        {
            double topConfidence = 0;
            double avgConfidence = 0;
            int predictionCount = e?.Predictions?.Count ?? 0;

            if (predictionCount > 0)
            {
                topConfidence = e.Predictions.Max(p => (double)p.Confidence);
                avgConfidence = e.Predictions.Average(p => (double)p.Confidence);
            }

            bool isUncertain = predictionCount == 0 || topConfidence < AUTO_CAPTURE_LOW_CONFIDENCE;
            double qualityScore = Math.Max(0, Math.Min(1, (topConfidence * 0.7) + (avgConfidence * 0.3)));

            string bucketName;
            int bucketCode;

            if (e.IsPassed)
            {
                if (topConfidence >= AUTO_CAPTURE_HIGH_CONFIDENCE)
                {
                    bucketName = "HighConfidence_OK";
                    bucketCode = 0;
                }
                else if (topConfidence >= AUTO_CAPTURE_LOW_CONFIDENCE)
                {
                    bucketName = "LowConfidence_OK";
                    bucketCode = 1;
                }
                else
                {
                    bucketName = "Uncertain_OK";
                    bucketCode = 2;
                    isUncertain = true;
                }
            }
            else
            {
                if (topConfidence >= AUTO_CAPTURE_HIGH_CONFIDENCE)
                {
                    bucketName = "HighConfidence_NG";
                    bucketCode = 3;
                }
                else if (topConfidence >= AUTO_CAPTURE_LOW_CONFIDENCE)
                {
                    bucketName = "LowConfidence_NG";
                    bucketCode = 4;
                }
                else
                {
                    bucketName = "Uncertain_NG";
                    bucketCode = 5;
                    isUncertain = true;
                }
            }

            return new AutoCaptureDecision
            {
                BucketName = bucketName,
                BucketCode = bucketCode,
                TopConfidence = topConfidence,
                AverageConfidence = avgConfidence,
                PredictionCount = predictionCount,
                QualityScore = qualityScore,
                IsUncertain = isUncertain
            };
        }

        private bool ShouldSkipDuplicateCapture(int stepNumber, string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
            {
                return false;
            }

            lock (_autoCaptureTimestamps)
            {
                if (_lastCaptureFingerprints.TryGetValue(stepNumber, out string lastFingerprint) &&
                    string.Equals(lastFingerprint, fingerprint, StringComparison.OrdinalIgnoreCase) &&
                    _autoCaptureTimestamps.TryGetValue(stepNumber, out DateTime lastTime) &&
                    (DateTime.Now - lastTime).TotalMilliseconds < AUTO_CAPTURE_DUPLICATE_INTERVAL_MS)
                {
                    return true;
                }

                _lastCaptureFingerprints[stepNumber] = fingerprint;
                _autoCaptureTimestamps[stepNumber] = DateTime.Now;
                return false;
            }
        }

        private string ComputeCaptureFingerprint(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            using (var thumb = new Bitmap(64, 64))
            using (var graphics = Graphics.FromImage(thumb))
            using (var ms = new MemoryStream())
            using (var sha = SHA256.Create())
            {
                graphics.DrawImage(bitmap, 0, 0, 64, 64);
                thumb.Save(ms, ImageFormat.Png);
                var hash = sha.ComputeHash(ms.ToArray());
                return Convert.ToBase64String(hash);
            }
        }

        private double ComputeHashScore(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
            {
                return 0;
            }

            int length = Math.Min(8, fingerprint.Length);
            long raw = 0;
            for (int i = 0; i < length; i++)
            {
                raw = (raw * 31) + fingerprint[i];
            }

            return Math.Abs(raw % 100000) / 100000.0;
        }

        private sealed class AutoCaptureDecision
        {
            public string BucketName { get; set; }
            public int BucketCode { get; set; }
            public double TopConfidence { get; set; }
            public double AverageConfidence { get; set; }
            public int PredictionCount { get; set; }
            public double QualityScore { get; set; }
            public bool IsUncertain { get; set; }
        }

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

            //flpMainSteps.ScrollControlIntoView(targetPanel);
            //flpMainSteps.AutoScrollPosition = new Point(0, targetPanel.Bottom + flpMainSteps.VerticalScroll.Value - flpMainSteps.ClientSize.Height);
            // 自动滚动
            flpMainSteps.AutoScrollPosition = new Point(0, targetPanel.Bottom + flpMainSteps.VerticalScroll.Value - (flpMainSteps.ClientSize.Height/2));

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
                //if (_cameraService == null || !_cameraService.IsConnected)
                //{
                //    MessageBox.Show("相机未连接！\n请先连接相机后再使用数据采集工具。", 
                //        "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}

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
        /// 停止作业流程按钮点击 - 立即终止当前检测并复位状态
        /// </summary>
        private async void btnStopWorkflow_Click(object sender, EventArgs e)
        {
            try
            {
                btnStopWorkflow.Enabled = false;
                btnStopWorkflow.Text = "停止中...";

                if (_workflowService != null)
                {
                    await _workflowService.StopWorkflow();
                }

                // 复位UI状态
                _isWorkflowRunning = false;
                lblResult.Text = "";
                lblResult.BackColor = System.Drawing.Color.LightGray;

                // 复位所有步骤面板状态
                if (_currentTemplate != null)
                {
                    foreach (var step in _currentTemplate.WorkSteps)
                    {
                        step.Status = "";
                        step.FailureReason = "";
                    }
                }

                txtProductSN.Focus();
                txtProductSN.SelectAll();
                flpMainSteps.AutoScrollPosition = new Point(0, 0);

                PrepareUiForNewRun();

                UpdateStatus("已停止作业流程，可重新开始");
                LoggerService.Info("用户手动停止作业流程");
            }
            catch (Exception ex)
            {
                LoggerService.Error(ex, "停止作业流程失败");
            }
            finally
            {
                btnStopWorkflow.Enabled = true;
                btnStopWorkflow.Text = "停止作业";
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
                    txtEmployeeId.Text, txtViewNo.Text);

                // 恢复初始化模式
                txtProductSN.Focus();
                txtProductSN.SelectAll();
                flpMainSteps.AutoScrollPosition = new Point(0, 0);
                //flpMainSteps.ScrollControlIntoView(targetPanel);
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
                // 关闭并释放所有独立的相机显示窗体
                foreach (Form openForm in Application.OpenForms.Cast<Form>().ToList())
                {
                    if (openForm is Views.CameraDisplayForm)
                    {
                        openForm.Close();
                    }
                }

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
