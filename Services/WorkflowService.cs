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
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private WorkTemplate _currentTemplate;
        private VideoRecordingService _videoRecordingService;
        
        // 离线模式测试图片索引
        private int _offlineImageIndex = 0;

        // ToolBlock 缓存
        private Dictionary<int, CogToolBlock> _stepToolBlocks = new Dictionary<int, CogToolBlock>();

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

        public event EventHandler<WorkflowState> StateChanged;
        public event EventHandler<string> StatusMessageChanged;
        public event Action<WorkStep> OnStepCompleted;
        public event Action<string> OverallResultChanged;
        public event Action<string, Color> RecordingStatusChanged; // status, color
        
        // 新增：检测结果事件，携带图像和图形
        public event EventHandler<InspectionResultEventArgs> InspectionResultReady;

        public event EventHandler<ToolBlockDebugEventArgs> ToolBlockDebugReady;

        public bool EnableDebugPopup { get; set; }
        
        public WorkflowService(CameraService cameraService)
        {
            _cameraService = cameraService;
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
        public async Task StartWorkflow(WorkTemplate currentTemplate, string productSN, string employeeID)
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
                
                await ChangeState(WorkflowState.LoadingTemplate);
                
                if (_currentTemplate != null)
                {
                    string templatePath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "HomeworkTemplate",
                        _currentTemplate.TemplateName
                    );
                    
                    _stepToolBlocks.Clear();
                    
                    foreach (var step in _currentTemplate.WorkSteps)
                    {
                        string stepFolder = Path.Combine(templatePath, $"Step{step.StepNumber}");
                        string modelPath = Path.Combine(stepFolder, "model.shm");
                        
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
                        else if (File.Exists(modelPath))
                        {
                            try
                            {
                                // 这里不做实际加载，因为 CogPMAlignTool 通常在运行时加载
                                _logger.Info($"步骤 {step.StepNumber}: (VisionPro模式) 需确认模板文件路径: {step.ToolBlockPath}");
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"步骤 {step.StepNumber}: 加载模板文件失败: {ex.Message}");
                            }
                        }
                    }
                    
                }
                
                UpdateStatus($"已加载模板: {_currentTemplate?.TemplateName}");

                // 执行各个作业步骤
                if (_currentTemplate != null)
                {
                    List<WorkStep> batch = new List<WorkStep>();
                    foreach (var step in _currentTemplate.WorkSteps)
                    {
                        batch.Add(step);
                        if (!step.IsParallel)
                        {
                            await ExecuteStepBatch(batch);
                            batch.Clear();
                        }
                    }
                    if (batch.Count > 0)
                    {
                        await ExecuteStepBatch(batch);
                    }
                }

                // 保存数据
                await ChangeState(WorkflowState.SavingData);
                await SaveImageAndVideo();
                UpdateStatus("数据已保存");

                // 上传MES
                await ChangeState(WorkflowState.UploadingMES);
                await UploadToMES();
                UpdateStatus("已上传MES");

                // 完成
                await ChangeState(WorkflowState.Idle);
                UpdateStatus("作业完成");
                
                // 检查所有步骤状态，确认最终结果
                bool allPassed = _currentTemplate.WorkSteps.All(s => s.Status == "检测通过");
                OverallResultChanged?.Invoke(allPassed ? "PASS" : "FAIL");
                
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

            var tasks = steps.Select(step => ExecuteWorkStep(step)).ToList();
            var whenAllTask = Task.WhenAll(tasks);
            var timeoutTask = Task.Delay(30000); // 30秒超时

            var completedTask = await Task.WhenAny(whenAllTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _logger.Error($"严重警告: 步骤批量执行超时（30秒）...");

                // 强制标记未完成的步骤为失败
                foreach (var step in steps)
                {
                    // 只要不是 "检测通过"，统统视为超时失败
                    if (step.Status != "检测通过" && step.Status != "检测成功")
                    {
                        step.Status = "检测失败";
                        step.FailureReason = "流程强制超时（任务卡死或检测超时）";
                        // 强制触发UI更新
                        UpdateStepStatus(step, "检测失败");
                        OnStepCompleted?.Invoke(step);
                    }
                }
            }
            else
            {
                await whenAllTask;
            }
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
                }
                else
                {
                    // 独立的标准任务
                    await ExecuteStandardCheckAsync(step);
                }
            }
            catch (Exception ex)
            {
                // 统一异常处理
            }
        }

        // 独立的 AR 逻辑方法
        private async Task ExecuteArLoopAsync(WorkStep step)
        {
            UpdateStatus($"步骤{step.StepNumber}: 进入AR装配模式...");
            Stopwatch passTimer = new Stopwatch();
            ICogImage liveImage = null;
            // 循环直到步骤完成
            while (true)
            {
                // 1. 获取最新图
                var camera = _cameraService.GetCamera(step.CameraIndex);
                if (camera != null)
                {
                     liveImage = await Task.Run(() => camera.CaptureSnapshotAsync());
                }

                if (liveImage != null)
                {
                    // 2. 运行快速检测 (建议用 PMAlign)
                    var result = await RunVisionInspection(liveImage, _stepToolBlocks[step.StepNumber], step);

                    // 3. 刷新 AR 界面
                    InspectionResultReady?.Invoke(this, new InspectionResultEventArgs
                    {
                        Image = liveImage,
                        Record = result.Record,
                        IsPassed = result.Passed,
                        Step = step
                    });

                    // 4. 业务判定 (保持时间逻辑)
                    if (result.Passed)
                    {
                        if (!passTimer.IsRunning) passTimer.Start();
                        if (passTimer.Elapsed.TotalSeconds >= step.ArHoldDuration)
                        {
                            step.Status = "检测通过";
                            UpdateStepStatus(step, "检测通过");
                            break; // 满足条件任务结束
                        }
                    }
                    else
                    {
                        passTimer.Reset();
                    }

                    if (liveImage is IDisposable d) d.Dispose();
                }

                // 5. 释放 CPU，防止界面卡死
                await Task.Delay(30);

                // TODO: 建议增加一个 CancellationToken 或超时机制，防止死循环无法退出
            }
        }

        /// <summary>
        /// 执行单个作业步骤
        /// </summary>
        private async Task ExecuteStandardCheckAsync(WorkStep step)
        {
            StabilityState stabilityState = new StabilityState();
            ICogImage lastCapturedImage = null; // 用于记录最后一张图像，防止失败时界面空白
            
            try
            {
                // 设置为检测中状态（黄色）
                _logger.Info($"执行了吗？ExecuteWorkStep");
                UpdateStepStatus(step, "检测中...");
                UpdateStatus($"开始执行步骤{step.StepNumber} (相机{step.CameraIndex + 1})");

                // 持续采集图像，直到图像稳定 AND 匹配分数都满足
                ICogImage validImage = null;
                stepPassed = false;
                int totalCheckCount = 0;
                const int MAX_TOTAL_CHECKS = 20;  // 总检测次数上限（需足够支撑2秒稳定检测：2000ms/30ms≈67次）
                const int CHECK_DELAY = 20;           // 每次检测间隔（毫秒）
                                
                // 主检测循环：同时检测稳定性和匹配分数
                while (!stepPassed && totalCheckCount < MAX_TOTAL_CHECKS)
                {
                    _logger.Info($"执行了吗？stepPassed，{stepPassed.ToString()}");
                    // 步骤1：采集图像
                    await ChangeState(WorkflowState.AddingOumitImage);
                    var currentImage = await CaptureOumitImage(step.CameraIndex);
                    
                    if (currentImage == null)
                    {
                        UpdateStatus($"步骤{step.StepNumber}: 采集图像失败，重试...");
                        await Task.Delay(CHECK_DELAY);
                        continue;
                    }

                    // 更新最后一张图像引用
                    lastCapturedImage = currentImage;

                    // 仅在采集到有效图像后才计数，避免无断点时因为取不到帧而快速耗尽次数
                    totalCheckCount++;
                    _logger.Info($"执行了吗？totalCheckCount次数，{totalCheckCount}");
                    if (_cameraService != null && _cameraService.IsConnected)
                    {
                        // 步骤2：检查图像稳定性
                        await ChangeState(WorkflowState.CheckingDefect);
                        var hasDefect = await CheckImageDefect(currentImage, step, stabilityState);

                        _logger.Info($"检查图像稳定性？hasDefect，{hasDefect}");
                        if (hasDefect)
                        {
                            // 图像不稳定，释放图像并继续下一次循环
                            if (totalCheckCount % 10 == 0)
                            {
                                UpdateStatus($"步骤{step.StepNumber}: 等待图像稳定... ({totalCheckCount}/{MAX_TOTAL_CHECKS})");
                            }

                            await Task.Delay(CHECK_DELAY);
                            continue;
                        }
                    }
                                      
                    UpdateStatus($"步骤{step.StepNumber}: 图像稳定，视觉检测中...");
                    await ChangeState(WorkflowState.CheckingScore);

                    // 执行视觉检测（包含参数公差比对）
                    _logger.Info($"执行了吗？执行视觉检测（包含参数公差比对）");
                    var inspection = await RunVisionInspection(currentImage, _stepToolBlocks[step.StepNumber], step);
                    UpdateStatus($"步骤{step.StepNumber}: {inspection.Reason}");
                    
                    // 触发结果事件，用于UI显示
                    InspectionResultReady?.Invoke(this, new InspectionResultEventArgs 
                    { 
                        Image = currentImage,
                        Record = inspection.Record,
                        Results = inspection.Results,
                        IsPassed = inspection.Passed,
                        Step = step
                    });

                    // 步骤4：判断匹配结果
                    if (inspection.Passed)
                    {
                        stepPassed = true;

                        // 构建保存路径（使用固定的 D 盘路径）
                        string baseFolder = @"D:\data\ZIPImg";
                        string snFolder = Path.Combine(baseFolder, Params.Instance.SN);
                        Directory.CreateDirectory(snFolder);
                        
                        // 设置参数
                        Params.Instance.LocationPicPath = snFolder;
                        
                        // 保存原始 BMP（用于备份）
                        string originalBmpPath = Path.Combine(snFolder, $"Step{step.StepNumber}_original.bmp");
                        SaveCogImageToBmp(currentImage, originalBmpPath);
                        
                        // 保存压缩后的 JPEG（用于 MES 上传）
                        string compressedJpgPath = Path.Combine(snFolder, $"Step{step.StepNumber}_compressed.jpg");
                        SaveCompressedImage(currentImage, compressedJpgPath, 200);

                        _logger.Info($"步骤{step.StepNumber}: 图片已保存 - BMP: {originalBmpPath}, JPEG: {compressedJpgPath}");

                        // 在图像上直接渲染匹配效果
                        // validImage = CogSerializer.DeepCopyObject(currentImage) as ICogImage; // 移除深拷贝，优化性能
                        validImage = currentImage;
                        
                        UpdateStepStatus(step, "检测通过");
                        await UpdateStepImage(step, validImage);
                        UpdateStatus($"步骤{step.StepNumber}: 检测通过");
                        
                        step.CompletedTime = DateTime.Now;
                        OnStepCompleted?.Invoke(step);
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
                if (!stepPassed)
                {
                    UpdateStepStatus(step, "检测失败");
                    UpdateStatus($"步骤{step.StepNumber}: 检测超时，已尝试{totalCheckCount}次");
                    
                    // 步骤失败，设置总体结果为 NG
                    OverallResultChanged?.Invoke("FAIL");
                    
                    step.FailureReason = $"检测超时: 已尝试{totalCheckCount}次，未能同时满足稳定性和匹配条件";
                    step.Status = "检测失败";
                    
                    // 修复：超时失败时，强制更新最后一次采集的图像，防止界面空白
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
                    
                    ResetStabilityCheck(stabilityState);
                }
            }
            catch (Exception ex)
            {
                UpdateStepStatus(step, "检测失败");
                UpdateStatus($"步骤{step.StepNumber}: 执行异常 - {ex.Message}");
                _logger.Error(ex, $"步骤{step.StepNumber}: 执行异常");
                
                OverallResultChanged?.Invoke("FAIL");
                
                step.FailureReason = $"执行异常: {ex.Message}";
                step.Status = "检测失败";
                
                // 修复：异常失败时，也尝试更新图像
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
            // 在WinForms中，ViewModel或UI层应监听变化并Invoke
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

                // Ensure grayscale
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
                        currentGray = currentFrame; // Fallback
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

                // Use the Operator class directly with Execute method for VisionPro 9.0
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
        /// 执行视觉检测：运行工具块并校验参数公差
        /// </summary>
        private async Task<(bool Passed, string Reason, Dictionary<string, double> Results, ICogRecord Record)> RunVisionInspection(ICogImage image, CogToolBlock cogToolBlock, WorkStep step)
        {
            return await Task.Run(() => 
            {
                var results = new Dictionary<string, double>();
                ICogRecord record = null;
                CogToolBlock toolBlock = null;

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
                    if (!_stepToolBlocks.TryGetValue(step.StepNumber, out toolBlock))
                    {
                        _logger.Warn($"步骤 {step.StepNumber}: 未加载 ToolBlock");
                        return (false, "视觉检测异常", results, null);
                    }

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

                        // 获取所有运行记录（图像+图形+文字）
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
                                    return (false, "视觉检测异常", results, record);
                                }
                                
                                double actualVal = results[param.Name];
                                double diff = Math.Abs(actualVal - param.StandardValue);
                                
                                if (diff > param.Tolerance)
                                {
                                    results["ToleranceDiff"] = diff;
                                    var reason = $"参数[{param.Name}]超差: 实际{actualVal:F3} 标准{param.StandardValue:F3} 偏差{diff:F3} > 公差{param.Tolerance}";
                                    _logger.Info(reason);
                                    FireDebug(reason);
                                    return (false, "视觉检测异常", results, record);
                                }
                            }
                            return (true, "参数检测通过", results, record);
                        }
                        else
                        {                            
                            // 既无参数也无Score，仅判断工具运行状态
                            if (toolBlock.RunStatus.Result == CogToolResultConstants.Accept)
                                return (true, "工具运行成功(无参数检查)", results, record);
                            else
                            {
                                var reason = $"工具运行失败: {toolBlock.RunStatus.Message}";
                                _logger.Info(reason);
                                FireDebug(reason);
                                return (false, "视觉检测异常", results, record);
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
                    return (false, "视觉检测异常", results, null);
                }
            });
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
                
                // 需要确保 ZIPHelper 可用 (TODO: Fix ZIPHelper reference)
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
    }

    public class ToolBlockDebugEventArgs : EventArgs
    {
        public int StepNumber { get; set; }
        public WorkStep Step { get; set; }
        public CogToolBlock ToolBlock { get; set; }
        public ICogRecord Record { get; set; }
        public string Message { get; set; }
    }
}
