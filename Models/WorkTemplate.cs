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
        private string _productSN;
        private string _description;
        private string _status;

        public string TemplateName
        {
            get => _templateName;
            set
            {
                _templateName = value;
                OnPropertyChanged(nameof(TemplateName));
            }
        }

        public string ProductSN
        {
            get => _productSN;
            set
            {
                _productSN = value;
                OnPropertyChanged(nameof(ProductSN));
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

        public ObservableCollection<WorkStep> WorkSteps { get; set; }

        public WorkTemplate()
        {
            WorkSteps = new ObservableCollection<WorkStep>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
