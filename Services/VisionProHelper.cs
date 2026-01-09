using System;
using System.Collections.Generic;
using System.Drawing;
using Cognex.VisionPro;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.Display;

namespace Audio900.Services
{
    public static class VisionProHelper
    {
        /// <summary>
        /// 将 YOLO 预测结果转换为 VisionPro 的 Record 结构（简化版本，仅返回图像记录）
        /// 注意：此方法已不再用于显示图形，图形显示请使用 ApplyYoloResultsToDisplay 方法
        /// </summary>
        public static ICogRecord CreateRecordFromYoloResults(ICogImage image, List<YoloPrediction> predictions, CogColorConstants color = CogColorConstants.Green)
        {
            // 创建简单的图像记录（不包含图形，图形将在 MainForm 中通过 StaticGraphics 添加）
            var rootRecord = new CogRecord("Image", typeof(ICogImage), CogRecordUsageConstants.Result, false, image, "Image");
            return rootRecord;
        }

        /// <summary>
        /// 将 YOLO 预测结果直接应用到 CogRecordDisplay 的 StaticGraphics，用于 AR 实时显示
        /// </summary>
        public static void ApplyYoloResultsToDisplay(CogRecordDisplay display, ICogImage image, List<YoloPrediction> predictions, CogColorConstants color = CogColorConstants.Green)
        {
            if (display == null) return;

            // 1. 清空现有的静态图形
            display.StaticGraphics.Clear();

            // 2. 设置图像
            if (image != null)
            {

                display.Image = image;
            }

            // 3. 为每个预测结果创建图形并添加到 StaticGraphics
            if (predictions != null && predictions.Count > 0)
            {
                foreach (var pred in predictions)
                {
                    // 3.1 创建矩形框
                    var rect = new CogRectangleAffine();
                    double centerX = pred.Rectangle.X + pred.Rectangle.Width / 2.0;
                    double centerY = pred.Rectangle.Y + pred.Rectangle.Height / 2.0;
                    
                    rect.SetCenterLengthsRotationSkew(centerX, centerY, pred.Rectangle.Width, pred.Rectangle.Height, 0, 0);
                    rect.Color = color;
                    rect.LineWidthInScreenPixels = 2;
                    rect.Interactive = false; // 只读展示

                    // 3.2 创建标签
                    var label = new CogGraphicLabel();
                    label.SetXYText(pred.Rectangle.X, Math.Max(0, pred.Rectangle.Y - 20), $"{pred.Label} : {pred.Confidence:P0}");
                    label.Color = color;
                    label.Font = new Font("Arial", 10, FontStyle.Bold);
                    label.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                    
                    // 使用 StaticGraphics.Add 添加图形
                    display.StaticGraphics.Add(rect, "Detection Box");
                    display.StaticGraphics.Add(label, "Detection Label");
                }
            }
        }
    }
}
