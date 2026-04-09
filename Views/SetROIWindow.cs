using System;
using System.Drawing;
using System.Windows.Forms;

namespace Audio900.Views
{
    public partial class SetROIWindow : Form
    {
        public RectangleF SelectedROI { get; private set; }
        public double SelectedROIRotation { get; private set; }
        
        private PictureBox _display;
        private Bitmap _image;

        public SetROIWindow(Bitmap image, RectangleF existingROI = default, double existingRotation = 0)
        {
            InitializeComponent();
            if (image != null)
            {
                _image = (Bitmap)image.Clone();
            }

            SelectedROI = existingROI;
            SelectedROIRotation = existingRotation;
            
            SetupDisplay();
            
            MessageBox.Show("VisionPro ROI 设置功能已移除，请手动输入 ROI 参数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_image != null)
            {
                _display.Image = _image;
            }
        }

        private void SetupDisplay()
        {
            _display = new PictureBox();
            _display.Dock = DockStyle.Fill;
            _display.SizeMode = PictureBoxSizeMode.Zoom;
            
            panelDisplay.Controls.Add(_display);
            
            lblStatus.Text = "VisionPro 交互式 ROI 设置已移除";
        }

        private void CreateInteractiveROI()
        {
            // VisionPro 交互式 ROI 功能已移除
        }

        private void UpdateROIStatus()
        {
            if (SelectedROI.Width > 0 && SelectedROI.Height > 0)
            {
                double angleDeg = SelectedROIRotation * 180.0 / Math.PI;
                lblStatus.Text = $"ROI: ({SelectedROI.X:F0}, {SelectedROI.Y:F0}), 宽高: {SelectedROI.Width:F0}x{SelectedROI.Height:F0}, 角度: {angleDeg:F1}°";
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // VisionPro 交互式 ROI 已移除，直接返回
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
