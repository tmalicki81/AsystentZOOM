using AsystentZOOM.VM.Common;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AsystentZOOM.Finisher.ViewModel
{
    public class TaskVM : BaseVM
    {
        public TaskVM(string taskName, bool isIndeterminate, Action<TaskVM> taskBody)
        {
            _taskName = taskName;
            IsIndeterminate = isIndeterminate;
            _taskBody = taskBody;
        }

        private string? _taskName;
        public string? TaskName
        {
            get => _taskName;
            set => SetValue(ref _taskName, value, nameof(TaskName));
        }

        private TaskStatusEnum _taskStatus = TaskStatusEnum.Queued;
        public TaskStatusEnum TaskStatus
        {
            get => _taskStatus;
            set => SetValue(ref _taskStatus, value, nameof(TaskStatus));
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

        private int? _percentComplette;
        public int? PercentComplette
        {
            get => _percentComplette;
            set => SetValue(ref _percentComplette, value, nameof(PercentComplette));
        }

        private string? _errorText;
        public string? ErrorText
        {
            get => _errorText;
            set => SetValue(ref _errorText, value, nameof(ErrorText));
        }

        private readonly Action<TaskVM> _taskBody;


        private RelayCommand? _taskExecuteCommand = null;
        public RelayCommand TaskExecuteCommand
            => _taskExecuteCommand ??= new RelayCommand(
                async () => await TaskExecute(),
                () => TaskStatus != TaskStatusEnum.InProgress);

        internal async Task TaskExecute()
        {
            TaskStatus = TaskStatusEnum.InProgress;
            DateBegin = DateTime.Now;
            PercentComplette = 0;
            TimeElapsed = TimeSpan.Zero;
            ErrorText = null;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            EventHandler handler = (s, e) => TimeElapsed = DateTime.Now - DateBegin;
            timer.Tick += handler;
            timer.Start();
            try
            {
                await Task.Run(() => _taskBody(this));

                TaskStatus = TaskStatusEnum.Finished;
                PercentComplette = 100;
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                TaskStatus = TaskStatusEnum.Error;
                PercentComplette = null;
            }
            finally
            {
                timer.Stop();
            }
        }
    }
}