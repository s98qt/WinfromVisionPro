using System.Drawing;
using System.Windows.Forms;

namespace Audio900.Views
{
    partial class CameraDisplayForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RecordDisplay = new Cognex.VisionPro.CogRecordDisplay();
            ((System.ComponentModel.ISupportInitialize)(this.RecordDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // RecordDisplay
            // 
            this.RecordDisplay.BackColor = System.Drawing.Color.Black;
            this.RecordDisplay.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.RecordDisplay.ColorMapLowerRoiLimit = 0D;
            this.RecordDisplay.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.RecordDisplay.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.RecordDisplay.ColorMapUpperRoiLimit = 1D;
            this.RecordDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecordDisplay.DoubleTapZoomCycleLength = 2;
            this.RecordDisplay.DoubleTapZoomSensitivity = 2.5D;
            //this.RecordDisplay.HorizontalScrollBar = false; // 不能在设计器里设置，设置了就会报错
            this.RecordDisplay.Location = new System.Drawing.Point(0, 0);
            this.RecordDisplay.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.RecordDisplay.MouseWheelSensitivity = 1D;
            this.RecordDisplay.Name = "RecordDisplay";
            this.RecordDisplay.Size = new System.Drawing.Size(800, 450);
            this.RecordDisplay.TabIndex = 0;
            //this.RecordDisplay.VerticalScrollBar = false;
            // 
            // CameraDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.RecordDisplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CameraDisplayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "相机独立显示屏";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.RecordDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Cognex.VisionPro.CogRecordDisplay RecordDisplay;
    }
}
