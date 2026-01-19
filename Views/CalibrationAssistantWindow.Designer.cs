namespace Audio900.Views
{
    partial class CalibrationAssistantWindow
    {
        private System.ComponentModel.IContainer components = null;

        private Cognex.VisionPro.CogRecordDisplay cogRecordDisplay;
        private Cognex.VisionPro.CalibFix.CogCalibCheckerboardEditV2 cogCalibEditV2;
        private System.Windows.Forms.Button btnStartPreview;
        private System.Windows.Forms.Button btnStopPreview;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox groupBoxParams;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numRows;
        private System.Windows.Forms.NumericUpDown numColumns;
        private System.Windows.Forms.NumericUpDown numSquareSize;
        private System.Windows.Forms.Label lblRMSError;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;

        private void InitializeComponent()
        {
            this.cogRecordDisplay = new Cognex.VisionPro.CogRecordDisplay();
            this.cogCalibEditV2 = new Cognex.VisionPro.CalibFix.CogCalibCheckerboardEditV2();
            this.btnStartPreview = new System.Windows.Forms.Button();
            this.btnStopPreview = new System.Windows.Forms.Button();
            this.btnCapture = new System.Windows.Forms.Button();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBoxParams = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numRows = new System.Windows.Forms.NumericUpDown();
            this.numColumns = new System.Windows.Forms.NumericUpDown();
            this.numSquareSize = new System.Windows.Forms.NumericUpDown();
            this.lblRMSError = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.groupBoxParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSquareSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogRecordDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogCalibEditV2)).BeginInit();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.cogRecordDisplay);
            this.panelLeft.Controls.Add(this.lblInstructions);
            this.panelLeft.Controls.Add(this.btnStartPreview);
            this.panelLeft.Controls.Add(this.btnStopPreview);
            this.panelLeft.Controls.Add(this.btnCapture);
            this.panelLeft.Controls.Add(this.btnCalibrate);
            this.panelLeft.Controls.Add(this.btnSave);
            this.panelLeft.Controls.Add(this.lblRMSError);
            this.panelLeft.Controls.Add(this.lblStatus);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(700, 800);
            this.panelLeft.TabIndex = 0;
            // 
            // cogRecordDisplay
            // 
            this.cogRecordDisplay.Location = new System.Drawing.Point(12, 12);
            this.cogRecordDisplay.Name = "cogRecordDisplay";
            this.cogRecordDisplay.Size = new System.Drawing.Size(676, 480);
            this.cogRecordDisplay.TabIndex = 0;
            // 
            // lblInstructions
            // 
            this.lblInstructions.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblInstructions.Location = new System.Drawing.Point(12, 500);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(676, 150);
            this.lblInstructions.TabIndex = 1;
            this.lblInstructions.Text = "【标定步骤】";
            // 
            // groupBoxParams
            // 
            this.groupBoxParams.Controls.Add(this.label1);
            this.groupBoxParams.Controls.Add(this.label2);
            this.groupBoxParams.Controls.Add(this.label3);
            this.groupBoxParams.Controls.Add(this.numRows);
            this.groupBoxParams.Controls.Add(this.numColumns);
            this.groupBoxParams.Controls.Add(this.numSquareSize);
            this.groupBoxParams.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.groupBoxParams.Location = new System.Drawing.Point(12, 625);
            this.groupBoxParams.Name = "groupBoxParams";
            this.groupBoxParams.Size = new System.Drawing.Size(676, 80);
            this.groupBoxParams.TabIndex = 2;
            this.groupBoxParams.TabStop = false;
            this.groupBoxParams.Text = "标定参数";
            this.groupBoxParams.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "行数：";
            // 
            // numRows
            // 
            this.numRows.Location = new System.Drawing.Point(80, 28);
            this.numRows.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.numRows.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
            this.numRows.Name = "numRows";
            this.numRows.Size = new System.Drawing.Size(80, 23);
            this.numRows.TabIndex = 1;
            this.numRows.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(200, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "列数：";
            // 
            // numColumns
            // 
            this.numColumns.Location = new System.Drawing.Point(260, 28);
            this.numColumns.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.numColumns.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
            this.numColumns.Name = "numColumns";
            this.numColumns.Size = new System.Drawing.Size(80, 23);
            this.numColumns.TabIndex = 3;
            this.numColumns.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(380, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "格子边长(mm)：";
            // 
            // numSquareSize
            // 
            this.numSquareSize.DecimalPlaces = 2;
            this.numSquareSize.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numSquareSize.Location = new System.Drawing.Point(490, 28);
            this.numSquareSize.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numSquareSize.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numSquareSize.Name = "numSquareSize";
            this.numSquareSize.Size = new System.Drawing.Size(100, 23);
            this.numSquareSize.TabIndex = 5;
            this.numSquareSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnStartPreview
            // 
            this.btnStartPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnStartPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartPreview.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnStartPreview.ForeColor = System.Drawing.Color.White;
            this.btnStartPreview.Location = new System.Drawing.Point(12, 660);
            this.btnStartPreview.Name = "btnStartPreview";
            this.btnStartPreview.Size = new System.Drawing.Size(100, 35);
            this.btnStartPreview.TabIndex = 3;
            this.btnStartPreview.Text = "开始预览";
            this.btnStartPreview.UseVisualStyleBackColor = false;
            this.btnStartPreview.Click += new System.EventHandler(this.btnStartPreview_Click);
            // 
            // btnStopPreview
            // 
            this.btnStopPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.btnStopPreview.Enabled = false;
            this.btnStopPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopPreview.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnStopPreview.ForeColor = System.Drawing.Color.White;
            this.btnStopPreview.Location = new System.Drawing.Point(120, 660);
            this.btnStopPreview.Name = "btnStopPreview";
            this.btnStopPreview.Size = new System.Drawing.Size(100, 35);
            this.btnStopPreview.TabIndex = 4;
            this.btnStopPreview.Text = "停止预览";
            this.btnStopPreview.UseVisualStyleBackColor = false;
            this.btnStopPreview.Click += new System.EventHandler(this.btnStopPreview_Click);
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnCapture.Enabled = false;
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnCapture.ForeColor = System.Drawing.Color.White;
            this.btnCapture.Location = new System.Drawing.Point(228, 660);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(100, 35);
            this.btnCapture.TabIndex = 5;
            this.btnCapture.Text = "抓取图像";
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.btnCalibrate.Enabled = false;
            this.btnCalibrate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalibrate.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnCalibrate.ForeColor = System.Drawing.Color.White;
            this.btnCalibrate.Location = new System.Drawing.Point(336, 660);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(100, 35);
            this.btnCalibrate.TabIndex = 6;
            this.btnCalibrate.Text = "执行标定";
            this.btnCalibrate.UseVisualStyleBackColor = false;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(39)))), ((int)(((byte)(176)))));
            this.btnSave.Enabled = false;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(444, 660);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "保存标定";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblRMSError
            // 
            this.lblRMSError.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblRMSError.Location = new System.Drawing.Point(12, 705);
            this.lblRMSError.Name = "lblRMSError";
            this.lblRMSError.Size = new System.Drawing.Size(676, 20);
            this.lblRMSError.TabIndex = 8;
            this.lblRMSError.Text = "RMS Error: --";
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblStatus.Location = new System.Drawing.Point(12, 730);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(676, 20);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "请点击开始预览开始标定流程";
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.cogCalibEditV2);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(700, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(500, 800);
            this.panelRight.TabIndex = 1;
            // 
            // cogCalibEditV2
            // 
            this.cogCalibEditV2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cogCalibEditV2.Location = new System.Drawing.Point(0, 0);
            this.cogCalibEditV2.Name = "cogCalibEditV2";
            this.cogCalibEditV2.Size = new System.Drawing.Size(684, 861);
            this.cogCalibEditV2.TabIndex = 0;
            // 
            // CalibrationAssistantWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CalibrationAssistantWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "相机标定助手 - VisionPro棋盘格标定";
            this.groupBoxParams.ResumeLayout(false);
            this.groupBoxParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSquareSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogRecordDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogCalibEditV2)).EndInit();
            this.panelLeft.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
