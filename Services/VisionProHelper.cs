using System;
using System.Collections.Generic;
using System.Drawing;
using Cognex.VisionPro;
using Cognex.VisionPro.Implementation;

namespace Audio900.Services
{
    public static class VisionProHelper
    {
        /// <summary>
        /// 将 YOLO 预测结果转换为 VisionPro 的 Record 结构，以便在控件中显示
        /// </summary>
        public static ICogRecord CreateRecordFromYoloResults(ICogImage image, List<YoloPrediction> predictions, CogColorConstants color = CogColorConstants.Green)
        {
            // 1. 创建根记录，包含原始图像
            var rootRecord = new CogRecord("Image", typeof(ICogImage), CogRecordUsageConstants.Result, false, image, "Image");

            if (predictions == null || predictions.Count == 0)
                return rootRecord;

            // 2. 为每个预测结果创建图形记录
            foreach (var pred in predictions)
            {
                // 2.1 创建矩形框
                var rect = new CogRectangleAffine();
                double centerX = pred.Rectangle.X + pred.Rectangle.Width / 2.0;
                double centerY = pred.Rectangle.Y + pred.Rectangle.Height / 2.0;
                
                rect.SetCenterLengthsRotationSkew(centerX, centerY, pred.Rectangle.Width, pred.Rectangle.Height, 0, 0);
                rect.Color = color;
                rect.LineWidthInScreenPixels = 2;
                rect.Interactive = false; // 只读展示

                // 2.2 创建标签
                var label = new CogGraphicLabel();
                label.SetXYText(pred.Rectangle.X, Math.Max(0, pred.Rectangle.Y - 20), $"{pred.Label} : {pred.Confidence:P0}");
                label.Color = color;
                label.Font = new Font("Arial", 10, FontStyle.Bold);
                label.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                
                var boxRecord = new CogRecord($"Box_{pred.ClassId}_{Guid.NewGuid()}", typeof(ICogGraphic), CogRecordUsageConstants.Result, true, rect, "Detection Box");
                rootRecord.SubRecords.Add(boxRecord);

                var labelRecord = new CogRecord($"Label_{pred.ClassId}_{Guid.NewGuid()}", typeof(ICogGraphic), CogRecordUsageConstants.Result, true, label, "Detection Label");
                rootRecord.SubRecords.Add(labelRecord);
            }

            return rootRecord;
        }
    }
}
