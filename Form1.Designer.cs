namespace Audio900
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnOpenCamera = new System.Windows.Forms.Button();
            this.lblCameraStatus = new System.Windows.Forms.Label();
            this.lblImageArea = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelCameraDisplay = new System.Windows.Forms.Panel();
            this.lblNoCamera = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFPS = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblResult = new System.Windows.Forms.Label();
            this.panelMesStatus = new System.Windows.Forms.Panel();
            this.lblMesStatus = new System.Windows.Forms.Label();
            this.panelStepImage = new System.Windows.Forms.Panel();
            this.pictureBoxStep = new System.Windows.Forms.PictureBox();
            this.lblStepTitle = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnTemplateManage = new System.Windows.Forms.Button();
            this.cmbTemplates = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtViewName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStationNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtViewNo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtEmployeeId = new System.Windows.Forms.TextBox();
            this.lblEmployeeId = new System.Windows.Forms.Label();
            this.txtProductSN = new System.Windows.Forms.TextBox();
            this.lblProductSN = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelCameraDisplay.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.panelMesStatus.SuspendLayout();
            this.panelStepImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.panelTop.Controls.Add(this.btnOpenCamera);
            this.panelTop.Controls.Add(this.lblCameraStatus);
            this.panelTop.Controls.Add(this.lblImageArea);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1600, 60);
            this.panelTop.TabIndex = 0;
            // 
            // btnOpenCamera
            // 
            this.btnOpenCamera.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenCamera.BackColor = System.Drawing.Color.White;
            this.btnOpenCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenCamera.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpenCamera.Location = new System.Drawing.Point(1480, 15);
            this.btnOpenCamera.Name = "btnOpenCamera";
            this.btnOpenCamera.Size = new System.Drawing.Size(100, 30);
            this.btnOpenCamera.TabIndex = 2;
            this.btnOpenCamera.Text = "打开相机";
            this.btnOpenCamera.UseVisualStyleBackColor = false;
            this.btnOpenCamera.Click += new System.EventHandler(this.btnOpenCamera_Click);
            // 
            // lblCameraStatus
            // 
            this.lblCameraStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCameraStatus.BackColor = System.Drawing.Color.Red;
            this.lblCameraStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCameraStatus.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCameraStatus.ForeColor = System.Drawing.Color.White;
            this.lblCameraStatus.Location = new System.Drawing.Point(1340, 18);
            this.lblCameraStatus.Name = "lblCameraStatus";
            this.lblCameraStatus.Size = new System.Drawing.Size(120, 24);
            this.lblCameraStatus.TabIndex = 1;
            this.lblCameraStatus.Text = "相机等待连接";
            this.lblCameraStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblImageArea
            // 
            this.lblImageArea.AutoSize = true;
            this.lblImageArea.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblImageArea.ForeColor = System.Drawing.Color.White;
            this.lblImageArea.Location = new System.Drawing.Point(15, 15);
            this.lblImageArea.Name = "lblImageArea";
            this.lblImageArea.Size = new System.Drawing.Size(86, 31);
            this.lblImageArea.TabIndex = 0;
            this.lblImageArea.Text = "影像区";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 60);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelCameraDisplay);
            this.splitContainer1.Panel1.Controls.Add(this.statusStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelRight);
            this.splitContainer1.Size = new System.Drawing.Size(1600, 840);
            this.splitContainer1.SplitterDistance = 1280;
            this.splitContainer1.TabIndex = 1;
            // 
            // panelCameraDisplay
            // 
            this.panelCameraDisplay.BackColor = System.Drawing.Color.Black;
            this.panelCameraDisplay.Controls.Add(this.lblNoCamera);
            this.panelCameraDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCameraDisplay.Location = new System.Drawing.Point(0, 0);
            this.panelCameraDisplay.Name = "panelCameraDisplay";
            this.panelCameraDisplay.Size = new System.Drawing.Size(1280, 814);
            this.panelCameraDisplay.TabIndex = 0;
            // 
            // lblNoCamera
            // 
            this.lblNoCamera.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNoCamera.AutoSize = true;
            this.lblNoCamera.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNoCamera.ForeColor = System.Drawing.Color.White;
            this.lblNoCamera.Location = new System.Drawing.Point(540, 390);
            this.lblNoCamera.Name = "lblNoCamera";
            this.lblNoCamera.Size = new System.Drawing.Size(206, 31);
            this.lblNoCamera.TabIndex = 0;
            this.lblNoCamera.Text = "未检测到相机";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelFPS});
            this.statusStrip1.Location = new System.Drawing.Point(0, 814);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1280, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(1165, 21);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "系统就绪";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelFPS
            // 
            this.toolStripStatusLabelFPS.Name = "toolStripStatusLabelFPS";
            this.toolStripStatusLabelFPS.Size = new System.Drawing.Size(100, 21);
            this.toolStripStatusLabelFPS.Text = "FPS: 0.00";
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.groupBoxResult);
            this.panelRight.Controls.Add(this.listViewSteps);
            this.panelRight.Controls.Add(this.groupBoxControl);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(10);
            this.panelRight.Size = new System.Drawing.Size(496, 900);
            this.panelRight.TabIndex = 0;
            // 
            // groupBoxResult
            // 
            this.groupBoxResult.Controls.Add(this.lblResult);
            this.groupBoxResult.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxResult.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxResult.Location = new System.Drawing.Point(10, 780);
            this.groupBoxResult.Name = "groupBoxResult";
            this.groupBoxResult.Size = new System.Drawing.Size(476, 110);
            this.groupBoxResult.TabIndex = 2;
            this.groupBoxResult.TabStop = false;
            this.groupBoxResult.Text = "检测结果";
            // 
            // lblResult
            // 
            this.lblResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResult.Font = new System.Drawing.Font("微软雅黑", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblResult.Location = new System.Drawing.Point(3, 25);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(470, 82);
            this.lblResult.TabIndex = 0;
            this.lblResult.Text = "---";
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listViewSteps
            // 
            this.listViewSteps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStep,
            this.columnHeaderStatus,
            this.columnHeaderScore});
            this.listViewSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSteps.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listViewSteps.FullRowSelect = true;
            this.listViewSteps.GridLines = true;
            this.listViewSteps.HideSelection = false;
            this.listViewSteps.Location = new System.Drawing.Point(10, 380);
            this.listViewSteps.Name = "listViewSteps";
            this.listViewSteps.Size = new System.Drawing.Size(476, 510);
            this.listViewSteps.TabIndex = 1;
            this.listViewSteps.UseCompatibleStateImageBehavior = false;
            this.listViewSteps.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderStep
            // 
            this.columnHeaderStep.Text = "步骤";
            this.columnHeaderStep.Width = 120;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "状态";
            this.columnHeaderStatus.Width = 200;
            // 
            // columnHeaderScore
            // 
            this.columnHeaderScore.Text = "分数";
            this.columnHeaderScore.Width = 120;
            // 
            // groupBoxControl
            // 
            this.groupBoxControl.Controls.Add(this.btnStop);
            this.groupBoxControl.Controls.Add(this.btnStart);
            this.groupBoxControl.Controls.Add(this.txtEmployeeId);
            this.groupBoxControl.Controls.Add(this.label2);
            this.groupBoxControl.Controls.Add(this.txtProductSN);
            this.groupBoxControl.Controls.Add(this.label1);
            this.groupBoxControl.Controls.Add(this.btnTemplateManage);
            this.groupBoxControl.Controls.Add(this.cmbTemplates);
            this.groupBoxControl.Controls.Add(this.lblTemplate);
            this.groupBoxControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxControl.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxControl.Location = new System.Drawing.Point(10, 10);
            this.groupBoxControl.Name = "groupBoxControl";
            this.groupBoxControl.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxControl.Size = new System.Drawing.Size(476, 370);
            this.groupBoxControl.TabIndex = 0;
            this.groupBoxControl.TabStop = false;
            this.groupBoxControl.Text = "控制面板";
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.btnStop.Enabled = false;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(13, 310);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(450, 45);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "停止检测";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Location = new System.Drawing.Point(13, 259);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(450, 45);
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "开始检测";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtEmployeeId
            // 
            this.txtEmployeeId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmployeeId.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtEmployeeId.Location = new System.Drawing.Point(120, 213);
            this.txtEmployeeId.Name = "txtEmployeeId";
            this.txtEmployeeId.Size = new System.Drawing.Size(343, 29);
            this.txtEmployeeId.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(13, 217);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "员工工号:";
            // 
            // txtProductSN
            // 
            this.txtProductSN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProductSN.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtProductSN.Location = new System.Drawing.Point(120, 168);
            this.txtProductSN.Name = "txtProductSN";
            this.txtProductSN.Size = new System.Drawing.Size(343, 29);
            this.txtProductSN.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 172);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "产品SN码:";
            // 
            // btnTemplateManage
            // 
            this.btnTemplateManage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTemplateManage.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTemplateManage.Location = new System.Drawing.Point(343, 70);
            this.btnTemplateManage.Name = "btnTemplateManage";
            this.btnTemplateManage.Size = new System.Drawing.Size(120, 35);
            this.btnTemplateManage.TabIndex = 2;
            this.btnTemplateManage.Text = "模板管理";
            this.btnTemplateManage.UseVisualStyleBackColor = true;
            this.btnTemplateManage.Click += new System.EventHandler(this.btnTemplateManage_Click);
            // 
            // cmbTemplates
            // 
            this.cmbTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTemplates.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbTemplates.FormattingEnabled = true;
            this.cmbTemplates.Location = new System.Drawing.Point(13, 73);
            this.cmbTemplates.Name = "cmbTemplates";
            this.cmbTemplates.Size = new System.Drawing.Size(324, 29);
            this.cmbTemplates.TabIndex = 1;
            this.cmbTemplates.SelectedIndexChanged += new System.EventHandler(this.cmbTemplates_SelectedIndexChanged);
            // 
            // lblTemplate
            // 
            this.lblTemplate.AutoSize = true;
            this.lblTemplate.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTemplate.Location = new System.Drawing.Point(13, 40);
            this.lblTemplate.Name = "lblTemplate";
            this.lblTemplate.Size = new System.Drawing.Size(79, 20);
            this.lblTemplate.TabIndex = 0;
            this.lblTemplate.Text = "选择模板:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audio900 视觉检测系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelCameraDisplay.ResumeLayout(false);
            this.panelCameraDisplay.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.groupBoxResult.ResumeLayout(false);
            this.groupBoxControl.ResumeLayout(false);
            this.groupBoxControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelCameraDisplay;
        private System.Windows.Forms.Label lblNoCamera;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.GroupBox groupBoxControl;
        private System.Windows.Forms.Label lblTemplate;
        private System.Windows.Forms.ComboBox cmbTemplates;
        private System.Windows.Forms.Button btnTemplateManage;
        private System.Windows.Forms.TextBox txtProductSN;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtEmployeeId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ListView listViewSteps;
        private System.Windows.Forms.ColumnHeader columnHeaderStep;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.ColumnHeader columnHeaderScore;
        private System.Windows.Forms.GroupBox groupBoxResult;
        private System.Windows.Forms.Label lblResult;
    }
}

