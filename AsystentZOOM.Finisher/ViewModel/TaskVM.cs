using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Model;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AsystentZOOM.Finisher.ViewModel
{
    public class TaskVM : BaseVM, IProgressInfoVM
    {
        public TaskVM(string operationName, bool isIndeterminate, Action<TaskVM> operationBody)
        {
            OperationName = operationName;
            IsIndeterminate = isIndeterminate;
            _operationBody = operationBody;
        }

        private string? _operationName;
        public string? OperationName
        {
            get => _operationName;
            set => SetValue(ref _operationName, value, nameof(OperationName));
        }

        private OperationStatusEnum _operationStatus = OperationStatusEnum.Queued;
        public OperationStatusEnum OperationStatus
        {
            get => _operationStatus;
            set => SetValue(ref _operationStatus, value, nameof(OperationStatus));
        }

        private DateTime? _dateBegin;
        public DateTime? DateBegin
        {
            get => _dateBegin;
            set => SetValue(ref _dateBegin, value, nameof(DateBegin));
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetValue(ref _isIndeterminate, value, nameof(IsIndeterminate));
        }

        private TimeSpan? _timeElapsed;
        public TimeSpan? TimeElapsed
        {
            get => _timeElapsed;
            set
            {
                if(_timeElapsed != value)
                    SetValue(ref _timeElapsed, value, nameof(TimeElapsed));
            }
        }

        private int? _percentCompletted;
        public int? PercentCompletted
        {
            get => _percentCompletted;
            set => SetValue(ref _percentCompletted, value, nameof(PercentCompletted));
        }

        private string? _taskName;
        public string? TaskName
        {
            get => _taskName;
            set => SetValue(ref _taskName, value, nameof(TaskName));
        }

        private readonly Action<TaskVM> _operationBody;

        private RelayCommand? _taskExecuteCommand = null;
        public RelayCommand TaskExecuteCommand
            => _taskExecuteCommand ??= new RelayCommand(
                async () => await TaskExecute(),
                () => OperationStatus != OperationStatusEnum.InProgress);

        internal async Task TaskExecute()
        {
            OperationStatus = OperationStatusEnum.InProgress;
            DateBegin = DateTime.Now;
            PercentCompletted = 0;
            TimeElapsed = TimeSpan.Zero;
            TaskName = null;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            EventHandler handler = (s, e) => TimeElapsed = DateTime.Now - DateBegin;
            timer.Tick += handler;
            timer.Start();
            try
            {
                await Task.Run(() => _operationBody(this));

                OperationStatus = OperationStatusEnum.Finished;
                PercentCompletted = 100;
            }
            catch (Exception ex)
            {
                TaskName = ex.Message;
                OperationStatus = OperationStatusEnum.Error;
                PercentCompletted = null;
            }
            finally
            {
                timer.Stop();
            }
        }
    }
}