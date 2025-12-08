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
            this.tlpInputFields = new System.Windows.Forms.TableLayoutPanel();
            this.lblProductSN = new System.Windows.Forms.Label();
            this.txtProductSN = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtViewNo = new System.Windows.Forms.TextBox();
            this.lblEmployeeId = new System.Windows.Forms.Label();
            this.txtEmployeeId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStationNo = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtViewName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
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
            this.cmbTemplates = new System.Windows.Forms.ComboBox();
            this.btnTemplateManage = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.panelStepImage = new System.Windows.Forms.Panel();
            this.pictureBoxStep = new System.Windows.Forms.PictureBox();
            this.lblStepTitle = new System.Windows.Forms.Label();
            this.panelMesStatus = new System.Windows.Forms.Panel();
            this.lblMesStatus = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.tlpInputFields.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelCameraDisplay.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.panelStepImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep)).BeginInit();
            this.panelMesStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpInputFields
            // 
            this.tlpInputFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpInputFields.ColumnCount = 4;
            this.tlpInputFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInputFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpInputFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInputFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpInputFields.Controls.Add(this.lblProductSN, 0, 0);
            this.tlpInputFields.Controls.Add(this.txtProductSN, 1, 0);
            this.tlpInputFields.Controls.Add(this.label2, 2, 0);
            this.tlpInputFields.Controls.Add(this.txtViewNo, 3, 0);
            this.tlpInputFields.Controls.Add(this.lblEmployeeId, 0, 1);
            this.tlpInputFields.Controls.Add(this.txtEmployeeId, 1, 1);
            this.tlpInputFields.Controls.Add(this.label3, 2, 1);
            this.tlpInputFields.Controls.Add(this.txtStationNo, 3, 1);
            this.tlpInputFields.Controls.Add(this.label4, 0, 2);
            this.tlpInputFields.Controls.Add(this.txtViewName, 1, 2);
            this.tlpInputFields.Controls.Add(this.label5, 2, 2);
            this.tlpInputFields.Controls.Add(this.txtFileName, 3, 2);
            this.tlpInputFields.Location = new System.Drawing.Point(5, 50);
            this.tlpInputFields.Name = "tlpInputFields";
            this.tlpInputFields.RowCount = 3;
            this.tlpInputFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpInputFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpInputFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpInputFields.Size = new System.Drawing.Size(305, 105);
            this.tlpInputFields.TabIndex = 1;
            // 
            // lblProductSN
            // 
            this.lblProductSN.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblProductSN.AutoSize = true;
            this.lblProductSN.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblProductSN.Location = new System.Drawing.Point(3, 3);
            this.lblProductSN.Name = "lblProductSN";
            this.lblProductSN.Size = new System.Drawing.Size(88, 28);
            this.lblProductSN.TabIndex = 0;
            this.lblProductSN.Text = "产品SN:";
            // 
            // txtProductSN
            // 
            this.txtProductSN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProductSN.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtProductSN.Location = new System.Drawing.Point(97, 3);
            this.txtProductSN.Name = "txtProductSN";
            this.txtProductSN.Size = new System.Drawing.Size(56, 35);
            this.txtProductSN.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label2.Location = new System.Drawing.Point(159, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 28);
            this.label2.TabIndex = 2;
            this.label2.Text = "治具码:";
            // 
            // txtViewNo
            // 
            this.txtViewNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtViewNo.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtViewNo.Location = new System.Drawing.Point(245, 3);
            this.txtViewNo.Name = "txtViewNo";
            this.txtViewNo.ReadOnly = true;
            this.txtViewNo.Size = new System.Drawing.Size(57, 35);
            this.txtViewNo.TabIndex = 3;
            // 
            // lblEmployeeId
            // 
            this.lblEmployeeId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEmployeeId.AutoSize = true;
            this.lblEmployeeId.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblEmployeeId.Location = new System.Drawing.Point(3, 38);
            this.lblEmployeeId.Name = "lblEmployeeId";
            this.lblEmployeeId.Size = new System.Drawing.Size(81, 28);
            this.lblEmployeeId.TabIndex = 4;
            this.lblEmployeeId.Text = "员工ID:";
            // 
            // txtEmployeeId
            // 
            this.txtEmployeeId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEmployeeId.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtEmployeeId.Location = new System.Drawing.Point(97, 38);
            this.txtEmployeeId.Name = "txtEmployeeId";
            this.txtEmployeeId.Size = new System.Drawing.Size(56, 35);
            this.txtEmployeeId.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label3.Location = new System.Drawing.Point(159, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 28);
            this.label3.TabIndex = 6;
            this.label3.Text = "工站号:";
            // 
            // txtStationNo
            // 
            this.txtStationNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStationNo.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtStationNo.Location = new System.Drawing.Point(245, 38);
            this.txtStationNo.Name = "txtStationNo";
            this.txtStationNo.ReadOnly = true;
            this.txtStationNo.Size = new System.Drawing.Size(57, 35);
            this.txtStationNo.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label4.Location = new System.Drawing.Point(3, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 28);
            this.label4.TabIndex = 8;
            this.label4.Text = "制程:";
            // 
            // txtViewName
            // 
            this.txtViewName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtViewName.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtViewName.Location = new System.Drawing.Point(97, 73);
            this.txtViewName.Name = "txtViewName";
            this.txtViewName.ReadOnly = true;
            this.txtViewName.Size = new System.Drawing.Size(56, 35);
            this.txtViewName.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label5.Location = new System.Drawing.Point(159, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 28);
            this.label5.TabIndex = 10;
            this.label5.Text = "文件名:";
            // 
            // txtFileName
            // 
            this.txtFileName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFileName.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtFileName.Location = new System.Drawing.Point(245, 73);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(57, 35);
            this.txtFileName.TabIndex = 11;
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
            this.panelTop.Size = new System.Drawing.Size(1656, 60);
            this.panelTop.TabIndex = 0;
            // 
            // btnOpenCamera
            // 
            this.btnOpenCamera.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenCamera.BackColor = System.Drawing.Color.White;
            this.btnOpenCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenCamera.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpenCamera.Location = new System.Drawing.Point(1509, 15);
            this.btnOpenCamera.Name = "btnOpenCamera";
            this.btnOpenCamera.Size = new System.Drawing.Size(135, 39);
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
            this.lblCameraStatus.Location = new System.Drawing.Point(1344, 17);
            this.lblCameraStatus.Name = "lblCameraStatus";
            this.lblCameraStatus.Size = new System.Drawing.Size(155, 39);
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
            this.lblImageArea.Size = new System.Drawing.Size(150, 56);
            this.lblImageArea.TabIndex = 0;
            this.lblImageArea.Text = "影像区";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
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
            this.splitContainer1.Size = new System.Drawing.Size(1656, 840);
            this.splitContainer1.SplitterDistance = 1324;
            this.splitContainer1.TabIndex = 1;
            // 
            // panelCameraDisplay
            // 
            this.panelCameraDisplay.BackColor = System.Drawing.Color.Black;
            this.panelCameraDisplay.Controls.Add(this.lblNoCamera);
            this.panelCameraDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCameraDisplay.Location = new System.Drawing.Point(0, 0);
            this.panelCameraDisplay.Name = "panelCameraDisplay";
            this.panelCameraDisplay.Size = new System.Drawing.Size(1324, 803);
            this.panelCameraDisplay.TabIndex = 0;
            // 
            // lblNoCamera
            // 
            this.lblNoCamera.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNoCamera.AutoSize = true;
            this.lblNoCamera.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNoCamera.ForeColor = System.Drawing.Color.White;
            this.lblNoCamera.Location = new System.Drawing.Point(562, 384);
            this.lblNoCamera.Name = "lblNoCamera";
            this.lblNoCamera.Size = new System.Drawing.Size(276, 56);
            this.lblNoCamera.TabIndex = 0;
            this.lblNoCamera.Text = "未检测到相机";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelFPS});
            this.statusStrip1.Location = new System.Drawing.Point(0, 803);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1324, 37);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(1209, 28);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "系统就绪";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelFPS
            // 
            this.toolStripStatusLabelFPS.Name = "toolStripStatusLabelFPS";
            this.toolStripStatusLabelFPS.Size = new System.Drawing.Size(100, 28);
            this.toolStripStatusLabelFPS.Text = "FPS: 0.00";
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.White;
            this.panelRight.Controls.Add(this.cmbTemplates);
            this.panelRight.Controls.Add(this.tlpInputFields);
            this.panelRight.Controls.Add(this.btnTemplateManage);
            this.panelRight.Controls.Add(this.label6);
            this.panelRight.Controls.Add(this.panelStepImage);
            this.panelRight.Controls.Add(this.panelMesStatus);
            this.panelRight.Controls.Add(this.lblResult);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(5);
            this.panelRight.Size = new System.Drawing.Size(328, 840);
            this.panelRight.TabIndex = 0;
            // 
            // cmbTemplates
            // 
            this.cmbTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTemplates.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.cmbTemplates.FormattingEnabled = true;
            this.cmbTemplates.Location = new System.Drawing.Point(5, 10);
            this.cmbTemplates.Name = "cmbTemplates";
            this.cmbTemplates.Size = new System.Drawing.Size(305, 36);
            this.cmbTemplates.TabIndex = 0;
            this.cmbTemplates.SelectedIndexChanged += new System.EventHandler(this.cmbTemplates_SelectedIndexChanged);
            // 
            // btnTemplateManage
            // 
            this.btnTemplateManage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTemplateManage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnTemplateManage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTemplateManage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.btnTemplateManage.ForeColor = System.Drawing.Color.White;
            this.btnTemplateManage.Location = new System.Drawing.Point(5, 165);
            this.btnTemplateManage.Name = "btnTemplateManage";
            this.btnTemplateManage.Size = new System.Drawing.Size(305, 42);
            this.btnTemplateManage.TabIndex = 2;
            this.btnTemplateManage.Text = "模板管理 ▼";
            this.btnTemplateManage.UseVisualStyleBackColor = false;
            this.btnTemplateManage.Click += new System.EventHandler(this.btnTemplateManage_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(5, 215);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(115, 33);
            this.label6.TabIndex = 3;
            this.label6.Text = "作业步骤";
            // 
            // panelStepImage
            // 
            this.panelStepImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelStepImage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelStepImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelStepImage.Controls.Add(this.pictureBoxStep);
            this.panelStepImage.Controls.Add(this.lblStepTitle);
            this.panelStepImage.Location = new System.Drawing.Point(5, 245);
            this.panelStepImage.Name = "panelStepImage";
            this.panelStepImage.Size = new System.Drawing.Size(305, 180);
            this.panelStepImage.TabIndex = 4;
            // 
            // pictureBoxStep
            // 
            this.pictureBoxStep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.pictureBoxStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxStep.Location = new System.Drawing.Point(100, 0);
            this.pictureBoxStep.Name = "pictureBoxStep";
            this.pictureBoxStep.Size = new System.Drawing.Size(203, 178);
            this.pictureBoxStep.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxStep.TabIndex = 1;
            this.pictureBoxStep.TabStop = false;
            // 
            // lblStepTitle
            // 
            this.lblStepTitle.BackColor = System.Drawing.Color.White;
            this.lblStepTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStepTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.lblStepTitle.Location = new System.Drawing.Point(0, 0);
            this.lblStepTitle.Name = "lblStepTitle";
            this.lblStepTitle.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lblStepTitle.Size = new System.Drawing.Size(100, 178);
            this.lblStepTitle.TabIndex = 0;
            this.lblStepTitle.Text = "步骤 1\r\n已配置模板";
            // 
            // panelMesStatus
            // 
            this.panelMesStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMesStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelMesStatus.Controls.Add(this.lblMesStatus);
            this.panelMesStatus.Location = new System.Drawing.Point(5, 420);
            this.panelMesStatus.Name = "panelMesStatus";
            this.panelMesStatus.Size = new System.Drawing.Size(305, 40);
            this.panelMesStatus.TabIndex = 16;
            // 
            // lblMesStatus
            // 
            this.lblMesStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMesStatus.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblMesStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblMesStatus.Location = new System.Drawing.Point(0, 0);
            this.lblMesStatus.Name = "lblMesStatus";
            this.lblMesStatus.Size = new System.Drawing.Size(305, 40);
            this.lblMesStatus.TabIndex = 0;
            this.lblMesStatus.Text = "MES上传等待执行";
            this.lblMesStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblResult
            // 
            this.lblResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.lblResult.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblResult.Font = new System.Drawing.Font("微软雅黑", 36F, System.Drawing.FontStyle.Bold);
            this.lblResult.ForeColor = System.Drawing.Color.White;
            this.lblResult.Location = new System.Drawing.Point(5, 752);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(318, 83);
            this.lblResult.TabIndex = 17;
            this.lblResult.Text = "PASS";
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(0, 0);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 0;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(0, 0);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1656, 900);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelTop);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audio900 视觉检测系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tlpInputFields.ResumeLayout(false);
            this.tlpInputFields.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
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
            this.panelRight.PerformLayout();
            this.panelStepImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep)).EndInit();
            this.panelMesStatus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblImageArea;
        private System.Windows.Forms.Label lblCameraStatus;
        private System.Windows.Forms.Button btnOpenCamera;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelCameraDisplay;
        private System.Windows.Forms.Label lblNoCamera;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFPS;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Label lblProductSN;
        private System.Windows.Forms.TextBox txtProductSN;
        private System.Windows.Forms.Label lblEmployeeId;
        private System.Windows.Forms.TextBox txtEmployeeId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtViewNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStationNo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtViewName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.ComboBox cmbTemplates;
        private System.Windows.Forms.Button btnTemplateManage;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panelStepImage;
        private System.Windows.Forms.Label lblStepTitle;
        private System.Windows.Forms.PictureBox pictureBoxStep;
        private System.Windows.Forms.Panel panelMesStatus;
        private System.Windows.Forms.Label lblMesStatus;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.TableLayoutPanel tlpInputFields;
    }
}

