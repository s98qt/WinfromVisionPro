namespace Audio900.Views
{
    partial class CreateTemplateWindow
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtProductSN = new System.Windows.Forms.TextBox();
            this.lblProductSN = new System.Windows.Forms.Label();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.lblTemplateName = new System.Windows.Forms.Label();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.groupBoxSteps = new System.Windows.Forms.GroupBox();
            this.flpSteps = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddStep = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.panelCenter.SuspendLayout();
            this.groupBoxSteps.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.groupBoxInfo);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10);
            this.panelTop.Size = new System.Drawing.Size(1008, 110);
            this.panelTop.TabIndex = 0;
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.txtDescription);
            this.groupBoxInfo.Controls.Add(this.lblDescription);
            this.groupBoxInfo.Controls.Add(this.txtProductSN);
            this.groupBoxInfo.Controls.Add(this.lblProductSN);
            this.groupBoxInfo.Controls.Add(this.txtTemplateName);
            this.groupBoxInfo.Controls.Add(this.lblTemplateName);
            this.groupBoxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInfo.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxInfo.Location = new System.Drawing.Point(10, 10);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(988, 90);
            this.groupBoxInfo.TabIndex = 0;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "建立作业模板";
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtDescription.Location = new System.Drawing.Point(110, 60);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(850, 23);
            this.txtDescription.TabIndex = 5;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblDescription.Location = new System.Drawing.Point(20, 63);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(80, 17);
            this.lblDescription.TabIndex = 4;
            this.lblDescription.Text = "作业模板说明";
            // 
            // txtProductSN
            // 
            this.txtProductSN.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtProductSN.Location = new System.Drawing.Point(460, 25);
            this.txtProductSN.Name = "txtProductSN";
            this.txtProductSN.Size = new System.Drawing.Size(200, 23);
            this.txtProductSN.TabIndex = 3;
            // 
            // lblProductSN
            // 
            this.lblProductSN.AutoSize = true;
            this.lblProductSN.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblProductSN.Location = new System.Drawing.Point(350, 28);
            this.lblProductSN.Name = "lblProductSN";
            this.lblProductSN.Size = new System.Drawing.Size(96, 17);
            this.lblProductSN.TabIndex = 2;
            this.lblProductSN.Text = "匹配产品SN格式";
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtTemplateName.Location = new System.Drawing.Point(110, 25);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(200, 23);
            this.txtTemplateName.TabIndex = 1;
            this.txtTemplateName.TextChanged += new System.EventHandler(this.txtTemplateName_TextChanged);
            // 
            // lblTemplateName
            // 
            this.lblTemplateName.AutoSize = true;
            this.lblTemplateName.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblTemplateName.Location = new System.Drawing.Point(20, 28);
            this.lblTemplateName.Name = "lblTemplateName";
            this.lblTemplateName.Size = new System.Drawing.Size(80, 17);
            this.lblTemplateName.TabIndex = 0;
            this.lblTemplateName.Text = "作业模板名称";
            // 
            // panelCenter
            // 
            this.panelCenter.Controls.Add(this.groupBoxSteps);
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCenter.Location = new System.Drawing.Point(0, 110);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Padding = new System.Windows.Forms.Padding(10);
            this.panelCenter.Size = new System.Drawing.Size(1008, 540);
            this.panelCenter.TabIndex = 1;
            // 
            // groupBoxSteps
            // 
            this.groupBoxSteps.Controls.Add(this.flpSteps);
            this.groupBoxSteps.Controls.Add(this.btnAddStep);
            this.groupBoxSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSteps.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.groupBoxSteps.Location = new System.Drawing.Point(10, 10);
            this.groupBoxSteps.Name = "groupBoxSteps";
            this.groupBoxSteps.Size = new System.Drawing.Size(988, 520);
            this.groupBoxSteps.TabIndex = 0;
            this.groupBoxSteps.TabStop = false;
            this.groupBoxSteps.Text = "作业步骤配置";
            // 
            // flpSteps
            // 
            this.flpSteps.AutoScroll = true;
            this.flpSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSteps.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpSteps.Location = new System.Drawing.Point(3, 22);
            this.flpSteps.Name = "flpSteps";
            this.flpSteps.Size = new System.Drawing.Size(982, 455);
            this.flpSteps.TabIndex = 0;
            this.flpSteps.WrapContents = false;
            // 
            // btnAddStep
            // 
            this.btnAddStep.BackColor = System.Drawing.Color.White;
            this.btnAddStep.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnAddStep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddStep.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnAddStep.Location = new System.Drawing.Point(3, 477);
            this.btnAddStep.Name = "btnAddStep";
            this.btnAddStep.Size = new System.Drawing.Size(982, 40);
            this.btnAddStep.TabIndex = 1;
            this.btnAddStep.Text = "+ 增加步骤";
            this.btnAddStep.UseVisualStyleBackColor = false;
            this.btnAddStep.Click += new System.EventHandler(this.btnAddStep_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnCreate);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 650);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1008, 80);
            this.panelBottom.TabIndex = 2;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Gray;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(520, 20);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 40);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreate.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btnCreate.ForeColor = System.Drawing.Color.White;
            this.btnCreate.Location = new System.Drawing.Point(340, 20);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(150, 40);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "建立作业模板";
            this.btnCreate.UseVisualStyleBackColor = false;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // CreateTemplateWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "CreateTemplateWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "建立作业模板";
            this.panelTop.ResumeLayout(false);
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.panelCenter.ResumeLayout(false);
            this.groupBoxSteps.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtProductSN;
        private System.Windows.Forms.Label lblProductSN;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Label lblTemplateName;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.GroupBox groupBoxSteps;
        private System.Windows.Forms.FlowLayoutPanel flpSteps;
        private System.Windows.Forms.Button btnAddStep;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCreate;
    }
}