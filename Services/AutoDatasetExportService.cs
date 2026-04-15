using Audio900.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Audio900.Services
{
    public class AutoDatasetExportService
    {
        private readonly string _rootFolder;
        private readonly object _syncRoot = new object();

        public AutoDatasetExportService(string rootFolder = null)
        {
            _rootFolder = string.IsNullOrWhiteSpace(rootFolder)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoDataset")
                : rootFolder;
        }

        public string ExportStepResult(WorkTemplate template, WorkStep step, Bitmap image, bool isPassed, string sn, string employeeId, List<YoloOBBPrediction> predictions, Dictionary<string, double> results)
        {
            if (template == null || step == null || image == null)
            {
                return null;
            }

            string templateName = SanitizePathSegment(template.TemplateName ?? template.Name ?? "UnknownTemplate");
            string stepName = SanitizePathSegment(step.Name ?? $"Step{step.StepNumber}");
            string resultFolderName = isPassed ? "OK" : "NG";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string safeSn = string.IsNullOrWhiteSpace(sn) ? "NOSN" : SanitizePathSegment(sn);
            string baseFileName = $"{timestamp}_SN_{safeSn}_STEP_{step.StepNumber}_CAM_{step.CameraIndex}";
            string captureBucketName = ResolveCaptureBucketName(results, isPassed);

            string baseFolder = Path.Combine(
                _rootFolder,
                templateName,
                $"Step_{step.StepNumber:D2}_{stepName}",
                $"Camera_{step.CameraIndex}",
                resultFolderName,
                captureBucketName);

            string metaFolder = Path.Combine(baseFolder, "meta");

            lock (_syncRoot)
            {
                Directory.CreateDirectory(baseFolder);
                Directory.CreateDirectory(metaFolder);
            }

            string imagePath = Path.Combine(baseFolder, baseFileName + ".jpg");
            string labelPath = Path.Combine(baseFolder, baseFileName + ".txt");
            string metaPath = Path.Combine(metaFolder, baseFileName + ".json");
            string classesPath = Path.Combine(baseFolder, "classes.txt");

            SaveImage(image, imagePath);
            SaveLabels(labelPath, image, predictions);
            SaveClassesFile(classesPath, predictions);
            
            string jsonPath = Path.Combine(baseFolder, baseFileName + ".json");
            SaveXAnyLabelingJson(jsonPath, Path.GetFileName(imagePath), image.Width, image.Height, predictions);

            var meta = new AutoDatasetMeta
            {
                TemplateName = template.TemplateName ?? template.Name,
                StepNumber = step.StepNumber,
                StepName = step.Name,
                CameraIndex = step.CameraIndex,
                ProductSN = sn,
                EmployeeId = employeeId,
                Result = isPassed ? "OK" : "NG",
                CaptureBucket = captureBucketName,
                TopConfidence = GetResultValue(results, "AutoCaptureTopConfidence"),
                AverageConfidence = GetResultValue(results, "AutoCaptureAvgConfidence"),
                PredictionCount = (int)Math.Round(GetResultValue(results, "AutoCapturePredictionCount")),
                QualityScore = GetResultValue(results, "AutoCaptureQualityScore"),
                IsUncertain = GetResultValue(results, "AutoCaptureIsUncertain") >= 0.5,
                Timestamp = DateTime.Now,
                ImagePath = imagePath,
                LabelPath = labelPath,
                Predictions = predictions?.Select(p => new AutoDatasetPredictionMeta
                {
                    ClassId = p.ClassId,
                    Label = p.Label,
                    Confidence = p.Confidence,
                    Angle = p.Angle,
                    Points = p.RotatedBox?.Select(pt => new[] { pt.X, pt.Y }).ToList()
                }).ToList() ?? new List<AutoDatasetPredictionMeta>(),
                Results = results ?? new Dictionary<string, double>()
            };

            File.WriteAllText(metaPath, JsonConvert.SerializeObject(meta, Formatting.Indented), new UTF8Encoding(false));
            return imagePath;
        }

        private void SaveImage(Bitmap image, string imagePath)
        {
            image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void SaveLabels(string labelPath, Bitmap image, List<YoloOBBPrediction> predictions)
        {
            SaveLabels(labelPath, image.Width, image.Height, predictions);
        }

        private void SaveLabels(string labelPath, int imageWidth, int imageHeight, List<YoloOBBPrediction> predictions)
        {
            var lines = new List<string>();

            if (predictions != null)
            {
                foreach (var prediction in predictions)
                {
                    if (prediction?.RotatedBox == null || prediction.RotatedBox.Length < 4)
                    {
                        continue;
                    }

                    var normalized = prediction.RotatedBox
                        .Take(4)
                        .SelectMany(pt => new[]
                        {
                            Clamp01(pt.X / imageWidth).ToString("F6"),
                            Clamp01(pt.Y / imageHeight).ToString("F6")
                        });

                    lines.Add($"{prediction.ClassId} {string.Join(" ", normalized)}");
                }
            }

            File.WriteAllLines(labelPath, lines, new UTF8Encoding(false));
        }

        private void SaveClassesFile(string classesPath, List<YoloOBBPrediction> predictions)
        {
            if (predictions == null || predictions.Count == 0)
            {
                return;
            }

            lock (_syncRoot)
            {
                var existing = File.Exists(classesPath)
                    ? File.ReadAllLines(classesPath).ToList()
                    : new List<string>();

                bool changed = false;
                foreach (var group in predictions.Where(p => p != null).GroupBy(p => p.ClassId).OrderBy(g => g.Key))
                {
                    string label = group.Select(p => p.Label).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? group.Key.ToString();
                    while (existing.Count <= group.Key)
                    {
                        existing.Add(string.Empty);
                        changed = true;
                    }

                    if (string.IsNullOrWhiteSpace(existing[group.Key]))
                    {
                        existing[group.Key] = label;
                        changed = true;
                    }
                }

                if (changed)
                {
                    File.WriteAllLines(classesPath, existing, new UTF8Encoding(false));
                }
            }
        }

        private static double Clamp01(double value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        /// <summary>
        /// 从 Bitmap 导出步骤结果（线程安全，bitmap 由调用方负责 Dispose）
        /// </summary>
        public string ExportFromBitmap(WorkTemplate template, WorkStep step, Bitmap bitmap,
            bool isPassed, string sn, string employeeId,
            List<YoloOBBPrediction> predictions, Dictionary<string, double> results)
        {
            if (template == null || step == null || bitmap == null)
                return null;

            string templateName = SanitizePathSegment(template.TemplateName ?? template.Name ?? "UnknownTemplate");
            string stepName = SanitizePathSegment(step.Name ?? $"Step{step.StepNumber}");
            string resultFolderName = isPassed ? "OK" : "NG";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string safeSn = string.IsNullOrWhiteSpace(sn) ? "NOSN" : SanitizePathSegment(sn);
            string baseFileName = $"{timestamp}_SN_{safeSn}_STEP_{step.StepNumber}_CAM_{step.CameraIndex}";

            string baseFolder = Path.Combine(
                _rootFolder,
                templateName,
                $"Step_{step.StepNumber:D2}_{stepName}",
                $"Camera_{step.CameraIndex}",
                resultFolderName);

            string metaFolder = Path.Combine(baseFolder, "meta");

            lock (_syncRoot)
            {
                Directory.CreateDirectory(baseFolder);
                Directory.CreateDirectory(metaFolder);
            }

            string imagePath = Path.Combine(baseFolder, baseFileName + ".jpg");
            string labelPath = Path.Combine(baseFolder, baseFileName + ".txt");
            string metaPath = Path.Combine(metaFolder, baseFileName + ".json");
            string classesPath = Path.Combine(baseFolder, "classes.txt");

            bitmap.Save(imagePath, ImageFormat.Jpeg);
            SaveLabels(labelPath, bitmap.Width, bitmap.Height, predictions);
            SaveClassesFile(classesPath, predictions);
            
            string jsonPath = Path.Combine(baseFolder, baseFileName + ".json");
            SaveXAnyLabelingJson(jsonPath, Path.GetFileName(imagePath), bitmap.Width, bitmap.Height, predictions);

            var meta = new AutoDatasetMeta
            {
                TemplateName = template.TemplateName ?? template.Name,
                StepNumber = step.StepNumber,
                StepName = step.Name,
                CameraIndex = step.CameraIndex,
                ProductSN = sn,
                EmployeeId = employeeId,
                Result = isPassed ? "OK" : "NG",
                Timestamp = DateTime.Now,
                ImagePath = imagePath,
                LabelPath = labelPath,
                Predictions = predictions?.Select(p => new AutoDatasetPredictionMeta
                {
                    ClassId = p.ClassId,
                    Label = p.Label,
                    Confidence = p.Confidence,
                    Angle = p.Angle,
                    Points = p.RotatedBox?.Select(pt => new[] { pt.X, pt.Y }).ToList()
                }).ToList() ?? new List<AutoDatasetPredictionMeta>(),
                Results = results ?? new Dictionary<string, double>()
            };

            File.WriteAllText(metaPath, JsonConvert.SerializeObject(meta, Formatting.Indented), new UTF8Encoding(false));
            return imagePath;
        }

        private void SaveXAnyLabelingJson(string jsonPath, string imageFileName, int imageWidth, int imageHeight, List<YoloOBBPrediction> predictions)
        {
            var shapes = new List<object>();

            if (predictions != null)
            {
                foreach (var pred in predictions)
                {
                    if (pred?.RotatedBox == null || pred.RotatedBox.Length < 4)
                        continue;

                    var points = pred.RotatedBox.Take(4)
                        .Select(pt => new[] { (double)pt.X, (double)pt.Y })
                        .ToList();

                    double directionRad = pred.Angle * Math.PI / 180.0;

                    shapes.Add(new
                    {
                        label = pred.Label ?? "defect",
                        points = points,
                        group_id = (int?)null,
                        description = "",
                        shape_type = "rotation",
                        direction = directionRad,
                        flags = new { },
                        attributes = new { }
                    });
                }
            }

            var json = new
            {
                version = "5.0.1",
                flags = new { },
                shapes = shapes,
                imagePath = imageFileName,
                imageData = (string)null,
                imageHeight = imageHeight,
                imageWidth = imageWidth
            };

            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(json, Formatting.Indented), new UTF8Encoding(false));
        }

        private static string SanitizePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unknown";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? "Unknown" : cleaned;
        }

        private static string ResolveCaptureBucketName(Dictionary<string, double> results, bool isPassed)
        {
            int bucketCode = (int)Math.Round(GetResultValue(results, "AutoCaptureBucketCode"));
            switch (bucketCode)
            {
                case 0: return "HighConfidence_OK";
                case 1: return "LowConfidence_OK";
                case 2: return "Uncertain_OK";
                case 3: return "HighConfidence_NG";
                case 4: return "LowConfidence_NG";
                case 5: return "Uncertain_NG";
                default: return isPassed ? "OK" : "NG";
            }
        }

        private static double GetResultValue(Dictionary<string, double> results, string key)
        {
            if (results == null || string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            if (results.TryGetValue(key, out double value))
            {
                return value;
            }

            var matchKey = results.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            return matchKey != null ? results[matchKey] : 0;
        }

        private class AutoDatasetMeta
        {
            public string TemplateName { get; set; }
            public int StepNumber { get; set; }
            public string StepName { get; set; }
            public int CameraIndex { get; set; }
            public string ProductSN { get; set; }
            public string EmployeeId { get; set; }
            public string Result { get; set; }
            public string CaptureBucket { get; set; }
            public double TopConfidence { get; set; }
            public double AverageConfidence { get; set; }
            public int PredictionCount { get; set; }
            public double QualityScore { get; set; }
            public bool IsUncertain { get; set; }
            public DateTime Timestamp { get; set; }
            public string ImagePath { get; set; }
            public string LabelPath { get; set; }
            public List<AutoDatasetPredictionMeta> Predictions { get; set; }
            public Dictionary<string, double> Results { get; set; }
        }

        private class AutoDatasetPredictionMeta
        {
            public int ClassId { get; set; }
            public string Label { get; set; }
            public float Confidence { get; set; }
            public float Angle { get; set; }
            public List<float[]> Points { get; set; }
        }
    }
}
