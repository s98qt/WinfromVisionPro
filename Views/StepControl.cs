using Audio900.Models;
using Audio900.Services;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Audio900.Views
{
    public partial class StepControl : UserControl
    {
        public WorkStep Step { get; private set; }
        private CameraService _cameraService;
        public event EventHandler DeleteRequested;
        private string _templateBasePath;

        public string TemplateBasePath
        {
            get => _templateBasePath;
            set => _templateBasePath = value;
        }

        public StepControl(WorkStep step, CameraService cameraService, string templateBasePath)
        {
            InitializeComponent();
            Step = step;
            _cameraService = cameraService;
            _templateBasePath = templateBasePath;
            
            // Explicitly set size to ensure visibility
            this.Size = new Size(800, 250);
            
            BindData();
            SetupGrid();
        }

        public void RefreshUI()
        {
            BindData();
        }

        private void BindData()
        {
            if (Step == null) return;
            
            lblStepName.Text = $"作业步骤 {Step.StepNumber}";
            groupBoxStep.Text = $"作业步骤 {Step.StepNumber}";
            txtTimeout.Text = Step.Timeout.ToString();
            chkShowPrompt.Checked = Step.ShowFailurePrompt;
            
            if (Step.ImageSource != null)
            {
                pictureBoxPreview.Image = Step.ImageSource;
            }
        }

        private void SetupGrid()
        {
            dgvParams.AutoGenerateColumns = false;
            dgvParams.Columns.Clear();
            dgvParams.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "名称", DataPropertyName = "Name", Width = 80 });
            dgvParams.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "启用", DataPropertyName = "IsEnabled", Width = 40 });
            dgvParams.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "下限", DataPropertyName = "LowerLimit", Width = 60 });
            dgvParams.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "上限", DataPropertyName = "UpperLimit", Width = 60 });
            
            var deleteBtn = new DataGridViewButtonColumn();
            deleteBtn.HeaderText = "操作";
            deleteBtn.Text = "X";
            deleteBtn.UseColumnTextForButtonValue = true;
            deleteBtn.Width = 40;
            dgvParams.Columns.Add(deleteBtn);

            dgvParams.DataSource = Step.Parameters;
            
            dgvParams.CellClick += DgvParams_CellClick;
            btnAddParam.Click += BtnAddParam_Click;
            
            txtTimeout.TextChanged += (s, e) => { if (int.TryParse(txtTimeout.Text, out int val)) Step.Timeout = val; };
            chkShowPrompt.CheckedChanged += (s, e) => Step.ShowFailurePrompt = chkShowPrompt.Checked;
        }

        private void DgvParams_CellClick(object sender, DataGridViewCellEventArgs e)
        {
             if (e.RowIndex >= 0 && e.ColumnIndex == dgvParams.Columns.Count - 1)
             {
                 Step.Parameters.RemoveAt(e.RowIndex);
             }
        }

        private void BtnAddParam_Click(object sender, EventArgs e)
        {
             int nextId = Step.Parameters.Count + 1;
             Step.Parameters.Add(new StepParameter 
             { 
                 Id = nextId, 
                 Name = $"参数{nextId}", 
                 IsEnabled = true, 
                 LowerLimit = 0.5, 
                 UpperLimit = 1.0 
             });
        }

        private async void btnCapture_Click(object sender, EventArgs e)
        {
            ICogImage snapshot = null;
            bool isOfflineMode = false;
            
            try 
            {
                if (_cameraService == null)
                {
                    isOfflineMode = true;
                }
                else
                {
                     snapshot = await System.Threading.Tasks.Task.Run(() => _cameraService.CaptureSnapshotAsync());
                     if (snapshot == null)
                     {
                         if (MessageBox.Show("相机采集失败，是否使用离线图片？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                         {
                             isOfflineMode = true;
                         }
                         else return;
                     }
                }
                
                if (isOfflineMode)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Image Files|*.bmp;*.jpg;*.png;*.tif";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            CogImageFileTool tool = new CogImageFileTool();
                            tool.Operator.Open(ofd.FileName, CogImageFileModeConstants.Read);
                            tool.Run();
                            snapshot = tool.OutputImage;
                        }
                        else return;
                    }
                }
                
                if (snapshot != null)
                {
                    Step.CapturedImage = snapshot;
                    
                    using(var bmp = snapshot.ToBitmap())
                    {
                        Step.ImageSource = new Bitmap(bmp);
                    }
                    pictureBoxPreview.Image = Step.ImageSource;
                    
                    MessageBox.Show($"采集成功！模式: {(isOfflineMode ? "离线" : "相机")}");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"采集出错: {ex.Message}");
            }
        }
        
        private void btnDeleteStep_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除此步骤吗？", "确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void pictureBoxPreview_DoubleClick(object sender, EventArgs e)
        {
            if (Step == null) return;
            
            try
            {
                if (string.IsNullOrEmpty(_templateBasePath))
                {
                    MessageBox.Show("请先在顶部输入模板名称，以确定保存路径。");
                    return;
                }

                string vppPath = Step.ToolBlockPath;
                string stepFolder = Path.Combine(_templateBasePath, $"Step{Step.StepNumber}");
                if (!Directory.Exists(stepFolder)) Directory.CreateDirectory(stepFolder);
                
                if (string.IsNullOrEmpty(vppPath))
                {
                     vppPath = Path.Combine(stepFolder, "task.vpp");
                     Step.ToolBlockPath = vppPath;
                }
                
                CogToolBlock toolBlock = null;
                if (File.Exists(vppPath))
                {
                    try 
                    {
                        toolBlock = CogSerializer.LoadObjectFromFile(vppPath) as CogToolBlock;
                    } 
                    catch { }
                }
                
                if (toolBlock == null)
                {
                    toolBlock = new CogToolBlock();
                    toolBlock.Name = $"Step{Step.StepNumber}_Task";
                }
                
                if (Step.CapturedImage != null)
                {
                    if (!toolBlock.Inputs.Contains("InputImage"))
                        toolBlock.Inputs.Add(new CogToolBlockTerminal("InputImage", typeof(ICogImage)));
                        
                    toolBlock.Inputs["InputImage"].Value = Step.CapturedImage;
                }
                
                using (Form editorForm = new Form())
                {
                    editorForm.Text = $"编辑步骤 {Step.StepNumber}";
                    editorForm.WindowState = FormWindowState.Maximized;
                    
                    var editControl = new CogToolBlockEditV2();
                    editControl.Dock = DockStyle.Fill;
                    editControl.Subject = toolBlock;
                    editorForm.Controls.Add(editControl);
                    
                    editorForm.FormClosed += (s, args) => 
                    {
                         CogSerializer.SaveObjectToFile(toolBlock, vppPath);
                         editControl.Dispose();
                    };
                    
                    editorForm.ShowDialog();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"打开编辑器失败: {ex.Message}");
            }
        }
    }
}
