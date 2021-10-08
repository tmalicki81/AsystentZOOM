using AsystentZOOM.VM.Common;

namespace AsystentZOOM.VM.Model
{
    public interface IProgressInfoVM
    {
        bool IsIndeterminate { get; set; }
        string OperationName { get; set; }
        int PercentCompletted { get; set; }
        bool ProgressBarVisibility { get; set; }
        string TaskName { get; set; }
    }

    public class ProgressInfoVM : BaseVM, IProgressInfoVM
    {
        public int PercentCompletted
        {
            get => _percentCompletted;
            set
            {
                SetValue(ref _percentCompletted, value, nameof(PercentCompletted));
                IsIndeterminate = false;
            }
        }
        private int _percentCompletted;

        public string OperationName
        {
            get => _operationName;
            set => SetValue(ref _operationName, value, nameof(OperationName));
        }
        private string _operationName;

        public string TaskName
        {
            get => _taskName;
            set => SetValue(ref _taskName, value, nameof(TaskName));
        }
        private string _taskName;

        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetValue(ref _progressBarVisibility, value, nameof(ProgressBarVisibility));
        }
        private bool _progressBarVisibility;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetValue(ref _isIndeterminate, value, nameof(IsIndeterminate));
        }
        private bool _isIndeterminate;
    }
}
