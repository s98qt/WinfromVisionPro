using System;
using System.Collections.Generic;
using System.Drawing;
using Cognex.VisionPro;
using Cognex.VisionPro.Implementation;

namespace Audio900.Services
{
    public static class VisionProHelper
    {
        // 全局静态 Font，避免重复创建导致内存泄漏
        private static readonly Font _sharedLabelFont = new Font("Arial", 10, FontStyle.Bold);

        public static ICogRecord CreateRecordFromYoloResults(ICogImage image, List<YoloPrediction> predictions, CogColorConstants color = CogColorConstants.Green)
        {
            // 1. 创建根记录
            var rootRecord = new CogRecord("Image", typeof(ICogImage), CogRecordUsageConstants.Result, false, image, "Image");

            if (predictions == null || predictions.Count == 0)
                return rootRecord;

            // 2. 遍历预测结果
            for (int i = 0; i < predictions.Count; i++)
            {
                var pred = predictions[i];

                // --- 2.1 创建矩形框 ---
                var rect = new CogRectangleAffine();
                double centerX = pred.Rectangle.X + pred.Rectangle.Width / 2.0;
                double centerY = pred.Rectangle.Y + pred.Rectangle.Height / 2.0;

                rect.SetCenterLengthsRotationSkew(centerX, centerY, pred.Rectangle.Width, pred.Rectangle.Height, 0, 0);
                rect.Color = color;

                // 动态调整颜色：如果置信度低，可以变色提醒 (可选)
                if (pred.Confidence < 0.8) rect.Color = CogColorConstants.Yellow;

                rect.LineWidthInScreenPixels = 2;
                rect.Interactive = false; // 只读

                // 关键：RecordKey 用索引即可，不要用 Guid，减少 GC 压力
                string keySuffix = $"{pred.ClassId}_{i}";

                // 添加到子记录
                rootRecord.SubRecords.Add(new CogRecord($"Box_{keySuffix}", typeof(ICogGraphic), CogRecordUsageConstants.Result, false, rect, "Box"));

                // --- 2.2 创建标签 ---
                var label = new CogGraphicLabel();
                // 防止文字跑出屏幕上方
                double labelY = pred.Rectangle.Y - 20;

                label.SetXYText(pred.Rectangle.X, labelY, $"{pred.Label} {pred.Confidence:P0}");
                label.Color = rect.Color;

                // 关键修复：使用全局静态 Font，杜绝内存泄漏
                label.Font = _sharedLabelFont;

                label.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;

                rootRecord.SubRecords.Add(new CogRecord($"Label_{keySuffix}", typeof(ICogGraphic), CogRecordUsageConstants.Result, false, label, "Label"));
            }

            return rootRecord;
        }
    }
}
