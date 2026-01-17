namespace Audio900.Views
{
    partial class DataCollectionWindow
    {
        private System.ComponentModel.IContainer components = null;

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numTargetCount = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxMode = new System.Windows.Forms.GroupBox();
            this.lblAutoIntervalHint = new System.Windows.Forms.Label();
            this.numAutoInterval = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.rbAutoMode = new System.Windows.Forms.RadioButton();
            this.rbManualMode = new System.Windows.Forms.RadioButton();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxProgress = new System.Windows.Forms.GroupBox();
            this.lblLastImage = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnStartCollect = new System.Windows.Forms.Button();
            this.btnStopCollect = new System.Windows.Forms.Button();
            this.btnManualCapture = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupBoxSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetCount)).BeginInit();
            this.groupBoxMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.groupBoxProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Controls.Add(this.btnBrowse);
            this.groupBoxSettings.Controls.Add(this.txtSavePath);
            this.groupBoxSettings.Controls.Add(this.label1);
            this.groupBoxSettings.Controls.Add(this.numTargetCount);
            this.groupBoxSettings.Controls.Add(this.label2);
            this.groupBoxSettings.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(760, 90);
            this.groupBoxSettings.TabIndex = 0;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "采集设置";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(660, 25);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(80, 23);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtSavePath
            // 
            this.txtSavePath.Location = new System.Drawing.Point(90, 27);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(560, 21);
            this.txtSavePath.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "保存路径:";
            // 
            // numTargetCount
            // 
            this.numTargetCount.Location = new System.Drawing.Point(90, 57);
            this.numTargetCount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numTargetCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTargetCount.Name = "numTargetCount";
            this.numTargetCount.Size = new System.Drawing.Size(120, 21);
            this.numTargetCount.TabIndex = 1;
            this.numTargetCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "目标数量:";
            // 
            // groupBoxMode
            // 
            this.groupBoxMode.Controls.Add(this.lblAutoIntervalHint);
            this.groupBoxMode.Controls.Add(this.numAutoInterval);
            this.groupBoxMode.Controls.Add(this.label3);
            this.groupBoxMode.Controls.Add(this.rbAutoMode);
            this.groupBoxMode.Controls.Add(this.rbManualMode);
            this.groupBoxMode.Location = new System.Drawing.Point(12, 108);
            this.groupBoxMode.Name = "groupBoxMode";
            this.groupBoxMode.Size = new System.Drawing.Size(760, 80);
            this.groupBoxMode.TabIndex = 1;
            this.groupBoxMode.TabStop = false;
            this.groupBoxMode.Text = "采集模式";
            // 
            // lblAutoIntervalHint
            // 
            this.lblAutoIntervalHint.AutoSize = true;
            this.lblAutoIntervalHint.Enabled = false;
            this.lblAutoIntervalHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblAutoIntervalHint.Location = new System.Drawing.Point(240, 52);
            this.lblAutoIntervalHint.Name = "lblAutoIntervalHint";
            this.lblAutoIntervalHint.Size = new System.Drawing.Size(281, 12);
            this.lblAutoIntervalHint.TabIndex = 4;
            this.lblAutoIntervalHint.Text = "提示: 手拿产品在摄像头下变换角度，自动拍照";
            // 
            // numAutoInterval
            // 
            this.numAutoInterval.Enabled = false;
            this.numAutoInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAutoInterval.Location = new System.Drawing.Point(120, 48);
            this.numAutoInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numAutoInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAutoInterval.Name = "numAutoInterval";
            this.numAutoInterval.Size = new System.Drawing.Size(100, 21);
            this.numAutoInterval.TabIndex = 3;
            this.numAutoInterval.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "间隔(毫秒):";
            // 
            // rbAutoMode
            // 
            this.rbAutoMode.AutoSize = true;
            this.rbAutoMode.Location = new System.Drawing.Point(22, 25);
            this.rbAutoMode.Name = "rbAutoMode";
            this.rbAutoMode.Size = new System.Drawing.Size(179, 16);
            this.rbAutoMode.TabIndex = 1;
            this.rbAutoMode.Text = "自动连拍（定时自动采集）";
            this.rbAutoMode.UseVisualStyleBackColor = true;
            this.rbAutoMode.CheckedChanged += new System.EventHandler(this.rbAutoMode_CheckedChanged);
            // 
            // rbManualMode
            // 
            this.rbManualMode.AutoSize = true;
            this.rbManualMode.Checked = true;
            this.rbManualMode.Location = new System.Drawing.Point(240, 25);
            this.rbManualMode.Name = "rbManualMode";
            this.rbManualMode.Size = new System.Drawing.Size(221, 16);
            this.rbManualMode.TabIndex = 0;
            this.rbManualMode.TabStop = true;
            this.rbManualMode.Text = "手动拍照（按空格键或点击按钮）";
            this.rbManualMode.UseVisualStyleBackColor = true;
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.BackColor = System.Drawing.Color.Black;
            this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPreview.Location = new System.Drawing.Point(12, 194);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(760, 400);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreview.TabIndex = 2;
            this.pictureBoxPreview.TabStop = false;
            // 
            // groupBoxProgress
            // 
            this.groupBoxProgress.Controls.Add(this.lblLastImage);
            this.groupBoxProgress.Controls.Add(this.lblProgress);
            this.groupBoxProgress.Controls.Add(this.progressBar);
            this.groupBoxProgress.Location = new System.Drawing.Point(12, 600);
            this.groupBoxProgress.Name = "groupBoxProgress";
            this.groupBoxProgress.Size = new System.Drawing.Size(760, 80);
            this.groupBoxProgress.TabIndex = 3;
            this.groupBoxProgress.TabStop = false;
            this.groupBoxProgress.Text = "采集进度";
            // 
            // lblLastImage
            // 
            this.lblLastImage.AutoSize = true;
            this.lblLastImage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblLastImage.Location = new System.Drawing.Point(400, 25);
            this.lblLastImage.Name = "lblLastImage";
            this.lblLastImage.Size = new System.Drawing.Size(41, 12);
            this.lblLastImage.TabIndex = 2;
            this.lblLastImage.Text = "最新: ";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblProgress.Location = new System.Drawing.Point(20, 23);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(121, 19);
            this.lblProgress.TabIndex = 1;
            this.lblProgress.Text = "已采集: 0 / 100 张";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(20, 48);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(720, 23);
            this.progressBar.TabIndex = 0;
            // 
            // btnStartCollect
            // 
            this.btnStartCollect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnStartCollect.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartCollect.ForeColor = System.Drawing.Color.White;
            this.btnStartCollect.Location = new System.Drawing.Point(12, 690);
            this.btnStartCollect.Name = "btnStartCollect";
            this.btnStartCollect.Size = new System.Drawing.Size(180, 40);
            this.btnStartCollect.TabIndex = 4;
            this.btnStartCollect.Text = "开始采集";
            this.btnStartCollect.UseVisualStyleBackColor = false;
            this.btnStartCollect.Click += new System.EventHandler(this.btnStartCollect_Click);
            // 
            // btnStopCollect
            // 
            this.btnStopCollect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnStopCollect.Enabled = false;
            this.btnStopCollect.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopCollect.ForeColor = System.Drawing.Color.White;
            this.btnStopCollect.Location = new System.Drawing.Point(198, 690);
            this.btnStopCollect.Name = "btnStopCollect";
            this.btnStopCollect.Size = new System.Drawing.Size(180, 40);
            this.btnStopCollect.TabIndex = 5;
            this.btnStopCollect.Text = "停止采集";
            this.btnStopCollect.UseVisualStyleBackColor = false;
            this.btnStopCollect.Click += new System.EventHandler(this.btnStopCollect_Click);
            // 
            // btnManualCapture
            // 
            this.btnManualCapture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnManualCapture.Enabled = false;
            this.btnManualCapture.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnManualCapture.ForeColor = System.Drawing.Color.White;
            this.btnManualCapture.Location = new System.Drawing.Point(384, 690);
            this.btnManualCapture.Name = "btnManualCapture";
            this.btnManualCapture.Size = new System.Drawing.Size(180, 40);
            this.btnManualCapture.TabIndex = 6;
            this.btnManualCapture.Text = "手动拍照 (空格)";
            this.btnManualCapture.UseVisualStyleBackColor = false;
            this.btnManualCapture.Click += new System.EventHandler(this.btnManualCapture_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnOpenFolder.Location = new System.Drawing.Point(592, 690);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(180, 40);
            this.btnOpenFolder.TabIndex = 7;
            this.btnOpenFolder.Text = "打开文件夹";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblStatus.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblStatus.Location = new System.Drawing.Point(12, 740);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(176, 17);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "就绪 - 请点击开始采集按钮";
            // 
            // DataCollectionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 761);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnManualCapture);
            this.Controls.Add(this.btnStopCollect);
            this.Controls.Add(this.btnStartCollect);
            this.Controls.Add(this.groupBoxProgress);
            this.Controls.Add(this.pictureBoxPreview);
            this.Controls.Add(this.groupBoxMode);
            this.Controls.Add(this.groupBoxSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "DataCollectionWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据采集工具 - 快速采集训练图片";
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetCount)).EndInit();
            this.groupBoxMode.ResumeLayout(false);
            this.groupBoxMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.groupBoxProgress.ResumeLayout(false);
            this.groupBoxProgress.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numTargetCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxMode;
        private System.Windows.Forms.Label lblAutoIntervalHint;
        private System.Windows.Forms.NumericUpDown numAutoInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rbAutoMode;
        private System.Windows.Forms.RadioButton rbManualMode;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.GroupBox groupBoxProgress;
        private System.Windows.Forms.Label lblLastImage;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnStartCollect;
        private System.Windows.Forms.Button btnStopCollect;
        private System.Windows.Forms.Button btnManualCapture;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Label lblStatus;
    }
}
