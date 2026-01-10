using System;
using System.Drawing;
using System.Windows.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;

namespace Audio900.Views
{
    public partial class SetROIWindow : Form
    {
        public RectangleF SelectedROI { get; private set; }
        public double SelectedROIRotation { get; private set; }
        
        private CogRecordDisplay _display;
        private ICogImage _image;
        private CogRectangleAffine _roiRect;

        public SetROIWindow(ICogImage image, RectangleF existingROI = default, double existingRotation = 0)
        {
            InitializeComponent();
            if (image != null)
            {
                _image = image.CopyBase(CogImageCopyModeConstants.CopyPixels);
            }

            SelectedROI = existingROI;
            SelectedROIRotation = existingRotation;
            
            SetupDisplay();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_image != null && _image.Allocated)
            {
                _display.Image = _image;
                _display.Fit(true);
                
                // 图像加载完成后创建交互式 ROI
                CreateInteractiveROI();
            }
        }

        private void SetupDisplay()
        {
            _display = new CogRecordDisplay();
            _display.Dock = DockStyle.Fill;
            
            panelDisplay.Controls.Add(_display);
            
            lblStatus.Text = "请调整检测区域的位置、大小和角度";
        }

        /// <summary>
        /// 创建交互式 ROI 矩形
        /// </summary>
        private void CreateInteractiveROI()
        {
            if (_image == null) return;
            
            _display.InteractiveGraphics.Clear();
            
            _roiRect = new CogRectangleAffine();
            
            // 如果有已存在的 ROI，使用它；否则在图像中心创建默认 ROI
            if (SelectedROI.Width > 0 && SelectedROI.Height > 0)
            {
                double centerX = SelectedROI.X + SelectedROI.Width / 2.0;
                double centerY = SelectedROI.Y + SelectedROI.Height / 2.0;
                _roiRect.SetCenterLengthsRotationSkew(centerX, centerY, SelectedROI.Width, SelectedROI.Height, SelectedROIRotation, 0);
            }
            else
            {
                // 在图像中心创建默认大小的 ROI
                double centerX = _image.Width / 2.0;
                double centerY = _image.Height / 2.0;
                double defaultSize = Math.Min(_image.Width, _image.Height) * 0.3;
                _roiRect.SetCenterLengthsRotationSkew(centerX, centerY, defaultSize, defaultSize, 0, 0);
            }
            
            // 设置交互属性
            _roiRect.Interactive = true;
            _roiRect.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;  // 允许所有自由度（移动、缩放、旋转）
            _roiRect.Color = CogColorConstants.Yellow;
            _roiRect.LineWidthInScreenPixels = 2;
            _roiRect.SelectedLineWidthInScreenPixels = 3;
            
            _display.InteractiveGraphics.Add(_roiRect, "ROI", true);
            
            UpdateROIStatus();
        }

        /// <summary>
        /// 更新 ROI 状态显示
        /// </summary>
        private void UpdateROIStatus()
        {
            if (_roiRect == null) return;
            
            double centerX = _roiRect.CenterX;
            double centerY = _roiRect.CenterY;
            double width = _roiRect.SideXLength;
            double height = _roiRect.SideYLength;
            double angleDeg = _roiRect.Rotation * 180.0 / Math.PI;
            
            lblStatus.Text = $"中心: ({centerX:F0}, {centerY:F0}), 宽高: {width:F0}x{height:F0}, 角度: {angleDeg:F1}°";
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (_roiRect == null)
            {
                MessageBox.Show("请先设置检测区域！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // 从交互式矩形提取最终的 ROI 数据
            double centerX = _roiRect.CenterX;
            double centerY = _roiRect.CenterY;
            double width = _roiRect.SideXLength;
            double height = _roiRect.SideYLength;
            
            float x = (float)(centerX - width / 2.0);
            float y = (float)(centerY - height / 2.0);
            
            SelectedROI = new RectangleF(x, y, (float)width, (float)height);
            SelectedROIRotation = _roiRect.Rotation;
            
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
            // 重新创建默认 ROI
            SelectedROI = RectangleF.Empty;
            SelectedROIRotation = 0;
            CreateInteractiveROI();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_display != null)
                {
                    _display.Dispose();
                    _display = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
