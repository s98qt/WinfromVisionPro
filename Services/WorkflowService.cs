using Audio.Services;
using Audio900.Models;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using NLog;
using Params_OUMIT_;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Audio900.Services
{
    /// <summary>
    /// 工作流服务 - 实现业务流程状态机
    /// </summary>
    public class WorkflowService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public enum WorkflowState
        {
            Idle,           // 空闲
            ReadingCode,    // 读工扫码
            LoadingTemplate, // 加载作业模板
            AddingOumitImage, // 加载Oumit实时图像
            CheckingDefect,  // 图像缺陷检测
            CheckingScore,   // 视觉检测匹配分数
            SavingData,      // 保存图片及视频
            UploadingMES     // 上传MES
        }

        private WorkflowState _currentState;
        private CameraService _cameraService;
        private CalibrationService _calibrationService;
        private WorkTemplate _currentTemplate;
        private VideoRecordingService _videoRecordingService;

        // 离线模式测试图片索引
        private int _offlineImageIndex = 0;

        // ToolBlock 缓存
        private Dictionary<int, CogToolBlock> _stepToolBlocks = new Dictionary<int, CogToolBlock>();
        
        // Deep Learning 模型全局实例
        private YoloOBBInference _globalYoloService;
        private string _loadedGlobalModelPath;

        // 相机锁机制，防止同一相机并发访问冲突
        private static Dictionary<int, SemaphoreSlim> _cameraLocks = new Dictionary<int, SemaphoreSlim>();
        public  object _lockInitLock = new object();

        // 图像稳定性判断状态类 (用于并行执行时的线程安全)
        private class StabilityState
        {
            public ICogImage PreviousGrayImage { get; set; }
            public Stopwatch StableTimer { get; set; }
            public bool IsStabilityChecking { get; set; }
        }

        private const double GRAY_DIFF_THRESHOLD = 4.0;  // 灰度差阈值
        private const int STABLE_DURATION_MS = 2000;     // 稳定持续时间（毫秒）
        bool stepPassed = false;
        public volatile bool IsArModeRunning = false; 
       

        public event EventHandler<WorkflowState> StateChanged;
        public event EventHandler<string> StatusMessageChanged;
        public event Action<WorkStep> OnStepCompleted;
        public event Action<string> OverallResultChanged;
        public event Action<string, Color> RecordingStatusChanged; 

        // 新增：检测结果事件，携带图像和图形
        public event EventHandler<InspectionResultEventArgs> InspectionResultReady; // 用于结果的精准量测
        public event EventHandler<InspectionResultEventArgs> InOnYoloDetection; // 用于Yolo的过程检测

        public event EventHandler<ToolBlockDebugEventArgs> ToolBlockDebugReady;

        public bool EnableDebugPopup { get; set; }

        public WorkflowService(CameraService cameraService, CalibrationService calibrationService = null)
        {
            _cameraService = cameraService;
            _calibrationService = calibrationService;
            _currentState = WorkflowState.Idle;

            // 初始化视频录制服务
            _videoRecordingService = new VideoRecordingService();
            if (_cameraService != null)
            {
                _cameraService.SetVideoRecordingService(_videoRecordingService);
            }
        }

        /// <summary>
        /// 获取指定步骤的 ToolBlock（用于实时AR跟踪等场景）
        /// </summary>
        public bool GetToolBlock(int stepNumber, out CogToolBlock toolBlock)
        {
            return _stepToolBlocks.TryGetValue(stepNumber, out toolBlock);
        }


        /// <summary>
        /// 开始工作流程
        /// </summary>
        public async void StartWorkflow(WorkTemplate currentTemplate, string productSN, string employeeID)
        {
            try
            {
                _currentTemplate = currentTemplate;
                Params.Instance.SN = productSN;
                Params.Instance.empNo = employeeID;

                RecordingStatusChanged?.Invoke("视频正在录制中...", Color.Red);

                // 初始化总体结果为空
                OverallResultChanged?.Invoke("");
                // 1. 开始录制视频
                _videoRecordingService.StartRecording(
                    productSN,
                    _currentTemplate?.TemplateName ?? "未知模板");

                _logger.Info($"【作业流程开始】SN: {productSN}, 模板: {_currentTemplate?.TemplateName}");

                ChangeState(WorkflowState.LoadingTemplate);

                if (_currentTemplate != null)
                {
                    string templatePath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "HomeworkTemplate",
                        _currentTemplate.TemplateName
                    );

                    _stepToolBlocks.Clear();
                    
                    // 1. 加载全局深度学习模型 (从固定路径加载)
                    // 固定加载程序目录下 GlobalModel 文件夹内的 .onnx 模型
                    string globalModelDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GlobalModel");
                    string modelPath = null;

                    if (Directory.Exists(globalModelDir))
                    {
                        var onnxFiles = Directory.GetFiles(globalModelDir, "*.onnx");
                        if (onnxFiles.Length > 0)
                        {
                            modelPath = onnxFiles[0];
                        }
                    }

                    if (!string.IsNullOrEmpty(modelPath))
                    {
                        try
                        {
                            // 如果路径变了，或者服务未初始化，则重新加载
                            if (_globalYoloService == null || _loadedGlobalModelPath != modelPath)
                            {
                                _globalYoloService?.Dispose();

                                if (File.Exists(modelPath))
                                {
                                    _globalYoloService = new YoloOBBInference();
                                    _globalYoloService.LoadModel(modelPath, null);
                                    _loadedGlobalModelPath = modelPath;
                                    _logger.Info($"全局OBB模型已加载: {modelPath}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string errMsg = $"全局模型加载失败: {ex.Message}";
                            _logger.Error(errMsg);
                            MessageBox.Show(errMsg, "模型加载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        string msg = "未找到全局深度学习模型！\r\n请确保程序目录下 GlobalModel 文件夹内存在 .onnx 模型文件。";
                        _logger.Error(msg);
                        // 提示用户
                        MessageBox.Show(msg, "缺少模型", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    foreach (var step in _currentTemplate.WorkSteps)
                    {
                        string stepFolder = Path.Combine(templatePath, $"Step{step.StepNumber}");
                        
                        // 优先加载 ToolBlockPath (VisionPro 模式)
                        if (!string.IsNullOrEmpty(step.ToolBlockPath) && File.Exists(step.ToolBlockPath))
                        {
                            try
                            {
                                var tb = CogSerializer.LoadObjectFromFile(step.ToolBlockPath) as CogToolBlock;
                                if (tb != null)
                                {
                                    _stepToolBlocks[step.StepNumber] = tb;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"步骤 {step.StepNumber}: 许可证找不到Failed to load ToolBlock: {ex.Message}");
                            }
                        }
                    }

                }

                UpdateStatus($"已加载模板: {_currentTemplate?.TemplateName}");

                // 执行各个作业步骤
                if (_currentTemplate != null)
                {
                    List<WorkStep> batch = new List<WorkStep>();
                    await Task.Run(async () => {

                        foreach (var step in _currentTemplate.WorkSteps)
                        {
                            batch.Add(step);
                            if (step.IsArMode)
                            {
                                await ExecuteArLoopAsync(step); // 只要是循环检测，那么就是逐步执行

                                batch.Clear();
                            }
                        }

                        if (batch.Count > 0) // 单次检测
                        {
                            if (batch == null || batch.Count == 0) return;
                           await ExecuteStandardCheckAsync(batch[0]);// 暂时设定最后一步为最终量测
                            // 动态计算批处理超时时间
                            //int maxTimeout = batch.Max(s => s.Timeout > 0 ? s.Timeout : 5000);
                            //int batchTimeout = maxTimeout + 5000; // 加5秒缓冲
                            //var tasks = batch.Select(step => ExecuteStandardCheckAsync(step)).ToList();
                            //var whenAllTask = Task.WhenAll(tasks);
                            //var timeoutTask = Task.Delay(batchTimeout);
                            //var completedTask = await Task.WhenAny(whenAllTask, timeoutTask);
                        }

                        //var data = GetData();
                        //// 并行处理两部分数据
                        //var t1 = Task.Run(() => ProcessPart1(data));
                        //var t2 = Task.Run(() => ProcessPart2(data));

                        //await Task.WhenAll(t1, t2);
                    });


                }

                IsArModeRunning = false;
                // 保存数据
                ChangeState(WorkflowState.SavingData);
                SaveImageAndVideo();
                UpdateStatus("数据已保存");

                // 上传MES
                ChangeState(WorkflowState.UploadingMES);
                UploadToMES();
                UpdateStatus("已上传MES");

                // 完成
                ChangeState(WorkflowState.Idle);
                UpdateStatus("作业完成");

                // 检查所有步骤状态，确认最终结果
                bool allPassed = _currentTemplate.WorkSteps.All(s => s.Status == "检测通过");
                OverallResultChanged?.Invoke("PASS");

                // 2. 停止录制视频
                _videoRecordingService.StopRecording();
                RecordingStatusChanged?.Invoke("视频录制完成", Color.Green);

                _logger.Info($"【作业流程完成】SN: {productSN}, 总帧数: {_videoRecordingService.FrameCount}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"作业流程异常: SN={productSN}");
                UpdateStatus($"错误: {ex.Message}");
                await ChangeState(WorkflowState.Idle);

                // 流程异常，设置总体结果为 NG
                OverallResultChanged?.Invoke("FAIL");

                // 异常时也要停止录制
                _videoRecordingService.StopRecording();
            }
        }

        /// <summary>
        /// 批量执行作业步骤
        /// </summary>
        private async Task ExecuteStepBatch(List<WorkStep> steps)
        {
            if (steps == null || steps.Count == 0) return;

            // 动态计算批处理超时时间
            int maxTimeout = steps.Max(s => s.Timeout > 0 ? s.Timeout : 5000);
            int batchTimeout = maxTimeout + 5000; // 加5秒缓冲

            var tasks = steps.Select(step => ExecuteWorkStep(step)).ToList();
            var whenAllTask = Task.WhenAll(tasks);
            var timeoutTask = Task.Delay(batchTimeout);

            var completedTask = await Task.WhenAny(whenAllTask, timeoutTask);

            //if (completedTask == timeoutTask)
            //{
            //    _logger.Error($"严重警告: 步骤批量执行超时（{batchTimeout}ms）...");

            //    // 强制标记未完成的步骤为失败
            //    foreach (var step in steps)
            //    {
            //        // 只要不是 "检测通过"，统统视为超时失败
            //        if (step.Status != "检测通过" && step.Status != "检测成功")
            //        {
            //            step.Status = "检测失败";
            //            step.FailureReason = "流程强制超时（任务卡死或检测超时）";
            //            // 强制触发UI更新
            //            UpdateStepStatus(step, "检测失败");
            //            OnStepCompleted?.Invoke(step);
            //        }
            //    }
            //}
            //else
            //{
            //    await whenAllTask;
            //}
        }

        private async Task ExecuteWorkStep(WorkStep step)
        {
            try
            {
                UpdateStepStatus(step, "准备执行...");

                if (step.IsArMode)
                {
                    // 独立的 AR 任务，await 等待它完成
                    await ExecuteArLoopAsync(step);
                    _cameraService._capture = true;
                }
                else
                {
                    // 独立的标准任务，需要双相机并行执行
                    await ExecuteStandardCheckAsync(step);
                }
            }
            catch (Exception ex)
            {
                // 统一异常处理
            }
        }

        /// <summary>
        /// 用Yolo进行过程检测的逻辑
        /// </summary>
        /// <param name="step">作业步骤</param>
        /// <returns></returns>
        private async Task ExecuteArLoopAsync(WorkStep step)
        {
            UpdateStatus($"步骤{step.StepNumber}: 进入AR装配模式...");

            Stopwatch passTimer = new Stopwatch();
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            IsArModeRunning = true;
            int loopCount = 0;
            int failureCount = 0;
            const int MAX_CONSECUTIVE_FAILURES = 10;

            try
            {
                //var camera = _cameraService.GetCamera(step.CameraIndex); // 双相机模式
                var camera = _cameraService; // 单相机模式
                if (camera == null)
                {
                    _logger.Error($"步骤{step.StepNumber}: 无法获取相机索引 {step.CameraIndex}");
                    step.Status = "检测失败";
                    step.FailureReason = "相机未初始化";
                    UpdateStepStatus(step, "检测失败");
                    OnStepCompleted?.Invoke(step);
                    return;
                }

                // 确保相机已启动
                if (!camera.IsRunning)
                {
                    _logger.Warn($"步骤{step.StepNumber}: 相机未启动，尝试启动...");
                    camera.StartCapture();
                    await Task.Delay(500); // 等待相机稳定
                }

                //_cameraService._capture = false; // 独立的线程采集关闭

                await Task.Run(async() =>
                {
                    while (true)
                    {
                        loopCount++;
                        ICogImage liveImage = null;

                        try
                        {
                            // 1. 异步图像采集
                            liveImage = camera.CaptureSnapshotAsync();

                            if (liveImage == null)
                            {
                                await Task.Delay(100); // 失败时等待
                                continue;
                            }

                            // 2. 应用标定（如果已标定）
                            if (_calibrationService != null && _calibrationService.IsCalibrated(step.CameraIndex))
                            {
                                liveImage = _calibrationService.ApplyCalibration(liveImage, step.CameraIndex);
                            }

                            (bool Passed, string Reason, Dictionary<string, double> Results, ICogRecord Record, List<YoloOBBPrediction> Predictions) result;
                            var toolBlock = _stepToolBlocks.ContainsKey(step.StepNumber) ? _stepToolBlocks[step.StepNumber] : null;
                            result = await RunHybridLogic(liveImage, step, toolBlock);
                            bool isInROI = result.Results.ContainsKey("IsInROI") && result.Results["IsInROI"] > 0;
                            PointF? centerPoint = null;
                            if (result.Results.ContainsKey("CenterX") && result.Results.ContainsKey("CenterY"))
                            {
                                centerPoint = new PointF((float)result.Results["CenterX"], (float)result.Results["CenterY"]);
                            }

                            InOnYoloDetection?.Invoke(this, new InspectionResultEventArgs
                            {
                                Image = liveImage,
                                Record = result.Record,
                                Results = result.Results,
                                IsPassed = result.Passed,
                                Step = step,
                                Predictions = result.Predictions,
                                IsInROI = isInROI,
                                CenterPoint = centerPoint
                            });

                            if (result.Passed)
                            {
                                if (!passTimer.IsRunning)
                                {
                                    passTimer.Start();
                                }

                                step.Status = "检测通过";
                                UpdateStepStatus(step, "检测通过");

                                OnStepCompleted?.Invoke(step);
                                break;
                            }
                        }

                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"步骤{step.StepNumber}: AR循环异常 (第{loopCount}次)");
                            failureCount++;

                            if (failureCount >= MAX_CONSECUTIVE_FAILURES)
                            {
                                step.Status = "检测失败";
                                step.FailureReason = $"AR检测异常: {ex.Message}";
                                UpdateStepStatus(step, "检测失败");
                                OnStepCompleted?.Invoke(step);
                                break;
                            }
                        }

                         await Task.Delay(70);                                             }
                    });
                
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"步骤{step.StepNumber}: ExecuteArLoopAsync 异常");
                step.Status = "检测失败";
                step.FailureReason = $"AR模式异常: {ex.Message}";
                UpdateStepStatus(step, "检测失败");
                OnStepCompleted?.Invoke(step);
            }
        }

        /// <summary>
        /// 初始化相机锁
        /// </summary>
        private void EnsureCameraLockInitialized(int cameraIndex)
        {
            lock (_lockInitLock)
            {
                if (!_cameraLocks.ContainsKey(cameraIndex))
                {
                    _cameraLocks[cameraIndex] = new SemaphoreSlim(1, 1);
                    _logger.Info($"相机{cameraIndex}锁已初始化");
                }
            }
        }

        /// <summary>
        /// 执行VisionPro的量测逻辑
        /// </summary>
        private async Task ExecuteStandardCheckAsync(WorkStep step)
        {
            // 确保相机锁已初始化
            EnsureCameraLockInitialized(step.CameraIndex);

            StabilityState stabilityState = new StabilityState();
            ICogImage lastCapturedImage = null; 
            
            try
            {
                // 设置为检测中状态（黄色）
                _logger.Info($"执行了吗？ExecuteWorkStep");
                UpdateStepStatus(step, "检测中...");
                UpdateStatus($"开始执行步骤{step.StepNumber} (相机{step.CameraIndex + 1})");

                // 使用 Stopwatch 配合 step.Timeout 控制循环退出
                Stopwatch timeoutWatch = Stopwatch.StartNew();
                int timeoutMs = step.Timeout > 0 ? step.Timeout : 5000; // 默认5秒
                _logger.Info($"步骤{step.StepNumber} 超时设置: {timeoutMs}ms");

                // 持续采集图像，直到图像稳定 AND 匹配分数都满足
                ICogImage validImage = null;
                stepPassed = false;
                int totalCheckCount = 0;
                const int CHECK_DELAY = 20; // 每次检测间隔（毫秒）

                while (true)
                {
                    totalCheckCount++;

                    // 使用相机锁防止并发冲突
                    ICogImage currentImage = null;
                    await _cameraLocks[step.CameraIndex].WaitAsync();
                    try
                    {
                        // 步骤1：采集图像
                        await ChangeState(WorkflowState.AddingOumitImage);
                        currentImage = await CaptureOumitImage(step.CameraIndex);
                    }
                    finally
                    {
                        _cameraLocks[step.CameraIndex].Release();
                    }
                    
                    if (currentImage == null)
                    {
                        UpdateStatus($"步骤{step.StepNumber}: 采集图像失败，重试...");
                        await Task.Delay(CHECK_DELAY);
                        continue;
                    }

                    // 确保旧图像释放，防止内存泄漏
                    if (lastCapturedImage != null && lastCapturedImage != currentImage)
                    {
                        (lastCapturedImage as IDisposable)?.Dispose();
                    }
                    // 更新最后一张图像引用
                    lastCapturedImage = currentImage;


                    // 稳定性检测改为可选
                    if (step.RequireStability && _cameraService != null && _cameraService.IsConnected)
                    {
                        // 步骤2：检查图像稳定性
                        await ChangeState(WorkflowState.CheckingDefect);
                        var hasDefect = await CheckImageDefect(currentImage, step, stabilityState);

                        _logger.Info($"检查图像稳定性？hasDefect，{hasDefect}");
                        if (hasDefect)
                        {
                            // 图像不稳定，继续下一次循环
                            if (totalCheckCount % 10 == 0)
                            {
                                UpdateStatus($"步骤{step.StepNumber}: 等待图像稳定... (已用{timeoutWatch.ElapsedMilliseconds}ms/{timeoutMs}ms)");
                            }
                            await Task.Delay(CHECK_DELAY);
                            continue;
                        }
                    }
                                      
                    UpdateStatus($"步骤{step.StepNumber}: 图像稳定，视觉检测中...");
                    await ChangeState(WorkflowState.CheckingScore);

                    // 执行视觉检测（包含参数公差比对）
                    _logger.Info($"执行了吗？执行视觉检测（包含参数公差比对）");
                    
                    (bool Passed, string Reason, Dictionary<string, double> Results, ICogRecord Record, List<YoloOBBPrediction> Predictions) inspection;

                    if (step.AlgorithmType == 1)
                    {
                         var toolBlock = _stepToolBlocks.ContainsKey(step.StepNumber) ? _stepToolBlocks[step.StepNumber] : null;
                         inspection = await RunHybridLogic(currentImage, step, toolBlock);
                    }
                    else
                    {
                         inspection = await RunVisionInspection(currentImage, _stepToolBlocks.ContainsKey(step.StepNumber) ? _stepToolBlocks[step.StepNumber] : null, step);
                    }

                    UpdateStatus($"步骤{step.StepNumber}: {inspection.Reason}");
                    
                    // 触发结果事件，用于UI显示
                    InspectionResultReady?.Invoke(this, new InspectionResultEventArgs 
                    { 
                        Image = currentImage,
                        Record = inspection.Record,
                        Results = inspection.Results,
                        IsPassed = inspection.Passed,
                        Step = step,
                        Predictions = inspection.Predictions
                    });

                    // 步骤4：判断匹配结果
                    if (inspection.Passed)
                    {
                        stepPassed = true;

                        // 构建保存路径（使用固定的 D 盘路径）
                        string baseFolder = @"D:\data\ZIPImg";
                        string snFolder = Path.Combine(baseFolder, Params.Instance.SN);
                        Directory.CreateDirectory(snFolder);

                        Params.Instance.LocationPicPath = snFolder;
                        
                        // 保存原始 BMP（用于备份）
                        string originalBmpPath = Path.Combine(snFolder, $"Step{step.StepNumber}_original.bmp");
                        SaveCogImageToBmp(currentImage, originalBmpPath);
                        
                        // 保存压缩后的 JPEG（用于 MES 上传）
                        string compressedJpgPath = Path.Combine(snFolder, $"Step{step.StepNumber}_compressed.jpg");
                        SaveCompressedImage(currentImage, compressedJpgPath, 200);

                        _logger.Info($"步骤{step.StepNumber}: 图片已保存 - BMP: {originalBmpPath}, JPEG: {compressedJpgPath}");
                        validImage = currentImage;
                        
                        UpdateStepStatus(step, "检测通过");
                        await UpdateStepImage(step, validImage);
                        UpdateStatus($"步骤{step.StepNumber}: 检测通过");
                        
                        step.CompletedTime = DateTime.Now;
                        OnStepCompleted?.Invoke(step);
                        break;
                    }
                    else
                    {
                        // 图像稳定但检测失败，释放图像并重新采集
                        UpdateStatus($"步骤{step.StepNumber}: 检测失败 - {inspection.Reason}，重新采集...");                        
                        // 重置稳定性检测状态，准备下一次采集
                        ResetStabilityCheck(stabilityState);
                        await Task.Delay(CHECK_DELAY);
                    }
                }
                
                // 检查是否超时
                //if (!stepPassed)
                //{
                //    UpdateStepStatus(step, "检测失败");
                //    UpdateStatus($"步骤{step.StepNumber}: 检测超时，已用时{timeoutWatch.ElapsedMilliseconds}ms，尝试{totalCheckCount}次");
                    
                //    // 步骤失败，设置总体结果为 NG
                //    OverallResultChanged?.Invoke("FAIL");
                    
                //    step.FailureReason = $"检测超时: 已用时{timeoutWatch.ElapsedMilliseconds}ms，尝试{totalCheckCount}次";
                //    step.Status = "检测失败";
                    
                //    if (lastCapturedImage != null)
                //    {
                //        await UpdateStepImage(step, lastCapturedImage);
                //    }

                //    // 触发步骤完成事件（失败状态），让UI更新颜色为红色
                //    OnStepCompleted?.Invoke(step);
                    
                //    // 根据配置决定是否提示用户
                //    if (step.ShowFailurePrompt)
                //    {
                //        string promptMessage = string.IsNullOrWhiteSpace(step.FailurePromptMessage) 
                //            ? $"步骤{step.StepNumber}检测失败！\n\n{step.FailureReason}" 
                //            : step.FailurePromptMessage;
                //        // MessageBox.Show(promptMessage, "检测失败提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //        _logger.Warn($"步骤{step.StepNumber}用户提示: {promptMessage}");
                //    }
                    
                //    ResetStabilityCheck(stabilityState);
                //}
            }
            catch (Exception ex)
            {
                UpdateStepStatus(step, "检测失败");
                UpdateStatus($"步骤{step.StepNumber}: 执行异常 - {ex.Message}");
                _logger.Error(ex, $"步骤{step.StepNumber}: 执行异常");
                
                OverallResultChanged?.Invoke("FAIL");
                
                step.FailureReason = $"执行异常: {ex.Message}";
                step.Status = "检测失败";
                
                if (lastCapturedImage != null)
                {
                    await UpdateStepImage(step, lastCapturedImage);
                }

                // 触发步骤完成事件（失败状态），让UI更新颜色为红色
                OnStepCompleted?.Invoke(step);
                
                // 根据配置决定是否提示用户
                if (step.ShowFailurePrompt)
                {
                    string promptMessage = string.IsNullOrWhiteSpace(step.FailurePromptMessage) 
                        ? $"步骤{step.StepNumber}检测失败！\n\n{step.FailureReason}" 
                        : step.FailurePromptMessage;
                    
                    // MessageBox.Show(promptMessage, "检测失败提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _logger.Warn($"步骤{step.StepNumber}用户提示: {promptMessage}");
                }
            }
        }
        
        private void UpdateStepStatus(WorkStep step, string status)
        {
            step.Status = status;
        }
        
        private async Task UpdateStepImage(WorkStep step, ICogImage image)
        {
            try
            {
                await Task.Run(() =>
                {
                    step.CapturedImage = image;
                    var bitmap = ConvertICogImageToBitmap(image);
                    if (bitmap != null) step.ImageSource = bitmap;
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "更新步骤图像失败");
            }
        }

        /// <summary>
        /// 将 ICogImage 转换为 Bitmap
        /// </summary>
        private Bitmap ConvertICogImageToBitmap(ICogImage ICogImage)
        {
            if (ICogImage == null) return null;
            try
            {
                using (var bmp = ICogImage.ToBitmap())
                {
                    // 克隆一个新的 Bitmap，确保与原始 ICogImage 解耦
                    return new Bitmap(bmp);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"图像转换失败: {ex.Message}");
                return null;
            }
        }

        public async Task StopWorkflow()
        {
            stepPassed = true;
            UpdateStatus($"已终止流程");
            await ChangeState(WorkflowState.Idle);

            // 异常时也要停止录制
            _videoRecordingService.StopRecording();
        }

        private async Task<ICogImage> CaptureOumitImage(int cameraIndex = 0)
        {
            // 统一使用封装的采集方法，支持离线回退
            return await Task.Run(() => CaptureFromCameraOrOffline(cameraIndex));
        }

        private ICogImage CaptureFromCameraOrOffline(int cameraIndex = 0)
        {
            // 1. 检查相机连接状态，如果未连接则直接进入离线模式
            if (_cameraService == null || !_cameraService.IsConnected)
            {
                // 相机未连接，直接从离线文件夹读取
                return LoadOfflineImage();
            }

            // 2. 相机已连接，尝试从相机采集
            try
            {
                if (_cameraService != null)
                {
                    ICogImage image = null;
                    if (_cameraService.IsMultiCameraMode)
                    {
                        var camera = _cameraService.GetCamera(cameraIndex);
                        if (camera != null)
                        {
                            image = camera.CaptureSnapshotAsync();
                        }
                        else
                        {
                            _logger.Warn($"无效的相机索引: {cameraIndex}");
                        }
                    }
                    else
                    {
                        image = _cameraService.CaptureSnapshotAsync();
                    }

                    if (image != null)
                    {
                        // 应用标定（如果已标定）
                        if (_calibrationService != null && _calibrationService.IsCalibrated(cameraIndex))
                        {
                            image = _calibrationService.ApplyCalibration(image, cameraIndex);
                        }
                        return image;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"相机采集失败: {ex.Message}，回退到离线模式");
            }

            // 3. 相机采集失败，回退到离线文件夹读取
            return LoadOfflineImage();
        }

        /// <summary>
        /// 从离线文件夹加载图片
        /// </summary>
        private ICogImage LoadOfflineImage()
        {
            try
            {
                // 优先使用用户指定的绝对路径，如果不存在则尝试项目目录下的 TEST
                string offlinePath = @"d:\Cowain\1122刘金帅\audio900\TEST";
                if (!Directory.Exists(offlinePath))
                {
                    offlinePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TEST");
                }

                if (Directory.Exists(offlinePath))
                {
                    var files = Directory.GetFiles(offlinePath, "*.*")
                        .Where(f => f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) || 
                                    f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f)
                        .ToArray();

                    if (files.Length > 0)
                    {
                        // 循环读取
                        string imageFile = files[_offlineImageIndex % files.Length];
                        
                        CogImageFileTool fileTool = new CogImageFileTool();
                        fileTool.Operator.Open(imageFile, CogImageFileModeConstants.Read);
                        fileTool.Run();
                        ICogImage offlineImage = fileTool.OutputImage;
                        
                        // 指向下一张
                        _offlineImageIndex++;
                        
                        return offlineImage;
                    }
                    else
                    {
                        _logger.Warn($"离线文件夹 {offlinePath} 中未找到图片文件");
                    }
                }
                else
                {
                    _logger.Warn($"离线文件夹不存在: {offlinePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"离线加载图片失败: {ex.Message}");
            }

            return null;
        }

        private async Task<bool> CheckImageDefect(object image, WorkStep step, StabilityState state)
        {
            try
            {
                ICogImage currentFrame = image as ICogImage; // 使用传入的图像作为当前帧

                if (currentFrame == null)
                {
                    UpdateStatus("图像为空");
                    return true;
                }

                ICogImage currentGray = null;
                if (currentFrame is CogImage8Grey)
                {
                    currentGray = currentFrame;
                }
                else
                {
                    try 
                    {
                        CogImageConvertTool convertTool = new CogImageConvertTool();
                        convertTool.InputImage = currentFrame;
                        convertTool.Run();
                        currentGray = convertTool.OutputImage as CogImage8Grey;
                    }
                    catch
                    {
                        currentGray = currentFrame; 
                    }
                }
                
                if (state.PreviousGrayImage == null)
                {
                    state.PreviousGrayImage = currentGray;
                    state.StableTimer = Stopwatch.StartNew();
                    state.IsStabilityChecking = true;
                    step.Status = "等待图像稳定...";
                    return true; // 继续检测
                }

                // 计算当前帧与上一帧的平均灰度差
                double avgDiff = CalculateAverageGrayDifference(state.PreviousGrayImage, currentGray);
                
                // 更新状态显示
                step.Status = $"灰度差值: {avgDiff:F2} (已稳定 {state.StableTimer.ElapsedMilliseconds}ms)";
                _logger.Info($"图像不稳定吗？avgDiff，{avgDiff}");
                // 判断灰度差是否小于阈值
                if (avgDiff < GRAY_DIFF_THRESHOLD)
                {
                    // 图像稳定，检查持续时间
                    _logger.Info($"检查持续时间？state.StableTimer.ElapsedMilliseconds，{state.StableTimer.ElapsedMilliseconds} ");
                    if (state.StableTimer.ElapsedMilliseconds >= STABLE_DURATION_MS)
                    {
                        step.Status = "图像稳定，触发成功";
                        UpdateStatus($"稳定检测通过 (灰度差: {avgDiff:F2})");
                        _logger.Info($"图像无问题可以继续吗？");
                        return false; // 无问题，可以继续
                    }
                    else
                    {
                        state.PreviousGrayImage = currentGray; // 更新上一帧
                        return true; 
                    }
                }
                else
                {
                    step.Status = $"图像不稳定，重新计时 (灰度差: {avgDiff:F2})";
                    state.StableTimer.Restart();
                    state.PreviousGrayImage = currentGray;
                    return true; 
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"稳定性检测异常: {ex.Message}");
                ResetStabilityCheck(state);
                return true; 
            }
        }

        /// <summary>
        /// 计算两幅灰度图像的平均灰度差
        /// </summary>
        private double CalculateAverageGrayDifference(ICogImage grayImage1, ICogImage grayImage2)
        {
            try
            {
                if (grayImage1 == null || grayImage2 == null) return 0.0;

                CogIPTwoImageSubtract subtractOp = new CogIPTwoImageSubtract();
                subtractOp.OverflowMode = CogIPTwoImageSubtractOverflowModeConstants.Absolute;
                ICogImage diffImage = subtractOp.Execute(grayImage1, grayImage2, null,null);

                CogHistogramTool histTool = new CogHistogramTool();
                histTool.InputImage = diffImage as CogImage8Grey;
                if (histTool.InputImage == null) return 0.0;

                histTool.Run();

                if (histTool.Result != null)
                {
                    return histTool.Result.Mean;
                }

                return 0.0;
            }
            catch (Exception ex)
            {
                _logger.Error($"灰度差计算异常: {ex.Message}");
                return 999.0; 
            }
        }

        /// <summary>
        /// 重置稳定性检测状态
        /// </summary>
        private void ResetStabilityCheck(StabilityState state)
        {
            if (state != null)
            {
                state.PreviousGrayImage = null;
                state.StableTimer?.Stop();
                state.StableTimer = null;
                state.IsStabilityChecking = false;
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            throw new ArgumentException("No appropriate encoder found.", nameof(format));
        }

        public static Image ZipImage(Image img, ImageFormat format, long targetLen, long srcLen = 0)
        {
            const long nearlyLen = 10240;
            var ms = new MemoryStream();
            if (0 == srcLen)
            {
                img.Save(ms, format);
                srcLen = ms.Length;
            }

            targetLen *= 1024;
            if (targetLen > srcLen)
            {
                ms.SetLength(0);
                ms.Position = 0;
                img.Save(ms, format);
                img = Image.FromStream(ms);
                return img;
            }

            var exitLen = targetLen - nearlyLen;

            var quality = (long)Math.Floor(100.00 * targetLen / srcLen);
            var parms = new EncoderParameters(1);
            ImageCodecInfo formatInfo = GetEncoder(ImageFormat.Jpeg);

            long startQuality = quality;
            long endQuality = 100;
            quality = (startQuality + endQuality) / 2;

            while (true)
            {
                parms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                ms.SetLength(0);
                ms.Position = 0;
                img.Save(ms, formatInfo, parms);

                if (ms.Length >= exitLen && ms.Length <= targetLen)
                {
                    break;
                }
                else if (startQuality >= endQuality) 
                {
                    break;
                }
                else if (ms.Length < exitLen)
                {
                    startQuality = quality;
                }
                else 
                {
                    endQuality = quality;
                }

                var newQuality = (startQuality + endQuality) / 2;
                if (newQuality == quality)
                {
                    break;
                }
                quality = newQuality;
            }
            img = Image.FromStream(ms);
            return img;
        }

        private void SaveCogImageToBmp(ICogImage image, string path)
        {
             using(var bmp = image.ToBitmap())
             {
                 bmp.Save(path, ImageFormat.Bmp);
             }
        }

        private void SaveCompressedImage(ICogImage image, string outputPath, int maxSizeKB = 200)
        {
            string tempBmpPath = null;
            try
            {
                tempBmpPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bmp");
                SaveCogImageToBmp(image, tempBmpPath);
                
                using (var originalBitmap = new Bitmap(tempBmpPath))
                {
                    long srcLen;
                    using (var ms = new MemoryStream())
                    {
                        originalBitmap.Save(ms, ImageFormat.Bmp);
                        srcLen = ms.Length;
                    }
                    
                    Image compressedImage = ZipImage(originalBitmap, ImageFormat.Jpeg, maxSizeKB, srcLen);
                    
                    compressedImage.Save(outputPath, ImageFormat.Jpeg);
                    
                    compressedImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"图片压缩失败: {ex.Message}");
                try
                {
                    SaveCogImageToBmp(image, outputPath);
                }
                catch { }
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempBmpPath) && File.Exists(tempBmpPath))
                        File.Delete(tempBmpPath);
                }
                catch { }
            }
        }

        /// <summary>
        /// 执行深度学习检测逻辑
        /// </summary>
        private async Task<(bool Passed, string Reason, Dictionary<string, double> Results, ICogRecord Record, List<YoloOBBPrediction> Predictions)> RunDeepLearningLogic(ICogImage image, WorkStep step)
        {
            var results = new Dictionary<string, double>();
            ICogRecord record = null;
            string reason = "";
            bool passed = false;
            List<YoloOBBPrediction> predictions = new List<YoloOBBPrediction>();
            int stepClassID = step.StepNumber - 1; // 步骤顺序和识别序号一一对应
            int predictClassID = -1;
            try
            {
                // 使用全局模型
                if (_globalYoloService == null)
                {
                    return (false, "深度学习模型未加载", results, null, predictions);
                }

                var dlService = _globalYoloService;

                // 转换图像
                using (var bitmap = image.ToBitmap())
                {
                    // 运行推理
                    predictions = await Task.Run(() => dlService.Predict(bitmap, (float)step.ConfidenceThreshold));

                    // 如果启用了过程检测模式
                    if (step.EnableProcessDetection && step.DetectionROI.Width > 0 && step.DetectionROI.Height > 0)
                    {
                        // 过滤目标类别（如果有设置）
                        var filteredPredictions = predictions;
                        if (step.TargetClassIds != null && step.TargetClassIds.Count > 0)
                        {
                            filteredPredictions = predictions.Where(p => step.TargetClassIds.Contains(p.ClassId)).ToList();
                        }

                        // 判断是否有目标的中心点在 ROI 内
                        bool isInROI = false;
                        YoloOBBPrediction targetInROI = null;
                        PointF centerPoint = PointF.Empty;

                        foreach (var pred in filteredPredictions)
                        {
                            predictClassID = pred.ClassId;
                            // 计算中心点（使用旋转框的4个角点计算中心）
                            float centerX = pred.RotatedBox.Average(p => p.X);
                            float centerY = pred.RotatedBox.Average(p => p.Y);
                            
                            // 判断中心点是否在 ROI 内（支持旋转矩形）
                            if (IsPointInRotatedROI(centerX, centerY, step.DetectionROI, step.DetectionROIRotation))
                            {
                                isInROI = true;
                                targetInROI = pred;
                                centerPoint = new PointF(centerX, centerY);
                                break;
                            }
                        }

                        // 判定条件：中心点在 ROI 内并且当前步骤顺序和识别的顺序是一一对应
                        if (isInROI && stepClassID == predictClassID)
                        {
                            passed = true;
                            reason = $"动作完成：{targetInROI.Label} 进入检测区域";
                            results["Score"] = targetInROI.Confidence;
                            results["CenterX"] = centerPoint.X;
                            results["CenterY"] = centerPoint.Y;
                            results["IsInROI"] = 1;
                            _logger.Info($"步骤{step.StepNumber}: 过程检测通过，中心点 ({centerPoint.X:F0}, {centerPoint.Y:F0}) 在 ROI 内");
                        }
                        else
                        {
                            passed = false;
                            if (filteredPredictions.Count > 0)
                            {
                                reason = "等待动作：物体未进入检测区域";
                                var best = filteredPredictions.OrderByDescending(p => p.Confidence).First();
                                results["Score"] = best.Confidence;
                            }
                            else
                            {
                                reason = "等待动作：未检测到目标物";
                                results["Score"] = 0;
                            }
                            results["IsInROI"] = 0;
                        }
                    }
                    else
                    {
                        // 原有的简单判定逻辑：识别到目标即为通过
                        passed = predictions.Count > 0;
                        
                        if (passed)
                        {
                            reason = $"识别成功: {predictions.Count}个目标";
                            // 取置信度最高的目标作为分数输出
                            var best = predictions.OrderByDescending(p => p.Confidence).First();
                            results["Score"] = best.Confidence;
                            results["ClassId"] = best.ClassId;
                            // 使用旋转框中心点
                            results["X"] = best.RotatedBox.Average(p => p.X);
                            results["Y"] = best.RotatedBox.Average(p => p.Y);
                            results["Angle"] = best.Angle;
                        }
                        else
                        {
                            reason = "未识别到目标";
                            results["Score"] = 0;
                        }
                    }

                    // 创建简单的图像记录
                    record = new CogRecord("Image", typeof(ICogImage), CogRecordUsageConstants.Result, false, image, "Image");
                }
            }
            catch (Exception ex)
            {
                reason = $"DL检测异常: {ex.Message}";
                _logger.Error(ex, $"步骤{step.StepNumber} DL异常");
            }

            return (passed, reason, results, record, predictions);
        }

        /// <summary>
        /// 执行过程检测逻辑：只用YOLOv8进行AR 显示
        /// </summary>
        private async Task<(bool Passed, string Reason, Dictionary<string, double> Results, ICogRecord Record, List<YoloOBBPrediction> Predictions)> RunHybridLogic(ICogImage image, WorkStep step, CogToolBlock toolBlock)
        {
            // 跑深度学习  - 主要是为了获取 AR 效果
            var yoloResult = await RunDeepLearningLogic(image, step);
            
            // 默认使用 YOLO 的 Record 
            var finalRecord = yoloResult.Record;
            
            if (yoloResult.Passed)
            {  
                return (yoloResult.Passed, "YOLO通过", yoloResult.Results, finalRecord, yoloResult.Predictions);             
            }
            
            // YOLO 没识别到，直接返回 YOLO 的失败结果
            return yoloResult;
        }

        /// <summary>
        /// 执行视觉检测：运行工具块并校验参数公差
        /// </summary>
        private async Task<(bool Passed, string Reason, Dictionary<string, double> Results, ICogRecord Record, List<YoloOBBPrediction> Predictions)> RunVisionInspection(ICogImage image, CogToolBlock cogToolBlock, WorkStep step)
        {
            
            var results = new Dictionary<string, double>();
            ICogRecord record = null;
            
            if (cogToolBlock == null)
            {
                return (false, "ToolBlock未加载", results, null, new List<YoloOBBPrediction>());
            }

            CogToolBlock toolBlock = cogToolBlock;

            async void FireDebug(string message)
            {
                if (!EnableDebugPopup) return;
                try
                {
                    if (record == null && toolBlock != null)
                    {
                        record = toolBlock.CreateLastRunRecord();
                    }

                    ToolBlockDebugReady?.Invoke(this, new ToolBlockDebugEventArgs
                    {
                        StepNumber = step.StepNumber,
                        Step = step,
                        ToolBlock = toolBlock,
                        Record = record,
                        Message = message
                    });
                }
                catch
                {
                }
            }

            try
            {
                lock (toolBlock)
                {
                    if (toolBlock.Inputs.Contains("InputImage"))
                    {
                        toolBlock.Inputs["InputImage"].Value = image;
                    }
                        
                    toolBlock.Run();

                    // 创建运行记录
                    record = toolBlock.CreateLastRunRecord();

                    _logger.Info($"已运行toolBlock，{record.RecordKey},{record.SubRecords.Count}");
                    // 调试模式：无论成功失败都弹出调试窗口
                    if (EnableDebugPopup)
                    {
                        string statusMsg = toolBlock.RunStatus.Result == CogToolResultConstants.Accept 
                            ? "运行成功" 
                            : toolBlock.RunStatus.Message;
                            FireDebug($"[{toolBlock.RunStatus.Result}] {statusMsg}");
                    }
                        
                    // 获取输出参数
                    foreach(CogToolBlockTerminal terminal in toolBlock.Outputs)
                    {
                        if (terminal.Value == null) continue;

                        // 1. 尝试直接作为 double 解析
                        if (double.TryParse(terminal.Value.ToString(), out double val))
                        {
                            results[terminal.Name] = val;
                        }                
                    }

                    if (record == null)
                    {
                        record = toolBlock.CreateLastRunRecord();
                    }
                        
                    // 2. 校验参数公差
                    if (step.Parameters != null && step.Parameters.Count > 0)
                    {
                        foreach(var param in step.Parameters)
                        {
                            if (!param.IsEnabled) continue;
                                
                            if (!results.ContainsKey(param.Name))
                            {
                                var reason = $"缺少输出参数: {param.Name}";
                                _logger.Info(reason);
                                FireDebug(reason);
                                return (false, "视觉检测异常", results, record, new List<YoloOBBPrediction>());
                            }
                                
                            double actualVal = results[param.Name];
                            double diff = Math.Abs(actualVal - param.StandardValue);
                                
                            if (diff > param.Tolerance)
                            {
                                results["ToleranceDiff"] = diff;
                                var reason = $"参数[{param.Name}]超差: 实际{actualVal:F3} 标准{param.StandardValue:F3} 偏差{diff:F3} > 公差{param.Tolerance}";
                                _logger.Info(reason);
                                FireDebug(reason);
                                return (false, "视觉检测异常", results, record, new List<YoloOBBPrediction>());
                            }
                        }
                        return (true, "参数检测通过", results, record, new List<YoloOBBPrediction>());
                    }
                    else
                    {                            
                        // 既无参数也无Score，仅判断工具运行状态
                        if (toolBlock.RunStatus.Result == CogToolResultConstants.Accept)
                            return (true, "工具运行成功(无参数检查)", results, record, new List<YoloOBBPrediction>());
                        else
                        {
                            var reason = $"工具运行失败: {toolBlock.RunStatus.Message}";
                            _logger.Info(reason);
                            FireDebug(reason);
                            return (false, "视觉检测异常", results, record, new List<YoloOBBPrediction>());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"VisionPro 运行异常: {ex.Message}");
                FireDebug($"视觉异常: {ex.Message}");
                if (EnableDebugPopup && toolBlock != null)
                {
                    try
                    {
                        if (record == null)
                        {
                            record = toolBlock.CreateLastRunRecord();
                        }

                        ToolBlockDebugReady?.Invoke(this, new ToolBlockDebugEventArgs
                        {
                            StepNumber = step.StepNumber,
                            Step = step,
                            ToolBlock = toolBlock,
                            Record = record,
                            Message = ex.Message
                        });
                    }
                    catch
                    {
                    }
                }
                return (false, "视觉检测异常", results, null, new List<YoloOBBPrediction>());
            }         
        }

        string ZIPName = string.Empty;
        private async Task SaveImageAndVideo()
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyyyMMdd");
                string currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                
                string zipBaseFolder = @"D:\data\ZIPImg";
                string zipDateFolder = Path.Combine(zipBaseFolder, currentDate);
                Directory.CreateDirectory(zipDateFolder);
                
                Params.Instance.LocationZIPPath = zipDateFolder;
                
                string[] stationParts = Params.Instance.MachineStation?.Split('_') ?? new[] { "Unknown", "Unknown", "Unknown" };
                ZIPName = $"{Params.Instance.SN}_{Params.Instance.ProjectCode}_{stationParts[0]}_{stationParts[1]}_{Params.Instance.station}_{stationParts[2]}_{currentTime}.zip";
                string zipFullPath = Path.Combine(zipDateFolder, ZIPName);
                
                string imageFolderPath = Params.Instance.LocationPicPath;
                string errorMsg = "";
                
                //_logger.Info($"开始压缩图片: {imageFolderPath} -> {zipFullPath}");
                
                // 需要确保 ZIPHelper 可用 
                /*
                string resultMsg = "";
                if (!myZIPHelper.ZIP(imageFolderPath, zipFullPath, ref resultMsg))
                {
                    _logger.Error($"压缩图片失败: {resultMsg}");
                    // MessageBox.Show($"压缩图片失败: {resultMsg}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _logger.Info($"压缩成功: {zipFullPath}");
                    // Params.Instance.privateParams.ImagePath = zipFullPath;
                }
                */
                
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logger.Error($"保存数据异常: {ex.Message}");
                throw;
            }
        }

        private async Task UploadToMES()
        {
            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string zipFilePath = getFiles_Path(Params.Instance.SN, date);
                
                if (string.IsNullOrEmpty(zipFilePath) || !File.Exists(zipFilePath))
                {
                    _logger.Error($"未找到压缩文件: SN={Params.Instance.SN}, Date={date}");
                    UpdateStatus("MES上传失败: 未找到压缩文件");
                    return;
                }
                
                _logger.Info($"开始上传MES: {zipFilePath}");
                
                string result = PostMes.GetMesInstance.PostMesPic(Params.Instance.SN, zipFilePath);
                
                if (result.Contains("true"))
                {
                    _logger.Info($"图片上传MES成功: SN={Params.Instance.SN}, 文件={zipFilePath}");
                    UpdateStatus("MES上传成功");
                }
                else
                {
                    _logger.Error($"图片上传MES失败: SN={Params.Instance.SN}, 结果={result}");
                    UpdateStatus($"MES上传失败: {result}");
                }
                
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logger.Error($"MES上传异常: {ex.Message}");
                UpdateStatus($"MES上传异常: {ex.Message}");
            }
        }

        public static string getFiles_Path(string sn, string date)
        {
            try
            {
                string dateFolder = date.Replace("-", ""); // 转换为 yyyyMMdd 格式
                string searchFolder = Path.Combine(@"D:\data\ZIPImg", dateFolder);
                
                if (!Directory.Exists(searchFolder))
                {
                    _logger.Warn($"搜索文件夹不存在: {searchFolder}");
                    return "";
                }
                
                DirectoryInfo dirInfo = new DirectoryInfo(searchFolder);
                FileInfo[] files = dirInfo.GetFiles("*.zip");
                
                var matchedFiles = files
                    .Where(f => f.Name.Contains(sn))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();
                
                if (matchedFiles.Count > 0)
                {
                    _logger.Info($"找到压缩文件: {matchedFiles[0].FullName}");
                    return matchedFiles[0].FullName;
                }
                
                _logger.Warn($"未找到包含 SN={sn} 的压缩文件");
                return "";
            }
            catch (Exception ex)
            {
                _logger.Error($"获取文件路径失败: {ex.Message}");
                return "";
            }
        }
    
        private async Task ChangeState(WorkflowState newState)
        {
            _currentState = newState;
            StateChanged?.Invoke(this, newState);
            await Task.Delay(100); 
        }

        public void UpdateStatus(string message)
        {
            StatusMessageChanged?.Invoke(this, message);
        }

        /// <summary>
        /// 判断点是否在旋转矩形 ROI 内
        /// </summary>
        private bool IsPointInRotatedROI(float pointX, float pointY, RectangleF roi, double rotationRadians)
        {
            // 如果没有旋转，直接使用简单判断
            if (Math.Abs(rotationRadians) < 0.001)
            {
                return roi.Contains(pointX, pointY);
            }

            // 计算 ROI 中心点
            double roiCenterX = roi.X + roi.Width / 2.0;
            double roiCenterY = roi.Y + roi.Height / 2.0;

            // 将点相对于 ROI 中心进行反向旋转，转换到 ROI 的局部坐标系
            double dx = pointX - roiCenterX;
            double dy = pointY - roiCenterY;

            // 反向旋转（旋转 -rotationRadians）
            double cos = Math.Cos(-rotationRadians);
            double sin = Math.Sin(-rotationRadians);

            double localX = dx * cos - dy * sin;
            double localY = dx * sin + dy * cos;

            // 在局部坐标系中判断是否在矩形内
            double halfWidth = roi.Width / 2.0;
            double halfHeight = roi.Height / 2.0;

            return Math.Abs(localX) <= halfWidth && Math.Abs(localY) <= halfHeight;
        }
    }

    /// <summary>
    /// 检测结果事件参数
    /// </summary>
    public class InspectionResultEventArgs : EventArgs
    {
        public ICogImage Image { get; set; }
        public ICogRecord Record { get; set; }
        public Dictionary<string, double> Results { get; set; }
        public bool IsPassed { get; set; }
        public WorkStep Step { get; set; }
        public List<YoloOBBPrediction> Predictions { get; set; }
        
        // 过程检测相关
        public bool IsInROI { get; set; }  // 中心点是否在 ROI 内
        public PointF? CenterPoint { get; set; }  // 检测物体的中心点
    }


    /// <summary>
    /// 调试模式使用
    /// </summary>
    public class ToolBlockDebugEventArgs : EventArgs
    {
        public int StepNumber { get; set; }
        public WorkStep Step { get; set; }
        public CogToolBlock ToolBlock { get; set; }
        public ICogRecord Record { get; set; }
        public string Message { get; set; }
    }
}
