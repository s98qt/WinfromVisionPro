using Audio900.Models;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Audio900.Services
{
    /// <summary>
    /// 模板存储服务 - 负责模板的保存、加载和删除
    /// </summary>
    public class TemplateStorageService
    {
        private readonly string _templatesDirectory;
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public TemplateStorageService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _templatesDirectory = Path.Combine(baseDir, "HomeworkTemplate");
            Directory.CreateDirectory(_templatesDirectory);
        }

        /// <summary>
        /// 保存模板
        /// </summary>
        public bool SaveTemplate(WorkTemplate template)
        {
            try
            {
                if (template == null || string.IsNullOrWhiteSpace(template.Name))
                {
                    _logger.Error("模板名称不能为空");
                    return false;
                }

                string templateFolder = Path.Combine(_templatesDirectory, template.Name);
                Directory.CreateDirectory(templateFolder);

                // 保存模板元数据
                string metadataPath = Path.Combine(templateFolder, "metadata.json");
                var metadata = new
                {
                    template.Name,
                    template.Description,
                    template.FormatSN,
                    template.CreatedTime,
                    template.ModifiedTime,
                    StepCount = template.Steps.Count
                };
                
                string metadataJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                File.WriteAllText(metadataPath, metadataJson);

                // 保存每个步骤
                for (int i = 0; i < template.Steps.Count; i++)
                {
                    var step = template.Steps[i];
                    string stepFolder = Path.Combine(templateFolder, $"Step{step.StepNumber}");
                    Directory.CreateDirectory(stepFolder);

                    // 保存步骤元数据
                    string stepMetadataPath = Path.Combine(stepFolder, "step_metadata.json");
                    var stepMetadata = new
                    {
                        step.StepNumber,
                        step.Timeout,
                        step.CameraIndex,
                        step.IsParallel,
                        step.IsArMode,
                        step.ShowFailurePrompt,
                        step.FailurePromptMessage,
                        step.ToolBlockPath,
                        step.Parameters
                    };
                    
                    string stepMetadataJson = JsonConvert.SerializeObject(stepMetadata, Formatting.Indented);
                    File.WriteAllText(stepMetadataPath, stepMetadataJson);

                    // 保存采集的图像
                    if (step.CapturedImage != null)
                    {
                        string capturedImagePath = Path.Combine(stepFolder, "captured.bmp");
                        SaveICogImageAsImage(step.CapturedImage, capturedImagePath);
                    }

                    // 保存模板图像
                    if (step.TemplateImage != null)
                    {
                        string templateImagePath = Path.Combine(stepFolder, "template.bmp");
                        SaveICogImageAsImage(step.TemplateImage, templateImagePath);
                    }

                    if (!string.IsNullOrEmpty(step.ToolBlockPath) && File.Exists(step.ToolBlockPath))
                    {
                        string toolBlockDestPath = Path.Combine(stepFolder, "task.vpp");
                        
                        string fullSourcePath = Path.GetFullPath(step.ToolBlockPath);
                        string fullDestPath = Path.GetFullPath(toolBlockDestPath);
                        
                        if (!string.Equals(fullSourcePath, fullDestPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(step.ToolBlockPath, toolBlockDestPath, true);
                            step.ToolBlockPath = toolBlockDestPath; // 更新路径
                        }
                    }
                }

                _logger.Info($"模板保存成功: {template.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"保存模板失败: {template?.Name}");
                return false;
            }
        }

        /// <summary>
        /// 加载模板
        /// </summary>
        public WorkTemplate LoadTemplate(string templateName)
        {
            try
            {
                string templateFolder = Path.Combine(_templatesDirectory, templateName);
                if (!Directory.Exists(templateFolder))
                {
                    _logger.Error($"模板文件夹不存在: {templateFolder}");
                    return null;
                }

                // 加载模板元数据
                string metadataPath = Path.Combine(templateFolder, "metadata.json");
                if (!File.Exists(metadataPath))
                {
                    _logger.Error($"模板元数据文件不存在: {metadataPath}");
                    return null;
                }

                string metadataJson = File.ReadAllText(metadataPath);
                var metadata = JsonConvert.DeserializeObject<JObject>(metadataJson);

                var template = new WorkTemplate
                {
                    Name = metadata["Name"]?.ToString() ?? "",
                    Description = metadata["Description"]?.ToString() ?? "",
                    FormatSN = metadata["FormatSN"]?.ToString() ?? "",
                    CreatedTime = metadata["CreatedTime"]?.ToObject<DateTime>() ?? DateTime.Now,
                    ModifiedTime = metadata["ModifiedTime"]?.ToObject<DateTime>() ?? DateTime.Now
                };

                // 加载步骤
                var stepFolders = Directory.GetDirectories(templateFolder, "Step*")
                    .OrderBy(f => f)
                    .ToList();

                foreach (var stepFolder in stepFolders)
                {
                    string stepMetadataPath = Path.Combine(stepFolder, "step_metadata.json");
                    if (!File.Exists(stepMetadataPath))
                        continue;

                    string stepMetadataJson = File.ReadAllText(stepMetadataPath);
                    var stepMetadata = JsonConvert.DeserializeObject<JObject>(stepMetadataJson);

                    var step = new WorkStep
                    {
                        StepNumber = stepMetadata["StepNumber"]?.ToObject<int>() ?? 0,
                        Timeout = stepMetadata["Timeout"]?.ToObject<int>() ?? 0,
                        CameraIndex = stepMetadata["CameraIndex"]?.ToObject<int>() ?? 0,
                        IsParallel = stepMetadata["IsParallel"]?.ToObject<bool>() ?? false,
                        IsArMode = stepMetadata["IsArMode"]?.ToObject<bool>() ?? false,
                        ShowFailurePrompt = stepMetadata["ShowFailurePrompt"]?.ToObject<bool>() ?? false,
                        FailurePromptMessage = stepMetadata["FailurePromptMessage"]?.ToString() ?? "",
                        ToolBlockPath = stepMetadata["ToolBlockPath"]?.ToString() ?? ""
                    };

                    // 加载参数列表
                    var parameters = stepMetadata["Parameters"]?.ToObject<System.ComponentModel.BindingList<StepParameter>>();
                    if (parameters != null)
                    {
                        step.Parameters = parameters;
                    }

                    // 优先检查当前目录下的 toolblock.vpp，解决绝对路径失效问题
                    string localVppPath = Path.Combine(stepFolder, "task.vpp");
                    if (File.Exists(localVppPath))
                    {
                        step.ToolBlockPath = localVppPath;
                    }

                    // 加载采集的图像
                    string capturedImagePath = Path.Combine(stepFolder, "captured.bmp");
                    if (File.Exists(capturedImagePath))
                    {
                        step.CapturedImage = LoadImageAsICogImage(capturedImagePath);
                        step.ImageSource = LoadImageAsDrawingImage(capturedImagePath);
                    }

                    // 加载模板图像
                    string templateImagePath = Path.Combine(stepFolder, "template.bmp");
                    if (File.Exists(templateImagePath))
                    {
                        step.TemplateImage = LoadImageAsICogImage(templateImagePath);
                    }

                    template.Steps.Add(step);
                }

                _logger.Info($"模板加载成功: {templateName}, 步骤数: {template.Steps.Count}");
                return template;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"加载模板失败: {templateName}");
                return null;
            }
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        public bool DeleteTemplate(string templateName)
        {
            try
            {
                string templateFolder = Path.Combine(_templatesDirectory, templateName);
                if (Directory.Exists(templateFolder))
                {
                    Directory.Delete(templateFolder, true);
                    _logger.Info($"模板删除成功: {templateName}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"删除模板失败: {templateName}");
                return false;
            }
        }

        /// <summary>
        /// 获取所有模板名称
        /// </summary>
        public List<string> GetAllTemplateNames()
        {
            try
            {
                if (!Directory.Exists(_templatesDirectory))
                    return new List<string>();

                return Directory.GetDirectories(_templatesDirectory)
                    .Select(Path.GetFileName)
                    .OrderBy(name => name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取模板列表失败");
                return new List<string>();
            }
        }

        /// <summary>
        /// 保存 ICogImage 为图像文件
        /// </summary>
        private void SaveICogImageAsImage(ICogImage image, string filePath)
        {
            try
            {
                if (image == null) return;

                using (var bitmap = image.ToBitmap())
                {
                    bitmap.Save(filePath, ImageFormat.Bmp);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"保存图像失败: {filePath}");
            }
        }

        /// <summary>
        /// 加载图像文件为 ICogImage
        /// </summary>
        private ICogImage LoadImageAsICogImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                CogImageFileTool fileTool = new CogImageFileTool();
                fileTool.Operator.Open(filePath, CogImageFileModeConstants.Read);
                fileTool.Run();
                return fileTool.OutputImage;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"加载ICogImage失败: {filePath}");
                return null;
            }
        }

        /// <summary>
        /// 加载图像文件为 System.Drawing.Image (用于WinForms显示)
        /// </summary>
        private Image LoadImageAsDrawingImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                // 使用FileStream加载，避免文件锁定
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var img = Image.FromStream(fs))
                    {
                        // 创建副本以允许流关闭
                        return new Bitmap(img);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"加载Drawing.Image失败: {filePath}");
                return null;
            }
        }
    }
}
