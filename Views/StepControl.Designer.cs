
namespace Audio900.Views
{
    partial class StepControl
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxStep = new System.Windows.Forms.GroupBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.btnDeleteStep = new System.Windows.Forms.Button();
            this.btnAddParam = new System.Windows.Forms.Button();
            this.dgvParams = new System.Windows.Forms.DataGridView();
            this.chkShowPrompt = new System.Windows.Forms.CheckBox();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.btnCapture = new System.Windows.Forms.Button();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxStep.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParams)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxStep
            // 
            this.groupBoxStep.Controls.Add(this.tlpMain);
            this.groupBoxStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxStep.Location = new System.Drawing.Point(0, 0);
            this.groupBoxStep.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBoxStep.Name = "groupBoxStep";
            this.groupBoxStep.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBoxStep.Size = new System.Drawing.Size(1467, 438);
            this.groupBoxStep.TabIndex = 0;
            this.groupBoxStep.TabStop = false;
            this.groupBoxStep.Text = "作业步骤 1";
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 642F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.panelLeft, 0, 0);
            this.tlpMain.Controls.Add(this.pictureBoxPreview, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(6, 29);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(1455, 404);
            this.tlpMain.TabIndex = 0;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.btnDeleteStep);
            this.panelLeft.Controls.Add(this.btnAddParam);
            this.panelLeft.Controls.Add(this.dgvParams);
            this.panelLeft.Controls.Add(this.chkShowPrompt);
            this.panelLeft.Controls.Add(this.txtTimeout);
            this.panelLeft.Controls.Add(this.lblTimeout);
            this.panelLeft.Controls.Add(this.btnCapture);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(6, 5);
            this.panelLeft.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(630, 394);
            this.panelLeft.TabIndex = 0;
            // 
            // btnDeleteStep
            // 
            this.btnDeleteStep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteStep.BackColor = System.Drawing.Color.LightCoral;
            this.btnDeleteStep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteStep.Location = new System.Drawing.Point(6, 335);
            this.btnDeleteStep.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btnDeleteStep.Name = "btnDeleteStep";
            this.btnDeleteStep.Size = new System.Drawing.Size(147, 44);
            this.btnDeleteStep.TabIndex = 7;
            this.btnDeleteStep.Text = "删除本步骤";
            this.btnDeleteStep.UseVisualStyleBackColor = false;
            this.btnDeleteStep.Click += new System.EventHandler(this.btnDeleteStep_Click);
            // 
            // btnAddParam
            // 
            this.btnAddParam.BackColor = System.Drawing.Color.LightBlue;
            this.btnAddParam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddParam.Location = new System.Drawing.Point(9, 271);
            this.btnAddParam.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btnAddParam.Name = "btnAddParam";
            this.btnAddParam.Size = new System.Drawing.Size(612, 44);
            this.btnAddParam.TabIndex = 6;
            this.btnAddParam.Text = "+ 增加参数";
            this.btnAddParam.UseVisualStyleBackColor = false;
            // 
            // dgvParams
            // 
            this.dgvParams.AllowUserToAddRows = false;
            this.dgvParams.BackgroundColor = System.Drawing.Color.White;
            this.dgvParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParams.Location = new System.Drawing.Point(9, 140);
            this.dgvParams.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.dgvParams.Name = "dgvParams";
            this.dgvParams.RowHeadersVisible = false;
            this.dgvParams.RowHeadersWidth = 72;
            this.dgvParams.RowTemplate.Height = 23;
            this.dgvParams.Size = new System.Drawing.Size(612, 122);
            this.dgvParams.TabIndex = 5;
            // 
            // chkShowPrompt
            // 
            this.chkShowPrompt.AutoSize = true;
            this.chkShowPrompt.Location = new System.Drawing.Point(9, 105);
            this.chkShowPrompt.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.chkShowPrompt.Name = "chkShowPrompt";
            this.chkShowPrompt.Size = new System.Drawing.Size(225, 25);
            this.chkShowPrompt.TabIndex = 4;
            this.chkShowPrompt.Text = "检测失败时提示用户";
            this.chkShowPrompt.UseVisualStyleBackColor = true;
            // 
            // txtTimeout
            // 
            this.txtTimeout.Location = new System.Drawing.Point(199, 58);
            this.txtTimeout.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new System.Drawing.Size(81, 31);
            this.txtTimeout.TabIndex = 3;
            this.txtTimeout.Text = "2000";
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(9, 61);
            this.lblTimeout.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(178, 21);
            this.lblTimeout.TabIndex = 2;
            this.lblTimeout.Text = "步骤通过超时时长";
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.ForeColor = System.Drawing.Color.White;
            this.btnCapture.Location = new System.Drawing.Point(9, 5);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(235, 46);
            this.btnCapture.TabIndex = 1;
            this.btnCapture.Text = "调用Oumit采集一次";
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.BackColor = System.Drawing.Color.Gainsboro;
            this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(648, 5);
            this.pictureBoxPreview.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(801, 394);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreview.TabIndex = 1;
            this.pictureBoxPreview.TabStop = false;
            this.pictureBoxPreview.DoubleClick += new System.EventHandler(this.pictureBoxPreview_DoubleClick);
            // 
            // StepControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxStep);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "StepControl";
            this.Size = new System.Drawing.Size(1467, 438);
            this.groupBoxStep.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParams)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxStep;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.CheckBox chkShowPrompt;
        private System.Windows.Forms.DataGridView dgvParams;
        private System.Windows.Forms.Button btnAddParam;
        private System.Windows.Forms.Button btnDeleteStep;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
    }
}
