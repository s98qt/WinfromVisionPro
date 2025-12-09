using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Audio900.Models;
using Audio900.Services;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;

namespace Audio900.Views
{
    public partial class CreateTemplateWindow : Form
    {
        public WorkTemplate CreatedTemplate { get; private set; }
        private CameraService _cameraService;
        private string _templatePath;
        private List<WorkStep> _tempSteps = new List<WorkStep>();

        public CreateTemplateWindow(CameraService cameraService, WorkTemplate templateToEdit = null)
        {
            InitializeComponent();
            _cameraService = cameraService;

            if (templateToEdit != null)
            {
                LoadTemplateData(templateToEdit);
                this.Text = $"编辑作业模板 - {templateToEdit.TemplateName}";
                btnCreate.Text = "保存修改";
            }
            else
            {
                // Default 2 steps
                AddStep();
                AddStep();
            }
        }

        private void LoadTemplateData(WorkTemplate template)
        {
            txtTemplateName.Text = template.TemplateName;
            txtProductSN.Text = template.ProductSN;
            txtDescription.Text = template.Description;
            UpdateTemplatePath();

            // Clear default steps
            _tempSteps.Clear();
            flpSteps.Controls.Clear();

            foreach (var step in template.WorkSteps)
            {
                // Clone step
                var newStep = new WorkStep
                {
                    StepNumber = step.StepNumber,
                    ScoreThreshold = step.ScoreThreshold,
                    Timeout = step.Timeout,
                    ShowFailurePrompt = step.ShowFailurePrompt,
                    Status = step.Status,
                    ToolBlockPath = step.ToolBlockPath,
                    CapturedImage = step.CapturedImage
                };
                
                // Copy image source
                if (step.ImageSource != null)
                {
                    newStep.ImageSource = (Image)step.ImageSource.Clone();
                }
                else if (step.CapturedImage != null)
                {
                     try {
                         using(var bmp = step.CapturedImage.ToBitmap())
                         {
                             newStep.ImageSource = new Bitmap(bmp);
                         }
                     } catch {}
                }

                // Copy params
                foreach(var p in step.Parameters)
                {
                    newStep.Parameters.Add(new StepParameter { 
                        Id=p.Id, Name=p.Name, IsEnabled=p.IsEnabled,
                        StandardValue = p.StandardValue,
                        Tolerance = p.Tolerance
                    });
                }
                
                // If image missing in object but exists in file (common on reload)
                if (newStep.ImageSource == null && !string.IsNullOrEmpty(_templatePath))
                {
                    string imgPath = Path.Combine(_templatePath, $"step{step.StepNumber}_image.bmp");
                    if (File.Exists(imgPath))
                    {
                        try {
                            CogImageFileTool imgTool = new CogImageFileTool();
                            imgTool.Operator.Open(imgPath, CogImageFileModeConstants.Read);
                            imgTool.Run();
                            newStep.CapturedImage = imgTool.OutputImage;
                             using(var bmp = newStep.CapturedImage.ToBitmap())
                             {
                                newStep.ImageSource = new Bitmap(bmp);
                             }
                        } catch {}
                    }
                }

                AddStep(newStep);
            }
        }

        private void UpdateTemplatePath()
        {
            string name = txtTemplateName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                _templatePath = "";
                return;
            }
            string safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HomeworkTemplate", safeName);
            
            // Update existing controls
            foreach(StepControl c in flpSteps.Controls)
            {
                c.TemplateBasePath = _templatePath;
            }
        }

        private void txtTemplateName_TextChanged(object sender, EventArgs e)
        {
            UpdateTemplatePath();
        }

        private void btnAddStep_Click(object sender, EventArgs e)
        {
            AddStep();
        }

        private void AddStep(WorkStep step = null)
        {
            if (step == null)
            {
                step = new WorkStep
                {
                    StepNumber = _tempSteps.Count + 1,
                    ScoreThreshold = 0.8,
                    Timeout = 2000,
                    ShowFailurePrompt = false
                };
            }
            
            _tempSteps.Add(step);
            
            // Ensure template path is set on new controls
            if (string.IsNullOrEmpty(_templatePath)) UpdateTemplatePath();

            var control = new StepControl(step, _cameraService, _templatePath);
            control.Width = flpSteps.Width - 25; 
            control.Visible = true; // Ensure visible
            control.DeleteRequested += StepControl_DeleteRequested;
            flpSteps.Controls.Add(control);
            flpSteps.ScrollControlIntoView(control);
        }

        private void StepControl_DeleteRequested(object sender, EventArgs e)
        {
            var control = sender as StepControl;
            if (control != null)
            {
                flpSteps.Controls.Remove(control);
                _tempSteps.Remove(control.Step);
                control.Dispose();
                
                // Renumber
                int index = 1;
                foreach(StepControl c in flpSteps.Controls)
                {
                    c.Step.StepNumber = index++;
                    c.RefreshUI(); // Update labels
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                MessageBox.Show("请输入模板名称"); return;
            }
            if (_tempSteps.Count == 0)
            {
                MessageBox.Show("请至少添加一个步骤"); return;
            }

            CreatedTemplate = new WorkTemplate
            {
                TemplateName = txtTemplateName.Text,
                ProductSN = txtProductSN.Text,
                Description = txtDescription.Text,
                Status = "已创建"
            };
            
            foreach(var s in _tempSteps) CreatedTemplate.WorkSteps.Add(s);
            
            // Save images to disk
            if (!string.IsNullOrEmpty(_templatePath))
            {
                if (!Directory.Exists(_templatePath)) Directory.CreateDirectory(_templatePath);
                
                foreach(var step in _tempSteps)
                {
                    if (step.CapturedImage != null)
                    {
                        try {
                            CogImageFileTool saveTool = new CogImageFileTool();
                            saveTool.InputImage = step.CapturedImage;
                            saveTool.Operator.Open(Path.Combine(_templatePath, $"step{step.StepNumber}_image.bmp"), CogImageFileModeConstants.Write);
                            saveTool.Run();
                        } catch(Exception ex) {
                            MessageBox.Show($"保存步骤{step.StepNumber}图片失败: {ex.Message}");
                        }
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
