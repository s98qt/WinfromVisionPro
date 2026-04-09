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
            this.RecordDisplay = new Audio900.Controls.LightweightImageDisplay();
            this.SuspendLayout();
            // 
            // RecordDisplay
            // 
            this.RecordDisplay.BackColor = System.Drawing.Color.Black;
            this.RecordDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecordDisplay.Location = new System.Drawing.Point(0, 0);
            this.RecordDisplay.Name = "RecordDisplay";
            this.RecordDisplay.Size = new System.Drawing.Size(800, 450);
            this.RecordDisplay.TabIndex = 0;
            // 
            // CameraDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.RecordDisplay);
            this.Name = "CameraDisplayForm";
            this.Text = "相机独立显示屏";
            this.ResumeLayout(false);

        }

        #endregion
        private Audio900.Controls.LightweightImageDisplay RecordDisplay;
    }
}
