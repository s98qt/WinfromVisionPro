using Audio900.Models;
using Audio900.Services;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
            groupBoxStep.Text = $"作业步骤 {Step.StepNumber}";
            txtTimeout.Text = Step.Timeout.ToString();
            chkShowPrompt.Checked = Step.ShowFailurePrompt;
            txtFailureMessage.Text = Step.FailurePromptMessage;
            
            if (Step.ImageSource != null)
            {
                pictureBoxPreview.Image = Step.ImageSource;
            }
        }

        private void SetupGrid()
        {
            dgvParams.AutoGenerateColumns = false;
            dgvParams.Columns.Clear();
            dgvParams.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "名称", DataPropertyName = "Name", Width = 80, ReadOnly = true });
            dgvParams.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "启用", DataPropertyName = "IsEnabled", Width = 40 });
            dgvParams.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "标准值", DataPropertyName = "StandardValue", Width = 60 });
            dgvParams.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "公差", DataPropertyName = "Tolerance", Width = 60 });
            
            var deleteBtn = new DataGridViewButtonColumn();
            deleteBtn.HeaderText = "操作";
            deleteBtn.Text = "X";
            deleteBtn.UseColumnTextForButtonValue = true;
            deleteBtn.Width = 40;
            dgvParams.Columns.Add(deleteBtn);

            dgvParams.DataSource = Step.Parameters;
            
            dgvParams.CellClick += DgvParams_CellClick;
            
            txtTimeout.TextChanged += (s, e) => { if (int.TryParse(txtTimeout.Text, out int val)) Step.Timeout = val; };
            chkShowPrompt.CheckedChanged += (s, e) => Step.ShowFailurePrompt = chkShowPrompt.Checked;

            // 初始化相机选择
            SetupCameraControl();
            
            // 初始化并行执行选项
            chkParallel.Checked = Step.IsParallel;
            chkParallel.CheckedChanged += (s, e) => Step.IsParallel = chkParallel.Checked;
        }

        private void SetupCameraControl()
        {
            cmbCamera.Items.Clear();
            int count = CameraService.GetCameraCount();
            if (count <= 0) count = 1; // 至少显示一个
            
            for (int i = 0; i < count; i++)
            {
                cmbCamera.Items.Add($"相机 {i + 1}");
            }

            if (Step.CameraIndex >= 0 && Step.CameraIndex < cmbCamera.Items.Count)
            {
                cmbCamera.SelectedIndex = Step.CameraIndex;
            }
            else
            {
                cmbCamera.SelectedIndex = 0;
            }
            
            cmbCamera.SelectedIndexChanged += (s, e) => Step.CameraIndex = cmbCamera.SelectedIndex;
        }

        private void txtFailureMessage_TextChanged(object sender, EventArgs e)
        {
            if (Step != null)
            {
                Step.FailurePromptMessage = txtFailureMessage.Text;
            }
        }

        private void DgvParams_CellClick(object sender, DataGridViewCellEventArgs e)
        {
             if (e.RowIndex >= 0 && e.ColumnIndex == dgvParams.Columns.Count - 1)
             {
                 Step.Parameters.RemoveAt(e.RowIndex);
             }
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
                     // 根据是否为多相机模式选择对应的相机进行采集
                     if (_cameraService.IsMultiCameraMode)
                     {
                         var camera = _cameraService.GetCamera(Step.CameraIndex);
                         if (camera != null)
                         {
                             snapshot = await System.Threading.Tasks.Task.Run(() => camera.CaptureSnapshotAsync());
                         }
                         else
                         {
                             MessageBox.Show($"无效的相机索引: {Step.CameraIndex}");
                             return;
                         }
                     }
                     else
                     {
                         snapshot = await System.Threading.Tasks.Task.Run(() => _cameraService.CaptureSnapshotAsync());
                     }

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
                    if (!(snapshot is CogImage8Grey))
                    {
                        try
                        {
                            CogImageConvertTool convertTool = new CogImageConvertTool();
                            convertTool.InputImage = snapshot;
                            convertTool.Run();
                            if (convertTool.OutputImage != null)
                            {
                                snapshot = convertTool.OutputImage as CogImage8Grey;
                            }
                        }
                        catch {  }
                    }

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
                         
                         try
                         {
                             // 运行一次以获取最新值（标准值）
                             toolBlock.Run();
                             
                             // 收集所有有效的输出参数名称
                             var validOutputNames = new System.Collections.Generic.HashSet<string>();

                             foreach (CogToolBlockTerminal terminal in toolBlock.Outputs)
                             {
                                 if (terminal.Value == null) continue;
                                 
                                 // 只处理数值类型的输出
                                 if (double.TryParse(terminal.Value.ToString(), out double val))
                                 {
                                     validOutputNames.Add(terminal.Name);
                                     
                                     var existingParam = Step.Parameters.FirstOrDefault(p => p.Name == terminal.Name);
                                     if (existingParam != null)
                                     {
                                         // 更新标准值
                                         existingParam.StandardValue = val;
                                     }
                                     else
                                     {
                                         // 自动添加新参数
                                         Step.Parameters.Add(new StepParameter
                                         {
                                             Id = Step.Parameters.Count + 1,
                                             Name = terminal.Name,
                                             IsEnabled = true,
                                             StandardValue = val,
                                             Tolerance = 0.5 // 默认公差
                                         });
                                     }
                                 }
                             }

                             // 删除不再存在的参数（反向遍历删除）
                             for (int i = Step.Parameters.Count - 1; i >= 0; i--)
                             {
                                 if (!validOutputNames.Contains(Step.Parameters[i].Name))
                                 {
                                     Step.Parameters.RemoveAt(i);
                                 }
                             }

                             // 刷新界面
                             // 刷新界面
                             // BindingList 会自动通知 DataGridView 更新，但重置 DataSource 更可靠
                             dgvParams.DataSource = null;
                             dgvParams.DataSource = Step.Parameters;
                             dgvParams.Refresh();
                         }
                         catch (Exception ex)
                         {
                             MessageBox.Show($"同步参数失败: {ex.Message}");
                         }
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
