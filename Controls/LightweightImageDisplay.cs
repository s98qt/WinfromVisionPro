using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Audio900.Services;

namespace Audio900.Controls
{
    public class LightweightImageDisplay : PictureBox
    {
        private Bitmap _currentImage;
        private readonly List<DetectionBox> _detectionBoxes = new List<DetectionBox>();
        private RectangleF _roi = RectangleF.Empty;
        private double _roiRotation = 0;
        private string _stepNumber = "";
        private bool _showRoi = false;

        public LightweightImageDisplay()
        {
            this.DoubleBuffered = true;
            this.SizeMode = PictureBoxSizeMode.Zoom;
            this.BackColor = Color.Black;
        }

        public void SetImage(Bitmap image)
        {
            if (_currentImage != null && _currentImage != image)
            {
                _currentImage.Dispose();
            }
            _currentImage = image;
            this.Image = image;
        }

        public void SetDetectionResults(List<YoloOBBPrediction> predictions, RectangleF roi, double roiRotation, string stepNumber, bool isInROI)
        {
            _detectionBoxes.Clear();
            _roi = roi;
            _roiRotation = roiRotation;
            _stepNumber = stepNumber;
            _showRoi = roi.Width > 0;

            if (predictions != null)
            {
                foreach (var pred in predictions)
                {
                    var (centerX, centerY, width, height) = CalculateOBBGeometry(pred.RotatedBox);

                    bool currentBoxInROI = false;
                    if (_showRoi)
                    {
                        currentBoxInROI = IsPointInRotatedROI((float)centerX, (float)centerY, roi, roiRotation);
                    }

                    Color boxColor = Color.Green;
                    if (_showRoi)
                    {
                        int expectedClassId = int.Parse(stepNumber) - 1;
                        if (currentBoxInROI && pred.ClassId == expectedClassId)
                        {
                            boxColor = Color.Green;
                        }
                        else
                        {
                            boxColor = Color.Red;
                        }
                    }

                    _detectionBoxes.Add(new DetectionBox
                    {
                        CenterX = centerX,
                        CenterY = centerY,
                        Width = width,
                        Height = height,
                        Angle = pred.Angle,
                        Color = boxColor,
                        Label = $"{pred.Label} : {pred.Confidence:P0} ({pred.Angle:F1}°)"
                    });
                }
            }

            this.Invalidate();
        }

        public void ClearGraphics()
        {
            _detectionBoxes.Clear();
            _showRoi = false;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_currentImage == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float scaleX = (float)this.Width / _currentImage.Width;
            float scaleY = (float)this.Height / _currentImage.Height;
            float scale = Math.Min(scaleX, scaleY);

            float offsetX = (this.Width - _currentImage.Width * scale) / 2;
            float offsetY = (this.Height - _currentImage.Height * scale) / 2;

            if (_showRoi && _roi.Width > 0)
            {
                DrawROI(e.Graphics, scale, offsetX, offsetY);
            }

            foreach (var box in _detectionBoxes)
            {
                DrawDetectionBox(e.Graphics, box, scale, offsetX, offsetY);
            }
        }

        private void DrawROI(Graphics g, float scale, float offsetX, float offsetY)
        {
            using (var pen = new Pen(Color.White, 2))
            using (var font = new Font("Arial", 16, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                double roiCenterX = _roi.X + _roi.Width / 2.0;
                double roiCenterY = _roi.Y + _roi.Height / 2.0;

                g.TranslateTransform(offsetX + (float)roiCenterX * scale, offsetY + (float)roiCenterY * scale);
                g.RotateTransform((float)(_roiRotation * 180 / Math.PI));

                var rect = new RectangleF(
                    -_roi.Width / 2 * scale,
                    -_roi.Height / 2 * scale,
                    _roi.Width * scale,
                    _roi.Height * scale);

                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                g.ResetTransform();

                g.DrawString(_stepNumber, font, brush,
                    offsetX + (float)(roiCenterX + _roi.Width / 2.0 + 10) * scale,
                    offsetY + (float)(roiCenterY - _roi.Height / 2.0) * scale);
            }
        }

        private void DrawDetectionBox(Graphics g, DetectionBox box, float scale, float offsetX, float offsetY)
        {
            using (var pen = new Pen(box.Color, 3))
            using (var font = new Font("Arial", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(box.Color))
            using (var centerPen = new Pen(Color.Red, 2))
            {
                float centerX = offsetX + (float)box.CenterX * scale;
                float centerY = offsetY + (float)box.CenterY * scale;

                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(box.Angle);

                var rect = new RectangleF(
                    -(float)box.Width / 2 * scale,
                    -(float)box.Height / 2 * scale,
                    (float)box.Width * scale,
                    (float)box.Height * scale);

                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                g.ResetTransform();

                g.FillEllipse(centerPen.Brush, centerX - 5, centerY - 5, 10, 10);

                g.DrawString(box.Label, font, brush, centerX, centerY - (float)box.Height / 2 * scale - 20);
            }
        }

        private (double centerX, double centerY, double width, double height) CalculateOBBGeometry(PointF[] corners)
        {
            if (corners == null || corners.Length != 4)
                return (0, 0, 0, 0);

            double centerX = (corners[0].X + corners[1].X + corners[2].X + corners[3].X) / 4.0;
            double centerY = (corners[0].Y + corners[1].Y + corners[2].Y + corners[3].Y) / 4.0;

            double width = Math.Sqrt(Math.Pow(corners[1].X - corners[0].X, 2) + Math.Pow(corners[1].Y - corners[0].Y, 2));
            double height = Math.Sqrt(Math.Pow(corners[2].X - corners[1].X, 2) + Math.Pow(corners[2].Y - corners[1].Y, 2));

            return (centerX, centerY, width, height);
        }

        private bool IsPointInRotatedROI(float pointX, float pointY, RectangleF roi, double rotationRadians)
        {
            if (Math.Abs(rotationRadians) < 0.001)
            {
                return roi.Contains(pointX, pointY);
            }

            double roiCenterX = roi.X + roi.Width / 2.0;
            double roiCenterY = roi.Y + roi.Height / 2.0;

            double dx = pointX - roiCenterX;
            double dy = pointY - roiCenterY;

            double cos = Math.Cos(-rotationRadians);
            double sin = Math.Sin(-rotationRadians);

            double localX = dx * cos - dy * sin;
            double localY = dx * sin + dy * cos;

            double halfWidth = roi.Width / 2.0;
            double halfHeight = roi.Height / 2.0;

            return Math.Abs(localX) <= halfWidth && Math.Abs(localY) <= halfHeight;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _currentImage != null)
            {
                _currentImage.Dispose();
                _currentImage = null;
            }
            base.Dispose(disposing);
        }
    }

    public struct DetectionBox
    {
        public double CenterX;
        public double CenterY;
        public double Width;
        public double Height;
        public float Angle;
        public Color Color;
        public string Label;
    }
}
