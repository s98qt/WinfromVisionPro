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
        /// 从 OBB 的 4 个角点计算中心点、宽度、高度
        /// </summary>
        private static (double centerX, double centerY, double width, double height) CalculateOBBGeometry(PointF[] corners)
        {
            if (corners == null || corners.Length != 4)
                return (0, 0, 0, 0);

            // 计算中心点（4个角点的平均值）
            double centerX = (corners[0].X + corners[1].X + corners[2].X + corners[3].X) / 4.0;
            double centerY = (corners[0].Y + corners[1].Y + corners[2].Y + corners[3].Y) / 4.0;

            // 计算宽度和高度（相邻两边的长度）
            double width = Math.Sqrt(Math.Pow(corners[1].X - corners[0].X, 2) + Math.Pow(corners[1].Y - corners[0].Y, 2));
            double height = Math.Sqrt(Math.Pow(corners[2].X - corners[1].X, 2) + Math.Pow(corners[2].Y - corners[1].Y, 2));

            return (centerX, centerY, width, height);
        }

        /// <summary>
        /// 判断点是否在旋转矩形 ROI 内
        /// </summary>
        private static bool IsPointInRotatedROI(float pointX, float pointY, RectangleF roi, double rotationRadians)
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

        /// <summary>
        /// 将 YOLO 预测结果直接应用到 CogRecordDisplay 的 StaticGraphics，用于过程检测的 AR 实时显示
        /// </summary>
        /// <param name="display">显示控件</param>
        /// <param name="image">图像</param>
        /// <param name="predictions">YOLO 预测结果</param>
        /// <param name="step">当前步骤（用于获取 ROI 信息）</param>
        /// <param name="isInROI">检测物体是否在 ROI 内</param>
        public static void ApplyYoloResultsToDisplay(CogRecordDisplay display, ICogImage image, List<YoloOBBPrediction> predictions, Audio900.Models.WorkStep step = null, bool isInROI = false)
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

            // 4. 绘制 YOLO 检测框（只显示在 ROI 内的检测框，支持旋转）
            if (predictions != null && predictions.Count > 0)
            {
                foreach (var pred in predictions)
                {
                    // 从 OBB 的 4 个角点计算中心点、宽度、高度
                    var (centerX, centerY, width, height) = CalculateOBBGeometry(pred.RotatedBox);

                    // 为每个检测框单独判断是否在 ROI 内
                    bool currentBoxInROI = false;
                    if (step != null && step.EnableProcessDetection && step.DetectionROI.Width > 0)
                    {
                        currentBoxInROI = IsPointInRotatedROI(
                            (float)centerX, (float)centerY, 
                            step.DetectionROI, step.DetectionROIRotation);
                    }

                    // 只显示在 ROI 内的检测框（绿色），过滤掉 ROI 外的检测框
                    if (!step.EnableProcessDetection || currentBoxInROI)
                    {
                        CogColorConstants boxColor = currentBoxInROI ? CogColorConstants.Green : CogColorConstants.Yellow;

                        // 绘制识别框（支持旋转角度）
                        var rect = new CogRectangleAffine();
                        // 将角度从度转换为弧度
                        double rotationRadians = pred.Angle * Math.PI / 180.0;
                        rect.SetCenterLengthsRotationSkew(centerX, centerY,
                            width, height, 
                            rotationRadians, 0); // 使用 YOLO OBB 输出的旋转角度
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

                        // 添加标签（显示在检测框上方）
                        var label = new CogGraphicLabel();
                        label.SetXYText(centerX, centerY - height / 2.0 - 20,
                            $"{pred.Label} : {pred.Confidence:P0} ({pred.Angle:F1}°)");
                        label.Color = boxColor;
                        label.Font = new Font("Arial", 10, FontStyle.Bold);
                        label.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                        display.StaticGraphics.Add(label, "DetectionLabel");
                    }
                }
            }
        }
    }
}
