using System;
using System.Drawing;
using System.Windows.Forms;

namespace Audio900.Views
{
    /// <summary>
    /// 用于多显示器模式下，在副屏全屏显示独立相机画面的窗体
    /// </summary>
    public partial class CameraDisplayForm : Form
    {
        public CameraDisplayForm()
        {
            InitializeComponent();
        }

        // 屏蔽 Alt+F4 意外关闭
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
