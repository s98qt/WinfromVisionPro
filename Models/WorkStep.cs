using Cognex.VisionPro;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Collections.ObjectModel;

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
        private double _lowerLimit;
        private double _upperLimit;

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

        public double LowerLimit
        {
            get => _lowerLimit;
            set { _lowerLimit = value; OnPropertyChanged(nameof(LowerLimit)); }
        }

        public double UpperLimit
        {
            get => _upperLimit;
            set { _upperLimit = value; OnPropertyChanged(nameof(UpperLimit)); }
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
        private double _scoreThreshold;
        private int _timeout;
        private bool _showFailurePrompt;
        private string _failurePromptMessage;
        
        // 新增属性
        private bool _syncNextStepsEnabled;
        private string _syncNextSteps;

        public WorkStep()
        {
            Parameters = new ObservableCollection<StepParameter>();
            // 默认添加两个示例参数
            Parameters.Add(new StepParameter { Id = 1, Name = "匹配参数", IsEnabled = true, LowerLimit = 0.5, UpperLimit = 1.0 });
            Parameters.Add(new StepParameter { Id = 2, Name = "量测参数", IsEnabled = true, LowerLimit = 0.5, UpperLimit = 1.0 });
        }

        public int StepNumber { get; set; }

        public ObservableCollection<StepParameter> Parameters { get; set; }

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

        public double ScoreThreshold
        {
            get => _scoreThreshold;
            set
            {
                _scoreThreshold = value;
                OnPropertyChanged(nameof(ScoreThreshold));
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
        
        // 实际匹配分数
        public double ActualScore { get; set; }
        
        // 完成时间
        public DateTime? CompletedTime { get; set; }
        
        // 失败原因
        public string FailureReason { get; set; }     
        
        // VisionPro 工具块文件路径 (.vpp)
        public string ToolBlockPath { get; set; }
        
        // 模板图像（ROI裁剪后的图像）
        public ICogImage TemplateImage { get; set; }
        
        // 搜索区域
        public ICogRegion SearchRegion { get; set; }
        
        // 模板区域
        public ICogRegion ModelRegion { get; set; }
        
        // 模板图像文件路径
        public string TemplateImagePath { get; set; }
        
        // 原始图像文件路径
        public string OriginalImagePath { get; set; } 

        // 基准直线参数
        public bool HasBenchmark { get; set; }
        /// <summary>
        /// 基准类型：Line, Circle, Point 等
        /// </summary>
        public string BenchmarkType { get; set; }
        public double BenchmarkRotation { get; set; } // 直线角度
        public double BenchmarkX { get; set; }        // 直线原点 X / 圆心 X
        public double BenchmarkY { get; set; }        // 直线原点 Y / 圆心 Y
        public double BenchmarkRadius { get; set; }   // 圆半径

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
