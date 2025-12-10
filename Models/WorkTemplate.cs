using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Audio900.Models
{
    /// <summary>
    /// 作业模板模型
    /// </summary>
    public class WorkTemplate : INotifyPropertyChanged
    {
        private string _templateName;
        private string _name;
        private string _formatSN;
        private string _description;
        private string _status;
        private DateTime _createdTime;
        private DateTime _modifiedTime;

        public string TemplateName
        {
            get => _templateName;
            set
            {
                _templateName = value;
                _name = value;
                OnPropertyChanged(nameof(TemplateName));
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Name
        {
            get => _name ?? _templateName;
            set
            {
                _name = value;
                _templateName = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(TemplateName));
            }
        }

        /// <summary>
        /// SN格式
        /// </summary>
        public string FormatSN
        {
            get => _formatSN;
            set
            {
                _formatSN = value;
                OnPropertyChanged(nameof(FormatSN));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
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

        public DateTime CreatedTime
        {
            get => _createdTime;
            set
            {
                _createdTime = value;
                OnPropertyChanged(nameof(CreatedTime));
            }
        }

        public DateTime ModifiedTime
        {
            get => _modifiedTime;
            set
            {
                _modifiedTime = value;
                OnPropertyChanged(nameof(ModifiedTime));
            }
        }

        public ObservableCollection<WorkStep> WorkSteps { get; set; }

        public WorkTemplate()
        {
            WorkSteps = new ObservableCollection<WorkStep>();
            _createdTime = DateTime.Now;
            _modifiedTime = DateTime.Now;
        }

        // 别名属性，用于兼容不同的命名约定
        public ObservableCollection<WorkStep> Steps
        {
            get => WorkSteps;
            set => WorkSteps = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
