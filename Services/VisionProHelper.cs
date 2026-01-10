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
        /// 将 YOLO 预测结果直接应用到 CogRecordDisplay 的 StaticGraphics，用于过程检测的 AR 实时显示
        /// 包含：ROI 框显示、检测框颜色判断（绿色=在 ROI 内，黄色=在 ROI 外）、中心点标记
        /// </summary>
        /// <param name="display">显示控件</param>
        /// <param name="image">图像</param>
        /// <param name="predictions">YOLO 预测结果</param>
        /// <param name="step">当前步骤（用于获取 ROI 信息）</param>
        /// <param name="isInROI">检测物体是否在 ROI 内</param>
        public static void ApplyYoloResultsToDisplay(CogRecordDisplay display, ICogImage image, List<YoloPrediction> predictions, Audio900.Models.WorkStep step = null, bool isInROI = false)
        {
            if (display == null) return;

            // 1. 清空现有的静态图形
            display.StaticGraphics.Clear();

            // 2. 设置图像
            if (image != null)
            {
                display.Image = image;
            }

            // 3. 如果启用了过程检测，绘制 ROI 框（白色细框，不可交互）
            if (step != null && step.EnableProcessDetection && step.DetectionROI.Width > 0)
            {
                var roiRect = new CogRectangleAffine();
                double roiCenterX = step.DetectionROI.X + step.DetectionROI.Width / 2.0;
                double roiCenterY = step.DetectionROI.Y + step.DetectionROI.Height / 2.0;

                // 使用保存的旋转角度
                roiRect.SetCenterLengthsRotationSkew(
                    roiCenterX, roiCenterY,
                    step.DetectionROI.Width, step.DetectionROI.Height,
                    step.DetectionROIRotation, 0);
                roiRect.Color = CogColorConstants.White;
                roiRect.LineWidthInScreenPixels = 2;
                roiRect.Interactive = false;  // 运行模式：不可交互
                display.StaticGraphics.Add(roiRect, "ROI");

                // 添加步骤编号标签
                var stepLabel = new CogGraphicLabel();
                stepLabel.SetXYText(roiCenterX + step.DetectionROI.Width / 2.0 + 10,
                                   roiCenterY - step.DetectionROI.Height / 2.0,
                                   step.StepNumber.ToString());
                stepLabel.Color = CogColorConstants.White;
                stepLabel.Font = new Font("Arial", 16, FontStyle.Bold);
                stepLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                display.StaticGraphics.Add(stepLabel, "StepNumber");
            }

            // 4. 绘制 YOLO 检测框（根据中心点位置选择颜色）
            if (predictions != null && predictions.Count > 0)
            {
                foreach (var pred in predictions)
                {
                    double centerX = pred.Rectangle.X + pred.Rectangle.Width / 2.0;
                    double centerY = pred.Rectangle.Y + pred.Rectangle.Height / 2.0;

                    // 判断颜色：中心点在 ROI 内 = 绿色，否则 = 黄色
                    CogColorConstants boxColor = isInROI ? CogColorConstants.Green : CogColorConstants.Yellow;

                    // 绘制识别框
                    var rect = new CogRectangleAffine();
                    rect.SetCenterLengthsRotationSkew(centerX, centerY,
                        pred.Rectangle.Width, pred.Rectangle.Height, 0, 0);
                    rect.Color = boxColor;
                    rect.LineWidthInScreenPixels = 3;
                    rect.Interactive = false;
                    display.StaticGraphics.Add(rect, "DetectionBox");

                    // 绘制中心点（红色小点）
                    var centerDot = new CogCircle();
                    centerDot.CenterX = centerX;
                    centerDot.CenterY = centerY;
                    centerDot.Radius = 5;
                    centerDot.Color = CogColorConstants.Red;
                    centerDot.LineWidthInScreenPixels = 2;
                    display.StaticGraphics.Add(centerDot, "CenterPoint");

                    // 添加标签
                    var label = new CogGraphicLabel();
                    label.SetXYText(pred.Rectangle.X, Math.Max(0, pred.Rectangle.Y - 20),
                        $"{pred.Label} : {pred.Confidence:P0}");
                    label.Color = boxColor;
                    label.Font = new Font("Arial", 10, FontStyle.Bold);
                    label.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                    display.StaticGraphics.Add(label, "DetectionLabel");
                }
            }
        }
    }
}
