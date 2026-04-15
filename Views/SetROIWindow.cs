using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Audio900.Views
{
    // 自定义双缓冲 Panel 解决闪烁问题
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
        }
    }

    /// <summary>
    /// 交互式 ROI 设置窗口 —— 纯 GDI+ 实现，无需 VisionPro
    /// 支持：鼠标拖拽移动、4角和4边拖拽缩放、旋转手柄拖拽旋转
    /// </summary>
    public partial class SetROIWindow : Form
    {
        // ─── 输出 ───────────────────────────────────────────
        public RectangleF SelectedROI { get; private set; }
        public double SelectedROIRotation { get; private set; }

        // ─── 原始图像 ────────────────────────────────────────
        private Bitmap _image;

        // ─── 显示画布 ────────────────────────────────────────
        private DoubleBufferedPanel _canvas;

        // ─── ROI 状态（画布坐标系） ───────────────────────────
        private PointF _roiCenter;
        private float _roiHalfW;
        private float _roiHalfH;
        private double _roiAngle;

        // ─── 图像在画布中的映射 ──────────────────────────────
        private RectangleF _imgRect;

        // ─── 鼠标交互 ────────────────────────────────────────
        private enum DragMode 
        { 
            None, Move, RotateHandle, 
            CornerTL, CornerTR, CornerBR, CornerBL,
            EdgeT, EdgeR, EdgeB, EdgeL
        }
        private DragMode _dragMode = DragMode.None;
        
        // 拖拽起点状态
        private PointF _dragStartMousePos;
        private PointF _roiCenterStart;
        private float _roiHalfWStart;
        private float _roiHalfHStart;
        private double _roiAngleStart;

        private const float HANDLE_RADIUS = 6f;
        private const float ROTATE_HANDLE_DIST = 35f;

        public SetROIWindow(Bitmap image, RectangleF existingROI = default, double existingRotation = 0)
        {
            InitializeComponent();
            _image = image != null ? (Bitmap)image.Clone() : null;
            SelectedROIRotation = existingRotation;
            _roiAngle = existingRotation;

            SetupCanvas();
            _existingROI = existingROI;
        }

        private RectangleF _existingROI;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RecalcImageRect();
            InitRoi();
            UpdateStatusLabel();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_canvas != null)
            {
                RecalcImageRect();
                _canvas.Invalidate();
            }
        }

        // ─── 画布初始化 ──────────────────────────────────────
        private void SetupCanvas()
        {
            _canvas = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Cursor = Cursors.Cross
            };
            _canvas.Paint += Canvas_Paint;
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;

            panelDisplay.Controls.Clear();
            panelDisplay.Controls.Add(_canvas);
        }

        // ─── 图像布局计算 ────────────────────────────────────
        private void RecalcImageRect()
        {
            if (_image == null)
            {
                _imgRect = new RectangleF(0, 0, _canvas.Width, _canvas.Height);
                return;
            }

            float cw = _canvas.Width;
            float ch = _canvas.Height;
            float iw = _image.Width;
            float ih = _image.Height;

            float scale = Math.Min(cw / iw, ch / ih);
            float dw = iw * scale;
            float dh = ih * scale;
            _imgRect = new RectangleF((cw - dw) / 2f, (ch - dh) / 2f, dw, dh);
        }

        // ─── ROI 初始化 ──────────────────────────────────────
        private void InitRoi()
        {
            if (_existingROI.Width > 0 && _existingROI.Height > 0 && _image != null)
            {
                PointF c = ImgToCanvas(new PointF(
                    _existingROI.X + _existingROI.Width / 2f,
                    _existingROI.Y + _existingROI.Height / 2f));
                _roiCenter = c;
                float scale = _imgRect.Width / _image.Width;
                _roiHalfW = _existingROI.Width * scale / 2f;
                _roiHalfH = _existingROI.Height * scale / 2f;
            }
            else
            {
                _roiCenter = new PointF(_imgRect.X + _imgRect.Width / 2f, _imgRect.Y + _imgRect.Height / 2f);
                _roiHalfW = _imgRect.Width * 0.25f;
                _roiHalfH = _imgRect.Height * 0.25f;
            }
        }

        // ─── 坐标转换 ────────────────────────────────────────
        private PointF ImgToCanvas(PointF imgPt)
        {
            if (_image == null) return imgPt;
            float scale = _imgRect.Width / _image.Width;
            return new PointF(_imgRect.X + imgPt.X * scale, _imgRect.Y + imgPt.Y * scale);
        }

        private PointF CanvasToImg(PointF canvasPt)
        {
            if (_image == null) return canvasPt;
            float scale = _imgRect.Width / _image.Width;
            return new PointF((canvasPt.X - _imgRect.X) / scale, (canvasPt.Y - _imgRect.Y) / scale);
        }

        private PointF CanvasToLocal(PointF canvasPt, PointF center, double angle)
        {
            double dx = canvasPt.X - center.X;
            double dy = canvasPt.Y - center.Y;
            double cos = Math.Cos(-angle);
            double sin = Math.Sin(-angle);
            return new PointF((float)(dx * cos - dy * sin), (float)(dx * sin + dy * cos));
        }

        private PointF LocalToCanvas(PointF localPt, PointF center, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new PointF(
                (float)(center.X + localPt.X * cos - localPt.Y * sin),
                (float)(center.Y + localPt.X * sin + localPt.Y * cos));
        }

        // ─── 获取手柄位置（画布坐标）───────────────────────
        private PointF[] GetCorners()
        {
            return new[] {
                LocalToCanvas(new PointF(-_roiHalfW, -_roiHalfH), _roiCenter, _roiAngle), // TL
                LocalToCanvas(new PointF( _roiHalfW, -_roiHalfH), _roiCenter, _roiAngle), // TR
                LocalToCanvas(new PointF( _roiHalfW,  _roiHalfH), _roiCenter, _roiAngle), // BR
                LocalToCanvas(new PointF(-_roiHalfW,  _roiHalfH), _roiCenter, _roiAngle)  // BL
            };
        }

        private PointF[] GetEdges()
        {
            return new[] {
                LocalToCanvas(new PointF(0, -_roiHalfH), _roiCenter, _roiAngle), // Top
                LocalToCanvas(new PointF(_roiHalfW, 0), _roiCenter, _roiAngle),  // Right
                LocalToCanvas(new PointF(0, _roiHalfH), _roiCenter, _roiAngle),  // Bottom
                LocalToCanvas(new PointF(-_roiHalfW, 0), _roiCenter, _roiAngle)  // Left
            };
        }

        private PointF GetRotateHandle()
        {
            return LocalToCanvas(new PointF(0, -_roiHalfH - ROTATE_HANDLE_DIST), _roiCenter, _roiAngle);
        }

        // ─── 绘制 ────────────────────────────────────────────
        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (_image != null)
                g.DrawImage(_image, _imgRect);

            DrawOverlay(g);

            PointF[] corners = GetCorners();
            using (var pen = new Pen(Color.FromArgb(255, 30, 144, 255), 2f))
            {
                g.DrawPolygon(pen, corners);
            }

            // 绘制中心十字
            DrawCenterCross(g);

            // 绘制手柄
            foreach (var c in corners) DrawHandle(g, c, Color.FromArgb(255, 30, 144, 255));
            foreach (var ePt in GetEdges()) DrawHandle(g, ePt, Color.White);

            PointF rh = GetRotateHandle();
            PointF topEdge = LocalToCanvas(new PointF(0, -_roiHalfH), _roiCenter, _roiAngle);
            using (var pen = new Pen(Color.Yellow, 1.5f) { DashStyle = DashStyle.Dash })
                g.DrawLine(pen, topEdge, rh);
            DrawHandle(g, rh, Color.Yellow);
        }

        private void DrawCenterCross(Graphics g)
        {
            PointF p1 = LocalToCanvas(new PointF(-5, 0), _roiCenter, _roiAngle);
            PointF p2 = LocalToCanvas(new PointF(5, 0), _roiCenter, _roiAngle);
            PointF p3 = LocalToCanvas(new PointF(0, -5), _roiCenter, _roiAngle);
            PointF p4 = LocalToCanvas(new PointF(0, 5), _roiCenter, _roiAngle);
            using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1.5f))
            {
                g.DrawLine(pen, p1, p2);
                g.DrawLine(pen, p3, p4);
            }
        }

        private void DrawOverlay(Graphics g)
        {
            using (var path = new GraphicsPath())
            {
                path.AddRectangle(new RectangleF(0, 0, _canvas.Width, _canvas.Height));
                path.AddPolygon(GetCorners());
                using (var brush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                    g.FillPath(brush, path);
            }
        }

        private void DrawHandle(Graphics g, PointF center, Color color)
        {
            float r = HANDLE_RADIUS;
            RectangleF rect = new RectangleF(center.X - r, center.Y - r, r * 2, r * 2);
            using (var brush = new SolidBrush(color))
                g.FillEllipse(brush, rect);
            using (var pen = new Pen(Color.Black, 1f))
                g.DrawEllipse(pen, rect);
        }

        // ─── 命中检测 ────────────────────────────────────────
        private DragMode HitTest(PointF pt)
        {
            float threshold = HANDLE_RADIUS + 4;

            if (Distance(pt, GetRotateHandle()) <= threshold) return DragMode.RotateHandle;

            PointF[] corners = GetCorners();
            if (Distance(pt, corners[0]) <= threshold) return DragMode.CornerTL;
            if (Distance(pt, corners[1]) <= threshold) return DragMode.CornerTR;
            if (Distance(pt, corners[2]) <= threshold) return DragMode.CornerBR;
            if (Distance(pt, corners[3]) <= threshold) return DragMode.CornerBL;

            PointF[] edges = GetEdges();
            if (Distance(pt, edges[0]) <= threshold) return DragMode.EdgeT;
            if (Distance(pt, edges[1]) <= threshold) return DragMode.EdgeR;
            if (Distance(pt, edges[2]) <= threshold) return DragMode.EdgeB;
            if (Distance(pt, edges[3]) <= threshold) return DragMode.EdgeL;

            PointF localPt = CanvasToLocal(pt, _roiCenter, _roiAngle);
            if (Math.Abs(localPt.X) <= _roiHalfW && Math.Abs(localPt.Y) <= _roiHalfH)
                return DragMode.Move;

            return DragMode.None;
        }

        // ─── 鼠标交互 ────────────────────────────────────────
        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _dragMode = HitTest(e.Location);
            if (_dragMode != DragMode.None)
            {
                _dragStartMousePos = e.Location;
                _roiCenterStart = _roiCenter;
                _roiHalfWStart = _roiHalfW;
                _roiHalfHStart = _roiHalfH;
                _roiAngleStart = _roiAngle;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointF pt = e.Location;

            if (_dragMode == DragMode.None)
            {
                DragMode hover = HitTest(pt);
                if (hover == DragMode.RotateHandle) _canvas.Cursor = Cursors.Hand;
                else if (hover == DragMode.Move) _canvas.Cursor = Cursors.SizeAll;
                else if (hover != DragMode.None) _canvas.Cursor = Cursors.Cross; // VisionPro 风格统一十字
                else _canvas.Cursor = Cursors.Default;
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (_dragMode == DragMode.Move)
            {
                _roiCenter = new PointF(
                    _roiCenterStart.X + (pt.X - _dragStartMousePos.X),
                    _roiCenterStart.Y + (pt.Y - _dragStartMousePos.Y));
            }
            else if (_dragMode == DragMode.RotateHandle)
            {
                double angle = Math.Atan2(pt.X - _roiCenter.X, _roiCenter.Y - pt.Y);
                _roiAngle = angle;
            }
            else
            {
                // 缩放逻辑：将当前鼠标点投射到初始 ROI 局部坐标系
                PointF localMouse = CanvasToLocal(pt, _roiCenterStart, _roiAngleStart);
                
                float left = -_roiHalfWStart;
                float right = _roiHalfWStart;
                float top = -_roiHalfHStart;
                float bottom = _roiHalfHStart;

                // 根据拖动点限制局部坐标的边界
                const float MIN_SIZE = 10f;
                switch (_dragMode)
                {
                    case DragMode.CornerTL: left = Math.Min(localMouse.X, right - MIN_SIZE); top = Math.Min(localMouse.Y, bottom - MIN_SIZE); break;
                    case DragMode.CornerTR: right = Math.Max(localMouse.X, left + MIN_SIZE); top = Math.Min(localMouse.Y, bottom - MIN_SIZE); break;
                    case DragMode.CornerBR: right = Math.Max(localMouse.X, left + MIN_SIZE); bottom = Math.Max(localMouse.Y, top + MIN_SIZE); break;
                    case DragMode.CornerBL: left = Math.Min(localMouse.X, right - MIN_SIZE); bottom = Math.Max(localMouse.Y, top + MIN_SIZE); break;
                    
                    case DragMode.EdgeT: top = Math.Min(localMouse.Y, bottom - MIN_SIZE); break;
                    case DragMode.EdgeB: bottom = Math.Max(localMouse.Y, top + MIN_SIZE); break;
                    case DragMode.EdgeL: left = Math.Min(localMouse.X, right - MIN_SIZE); break;
                    case DragMode.EdgeR: right = Math.Max(localMouse.X, left + MIN_SIZE); break;
                }

                // 重新计算局部中心点和宽高
                float newLocalCenterX = (left + right) / 2f;
                float newLocalCenterY = (top + bottom) / 2f;
                
                _roiHalfW = (right - left) / 2f;
                _roiHalfH = (bottom - top) / 2f;

                // 将局部中心点转换回世界坐标（注意要加上初始中心）
                _roiCenter = LocalToCanvas(new PointF(newLocalCenterX, newLocalCenterY), _roiCenterStart, _roiAngleStart);
            }

            CommitRoi();
            _canvas.Invalidate();
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            _dragMode = DragMode.None;
            _canvas.Cursor = Cursors.Default;
        }

        private static float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // ─── 提交 ROI 到输出属性 ─────────────────────────────
        private void CommitRoi()
        {
            SelectedROIRotation = _roiAngle;
            UpdateStatusLabel();
        }

        // ─── 状态栏 ──────────────────────────────────────────
        private void UpdateStatusLabel()
        {
            double angleDeg = _roiAngle * 180.0 / Math.PI;

            if (_image != null)
            {
                PointF imgCenter = CanvasToImg(_roiCenter);
                float scale = _imgRect.Width / _image.Width;
                float imgW = _roiHalfW * 2f / scale;
                float imgH = _roiHalfH * 2f / scale;
                lblStatus.Text = $"中心:({imgCenter.X:F0},{imgCenter.Y:F0})  宽高:{imgW:F0}×{imgH:F0}  角度:{angleDeg:F1}°  |  拖拽矩形移动，四边/角缩放，黄色手柄旋转";
            }
            else
            {
                lblStatus.Text = $"角度:{angleDeg:F1}°";
            }
        }

        // ─── 按钮事件 ────────────────────────────────────────
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (_image != null)
            {
                float scale = _imgRect.Width / _image.Width;
                float imgW = _roiHalfW * 2f / scale;
                float imgH = _roiHalfH * 2f / scale;
                PointF imgCenter = CanvasToImg(_roiCenter);
                SelectedROI = new RectangleF(imgCenter.X - imgW / 2f, imgCenter.Y - imgH / 2f, imgW, imgH);
            }
            else
            {
                SelectedROI = new RectangleF(_roiCenter.X - _roiHalfW, _roiCenter.Y - _roiHalfH, _roiHalfW * 2, _roiHalfH * 2);
            }
            SelectedROIRotation = _roiAngle;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _roiAngle = 0;
            RecalcImageRect();
            InitRoi();
            CommitRoi();
            _canvas.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }
    }
}
