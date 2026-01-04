using Cognex.VisionPro;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Audio900.Models
{
    /// <summary>
    /// 步骤参数模型
    /// </summary>
    public class StepParameter : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private bool _isEnabled;
        private double _standardValue;
        private double _tolerance;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public double StandardValue
        {
            get => _standardValue;
            set { _standardValue = value; OnPropertyChanged(nameof(StandardValue)); }
        }

        public double Tolerance
        {
            get => _tolerance;
            set { _tolerance = value; OnPropertyChanged(nameof(Tolerance)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 作业步骤模型
    /// </summary>
    public class WorkStep : INotifyPropertyChanged
    {
        private string _name;
        private string _status;
        private int _timeout;
        private bool _showFailurePrompt;
        private string _failurePromptMessage;
        
        private int _cameraIndex;
        private bool _isParallel;
        private bool _syncNextStepsEnabled;
        private string _syncNextSteps;

        /// <summary>
        /// 是否为AR实时连续检测模式
        /// </summary>
        public bool _isArMode  = false;

        /// <summary>
        /// AR模式下的保持时间（秒）
        /// 例如：用户动作保持正确2秒后，才算该步骤完成，自动跳到下一步
        /// </summary>
        public int _arHoldDuration = 20;

        /// <summary>
        /// 是否需要图像稳定性检查（默认 true）
        /// </summary>
        private bool _requireStability = true;

        public WorkStep()
        {
            Parameters = new BindingList<StepParameter>();
        }

        public int StepNumber { get; set; }

        public BindingList<StepParameter> Parameters { get; set; }

        /// <summary>
        /// 相机索引 (0: 左相机, 1: 右相机, etc.)
        /// </summary>
        public int CameraIndex
        {
            get => _cameraIndex;
            set { _cameraIndex = value; OnPropertyChanged(nameof(CameraIndex)); }
        }

        /// <summary>
        /// 是否与下一步并行执行
        /// </summary>
        public bool IsParallel
        {
            get => _isParallel;
            set { _isParallel = value; OnPropertyChanged(nameof(IsParallel)); }
        }

        public bool SyncNextStepsEnabled
        {
            get => _syncNextStepsEnabled;
            set { _syncNextStepsEnabled = value; OnPropertyChanged(nameof(SyncNextStepsEnabled)); }
        }

        public string SyncNextSteps
        {
            get => _syncNextSteps;
            set { _syncNextSteps = value; OnPropertyChanged(nameof(SyncNextSteps)); }
        }
        
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public int Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                OnPropertyChanged(nameof(Timeout));
            }
        }

        /// <summary>
        /// 是否为AR实时连续检测模式
        /// </summary>
        public bool IsArMode
        {
            get => _isArMode;
            set
            {
                _isArMode = value;
                OnPropertyChanged(nameof(IsArMode));
            }
        }

        /// <summary>
        /// AR模式下的保持时间（秒）
        /// 例如：用户动作保持正确2秒后，才算该步骤完成，自动跳到下一步
        /// </summary>
        public int ArHoldDuration
        {
            get => _arHoldDuration;
            set
            {
                _arHoldDuration = value;
                OnPropertyChanged(nameof(ArHoldDuration));
            }
        }

        /// <summary>
        /// 是否需要图像稳定性检查（默认 true）
        /// </summary>
        public bool RequireStability
        {
            get => _requireStability;
            set
            {
                _requireStability = value;
                OnPropertyChanged(nameof(RequireStability));
            }
        }

        /// <summary>
        /// 是否在检测失败时提示用户
        /// </summary>
        public bool ShowFailurePrompt
        {
            get => _showFailurePrompt;
            set
            {
                _showFailurePrompt = value;
                OnPropertyChanged(nameof(ShowFailurePrompt));
            }
        }

        /// <summary>
        /// 检测失败时的提示内容
        /// </summary>
        public string FailurePromptMessage
        {
            get => _failurePromptMessage;
            set
            {
                _failurePromptMessage = value;
                OnPropertyChanged(nameof(FailurePromptMessage));
            }
        }

        // 存储拍摄的图像
        public ICogImage CapturedImage { get; set; }

        // 用于WinForms显示的图像源 (改为Image类型)
        private Image _imageSource;
        public Image ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }       
        
        // 完成时间
        public DateTime? CompletedTime { get; set; }
        
        // 失败原因
        public string FailureReason { get; set; }     
        
        // VisionPro 工具块文件路径 (.vpp)
        public string ToolBlockPath { get; set; }

        /// <summary>
        /// 算法类型：0=VisionPro模板匹配, 1=深度学习(YOLOv8)
        /// </summary>
        private int _algorithmType;
        public int AlgorithmType
        {
            get => _algorithmType;
            set { _algorithmType = value; OnPropertyChanged(nameof(AlgorithmType)); }
        }

        /// <summary>
        /// 深度学习模型路径 (.onnx)
        /// </summary>
        private string _modelPath;
        public string ModelPath
        {
            get => _modelPath;
            set { _modelPath = value; OnPropertyChanged(nameof(ModelPath)); }
        }

        /// <summary>
        /// 置信度阈值 (0.0 - 1.0)
        /// </summary>
        private double _confidenceThreshold = 0.5;
        public double ConfidenceThreshold
        {
            get => _confidenceThreshold;
            set { _confidenceThreshold = value; OnPropertyChanged(nameof(ConfidenceThreshold)); }
        }

        // 模板图像
        public ICogImage TemplateImage { get; set; }
       
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
