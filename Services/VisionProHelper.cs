using System;
using System.Collections.Generic;
using System.Drawing;

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
        /// VisionPro 显示功能已移除，此方法已被 LightweightImageDisplay 替代
        /// </summary>
        [Obsolete("已被 LightweightImageDisplay 替代")]
        public static void ApplyYoloResultsToDisplay_DEPRECATED()
        {
            // VisionPro 显示功能已完全移除
        }
    }
}
