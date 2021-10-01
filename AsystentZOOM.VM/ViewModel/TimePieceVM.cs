using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    public class TimePieceVM : SingletonBaseVM, ILayerVM
    {
        public bool IsVisibleInRelease => true;
        public string LayerName => "Pomiar czasu";

        private readonly Timer _timer;
        private DateTime _start;
        private bool _isRunning;

        public TimePieceVM()
        {
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            EventAggregator.Subscribe<MeetingPointVM>(
                nameof(MeetingPointVM) + "_SourcesChanged", 
                OnSourcesChangedHandler, 
                OnSourcesChangedPredicate);
        }

        private void OnSourcesChangedHandler(MeetingPointVM meetingPoint)
        {
            if (Direction == TimePieceDirectionEnum.Back && 
                ReferencePoint == TimePieceReferencePointEnum.ToSpecificTime)
            {
                int currentLp = meetingPoint.Sources.First(x => x.GetContent() == this).Sorter.Lp;

                BreakTime = TimeSpan.FromMilliseconds(meetingPoint
                   .Sources
                   .Where(x => x.Sorter.Lp > currentLp)
                   .OfType<IMovable>()
                   .Sum(x => x.Duration.TotalMilliseconds));
            }
        }

        private bool OnSourcesChangedPredicate(MeetingPointVM meetingPoint)
            => meetingPoint?.Sources?.Any(x => x.GetContent() == this) == true;

        private DateTime DateTimeFromTimespan(TimeSpan timeSpan) 
            => DateTime.Now.Date.Add(timeSpan);

        private void BBB()
        {
            _timer.Enabled = false;
            lock (this)
            {
                try
                {
                    if (ReferencePoint == TimePieceReferencePointEnum.ToSpecificTimeSpan)
                    {
                        TimerValue = Direction == TimePieceDirectionEnum.Forward
                            ? DateTime.Now - _start + StartingTimeSpan
                            : _start - DateTime.Now + StartingTimeSpan;
                    }
                    else if (ReferencePoint == TimePieceReferencePointEnum.ToSpecificTime)
                    {
                        TimerValue = DateTimeFromTimespan(EndTime) - DateTime.Now - (UseBreak ? BreakTime : TimeSpan.Zero);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    _timer.Enabled = true;
                }
            }

            if (Mode == TimePieceModeEnum.Timer &&
                Direction == TimePieceDirectionEnum.Back &&
                TimerValue <= TimeSpan.Zero)
            {
                StopExecute();
                EventAggregator.Publish($"{typeof(ILayerVM)}_Finished", this);
                return;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainVM.Dispatcher.Invoke(BBB);
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetValue(ref _isEnabled, value, nameof(IsEnabled));
        }

        private bool _useBreak = false;
        public bool UseBreak
        {
            get => _useBreak;
            set => SetValue(ref _useBreak, value, nameof(UseBreak));
        }

        private TimeSpan _timerValue = TimeSpan.FromSeconds(25);
        public TimeSpan TimerValue
        {
            get => _timerValue;
            set
            {
                SetValue(ref _timerValue, value, nameof(TimerValue));
                RaisePropertyChanged(nameof(TimerValueString));
                RaisePropertyChanged(nameof(IsAlert)); 
            }
        }

        private TimePieceReferencePointEnum _referencePoint = TimePieceReferencePointEnum.ToSpecificTimeSpan;
        public TimePieceReferencePointEnum ReferencePoint
        {
            get => _referencePoint;
            set
            {
                SetValue(ref _referencePoint, value, nameof(ReferencePoint));
                if (_isRunning)
                    RestartExecute();
            }
        }

        private TimeSpan _startingTimeSpan = TimeSpan.FromSeconds(30);
        public TimeSpan StartingTimeSpan
        {
            get => _startingTimeSpan;
            set => SetValue(ref _startingTimeSpan, value, nameof(StartingTimeSpan));
        }

        private TimeSpan _endTime = TimeSpan.Parse("18:15:00", CultureInfo.InvariantCulture);
        public TimeSpan EndTime
        {
            get => _endTime;
            set => SetValue(ref _endTime, value, nameof(EndTime));
        }

        private TimeSpan _breakTime = TimeSpan.FromMinutes(5);
        public TimeSpan BreakTime
        {
            get => _breakTime;
            set => SetValue(ref _breakTime, value, nameof(BreakTime));
        }

        public string TimerValueString
            => Mode == TimePieceModeEnum.Timer
                   ? TimerValue.ToString(TimerFormat)
                   : _isRunning 
                        ? DateTime.Now.ToString("HH:mm:ss")
                        : "00:00:00";

        public bool IsAlert
        { 
            get
            {
                bool isAlert = Mode == TimePieceModeEnum.Timer &&
                     Direction == TimePieceDirectionEnum.Back &&
                     TimerValue <= AlertMinTime &&
                     TimerValue > TimeSpan.Zero &&
                     _isRunning;
                EventAggregator.Publish($"{nameof(MainVM)}_IsAlertChanged", isAlert);
                return isAlert;
            }
        }

        private TimeSpan _alertMinTime = TimeSpan.FromSeconds(5);
        public TimeSpan AlertMinTime
        {
            get => _alertMinTime;
            set => SetValue(ref _alertMinTime, value, nameof(AlertMinTime));
        }

        private string _textAbove = "Drodzy bracia, za";
        public string TextAbove
        {
            get => _textAbove;
            set => SetValue(ref _textAbove, value, nameof(TextAbove));
        }

        private string _textBelow = "rozpocznie się program muzyczny";
        public string TextBelow
        {
            get => _textBelow;
            set => SetValue(ref _textBelow, value, nameof(TextBelow));
        }

        private int _textFontSize = 43;
        public int TextFontSize
        {
            get => _textFontSize;
            set => SetValue(ref _textFontSize, value, nameof(TextFontSize));
        }

        private int _clockFontSize = 177;
        public int ClockFontSize
        {
            get => _clockFontSize;
            set => SetValue(ref _clockFontSize, value, nameof(ClockFontSize));
        }

        private Color _textColor = Colors.Orange;
        public Color TextColor
        {
            get => _textColor;
            set => SetValue(ref _textColor, value, nameof(TextColor));
        }

        private Color _clockColor = Colors.Yellow;
        public Color ClockColor
        {
            get => _clockColor;
            set => SetValue(ref _clockColor, value, nameof(ClockColor));
        }

        private Color _clockAlertColor = Colors.Red;
        public Color ClockAlertColor
        {
            get => _clockAlertColor;
            set => SetValue(ref _clockAlertColor, value, nameof(ClockAlertColor));
        }

        private TimePieceModeEnum _mode = TimePieceModeEnum.Timer;
        public TimePieceModeEnum Mode
        {
            get => _mode;
            set
            {
                SetValue(ref _mode, value, nameof(Mode));
                RaisePropertyChanged(nameof(IsAlert));
                if (_isRunning)
                    RestartExecute();
            }
        }

        private TimePieceDirectionEnum _direction = TimePieceDirectionEnum.Back;
        public TimePieceDirectionEnum Direction
        {
            get => _direction;
            set => SetValue(ref _direction, value, nameof(Direction));
        }

        private void SetInterval() 
            => _timer.Interval = _timerFormat == @"HH\:mm\:ss\.fff" ? 50 : 200;

        private string _timerFormat = @"mm\:ss";
        public string TimerFormat
        {
            get => _timerFormat;
            set
            {
                SetValue(ref _timerFormat, value, nameof(TimerFormat));
                SetInterval();
                RaisePropertyChanged(nameof(TimerValueString));
            }
        }

        private int _margins;
        public int Margins
        {
            get => _margins;
            set => SetValue(ref _margins, value, nameof(Margins));
        }

        private BaseMediaFileInfo _backgroundMediaFile;
        public BaseMediaFileInfo BackgroundMediaFile
        {
            get => _backgroundMediaFile;
            set => SetValue(ref _backgroundMediaFile, value, nameof(BackgroundMediaFile));
        }

        [XmlIgnore]
        public BaseMediaFileInfo FileInfo
        {
            get => _fileInfo;
            set => SetValue(ref _fileInfo, value, nameof(FileInfo));
        }
        private BaseMediaFileInfo _fileInfo;

        public RelayCommand PlayCommand
            => _playCommand ?? (_playCommand = new RelayCommand(StartExecute, () => !_isRunning));
        private RelayCommand _playCommand;

        private void StartExecute() 
        {
            _isRunning = true;
            SetInterval();
            _start = DateTime.Now;
            _timer.Start();
            
            PlayCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            RestartCommand.RaiseCanExecuteChanged();
        }

        public RelayCommand StopCommand
            => _stopCommand ?? (_stopCommand = new RelayCommand(StopExecute, () => _isRunning));
        private RelayCommand _stopCommand;

        private void StopExecute()
        {
            lock (this)
            {
                TimerValue = TimeSpan.Zero;
                _timer.Stop();
            }
            _isRunning = false; 
            RaisePropertyChanged(nameof(TimerValueString));
            PlayCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            RestartCommand.RaiseCanExecuteChanged();
        }

        public RelayCommand RestartCommand
            => _restartCommand ?? (_restartCommand = new RelayCommand(RestartExecute, () => _isRunning && Mode == TimePieceModeEnum.Timer));
        private RelayCommand _restartCommand;

        private void RestartExecute()
        {
            StopExecute();
            StartExecute();
        }
    }
}
