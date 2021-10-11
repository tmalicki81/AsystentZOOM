using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.AudioRecording;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Common.Sortable;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Interfaces.Sortable;
using AsystentZOOM.VM.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    public interface IMeetingPointVM : IBaseVM, ISortableItemVM, IDisposable
    {
        IRelayCommand AddIndentCommand { get; }
        IRelayCommand<bool> AddSourcesCommand { get; }
        IAudioRecordingProvider AudioRecording { get; set; }
        bool BeforeTimeout { get; }
        IRelayCommand ChangeExpandedCommand { get; }
        IRelayCommand ClearCurrentCommand { get; }
        IRelayCommand ClearSourcesCommand { get; }
        bool ContinueOnNext { get; set; }
        TimeSpan Duration { get; set; }
        int Indent { get; set; }
        bool IsCurrent { get; set; }
        bool IsExpanded { get; set; }
        IMeetingVM Meeting { get; set; }
        IRelayCommand OpenWwwCommand { get; }
        IParametersCollectionVM ParameterList { get; set; }
        IRelayCommand PlayAllCommand { get; }
        string PointTitle { get; set; }
        TimeSpan Position { get; set; }
        IRelayCommand RemoveIndentCommand { get; }
        IRelayCommand SetAsCurrentCommand { get; }
        IBaseMediaFileInfo Source { get; set; }
        ObservableCollection<IBaseMediaFileInfo> Sources { get; set; }
        bool Timeout { get; }
        Color TitleColor { get; set; }
        string WebAddress { get; set; }
        ISortableItemVM[] GetDataFromFileDrop(DragEventArgs e);
        void SetOrder();
    }

    [Serializable]
    public class MeetingPointVM : BaseVM, ISortableItemVM, IDisposable, IMeetingPointVM
    {
        public class SortableMeetingPointProvider : SortableItemProvider<MeetingPointVM>
        {
            public SortableMeetingPointProvider(MeetingPointVM parameter) : base(parameter) { }
            public override string ItemCategory => "Punkt";
            public override string ItemName => Item.PointTitle;
            public override bool CanCreateNewItem => true;
            public override ObservableCollection<MeetingPointVM> ContainerItemsSource => (Item.Meeting as MeetingVM)?.MeetingPointList;
            public override MeetingPointVM NewItem() => new MeetingPointVM
            {
                Meeting = Item.Meeting,
                TitleColor = Item.TitleColor
            };
            public override MeetingPointVM SelectedItem
            {
                get => ContainerItemsSource.FirstOrDefault(x => x.Sorter.IsSelected);
                set
                {
                    foreach (var d in ContainerItemsSource)
                        d.Sorter.IsSelected = d == value;
                }
            }
            public override object Container
            {
                get => Item.Meeting;
                set => Item.Meeting = (MeetingVM)value;
            }
        }

        [Serializable]
        public class MeetingPointAudioRecordingProvider : AudioRecordingProvider
        {
            internal MeetingPointVM _meetingPoint;

            public override bool IsReady
                => _meetingPoint != null;

            public override string Title
                => _meetingPoint.PointTitle;
        }

        public MeetingPointAudioRecordingProvider AudioRecording
        {
            get => _audioRecording;
            set => SetValue(ref _audioRecording, value, nameof(AudioRecording));
        }
        private MeetingPointAudioRecordingProvider _audioRecording;

        public MeetingPointVM()
        {
            Sorter = new SortableMeetingPointProvider(this);
            AudioRecording = new MeetingPointAudioRecordingProvider();
            EventAggregator.Subscribe<ILayerVM>($"{typeof(ILayerVM)}_Finished", OnNext, CanNext);

            _changePositionTimer = new Timer(200);
            _changePositionTimer.Elapsed += _changePositionTimer_Elapsed;
        }

        [XmlIgnore]
        public SortableMeetingPointProvider Sorter
        {
            get => _sorter;
            set => SetValue(ref _sorter, value, nameof(Sorter));
        }
        private SortableMeetingPointProvider _sorter;

        ISortableItemProvider ISortableItemVM.Sorter => Sorter;

        private bool CanNext(ILayerVM content)
            => Sources.Any(x => x == content.FileInfo);

        private void OnNext(ILayerVM content)
        {
            if (!ContinueOnNext)
            {
                Meeting.StopAllCommand.Execute(true);
                return;
            }
            var previous = Source;
            previous.IsPlaying = false;
            var next = Sources.FirstOrDefault(x => x.Sorter.Lp == previous.Sorter.Lp + 1);
            if (next != null)
                next.PlayCommand.Execute();
            else
            {
                ContinueOnNext = false;
                Meeting.StopAllCommand.Execute(true);
            }
        }

        public ISortableItemVM[] GetDataFromFileDrop(DragEventArgs e)
        {
            if (e.Data.GetData("FileDrop") is string[] fileList)
            {
                return GetSourcesFromLocal(fileList).ToArray();
            }
            if (e.Data.GetData("HTML Format") != null &&
                e.Data.GetData("Text") is string fileAddress)
            {
                string fileName = fileAddress.Split('/').Last();
                var v = BaseMediaFileInfo.GetFileExtensionConfig(fileName);
                string destDirectory = v?.MediaLocalFileRepository.RootDirectory;

                if (!string.IsNullOrEmpty(destDirectory))
                {
                    if (!Directory.Exists(destDirectory))
                        Directory.CreateDirectory(destDirectory);
                    string fullFileName = Path.Combine(destDirectory, $"{Guid.NewGuid()}.{v.Extension}");
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(fileAddress, fullFileName);
                    }

                    var newMediaFileInfo = BaseMediaFileInfo.Factory.Create(this, fileName, fileAddress);
                    newMediaFileInfo.FillMetadata();
                    return new ISortableItemVM[] { newMediaFileInfo };
                }
                else
                {
                    return new ISortableItemVM[] { };
                }
            }
            return null;
        }

        [XmlIgnore]
        public bool ContinueOnNext
        {
            get => _continueOnNext;
            set => SetValue(ref _continueOnNext, value, nameof(ContinueOnNext));
        }
        private bool _continueOnNext;

        private string _pointTitle = "Nowy punkt";
        public string PointTitle
        {
            get => _pointTitle;
            set
            {
                SetValue(ref _pointTitle, value, nameof(PointTitle));
                ChangeFromChild(this);
            }
        }

        private int _indent;
        public int Indent
        {
            get => _indent;
            set
            {
                SetValue(ref _indent, value, nameof(Indent));
                ChangeFromChild(this);
            }
        }

        private TimeSpan _duration = TimeSpan.FromMinutes(5);
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                SetValue(ref _duration, value, nameof(Duration));
                ChangeFromChild(this);
            }
        }

        private TimeSpan _position;
        public TimeSpan Position
        {
            get => _position;
            set
            {
                SetValue(ref _position, value, nameof(Position));
                RaisePropertyChanged(nameof(Timeout));
                RaisePropertyChanged(nameof(BeforeTimeout));
            }
        }

        private Color _titleColor = Colors.LightGray;
        public Color TitleColor
        {
            get => _titleColor;
            set
            {
                SetValue(ref _titleColor, value, nameof(TitleColor));
                ChangeFromChild(this);
            }
        }

        private string _webAddress;
        public string WebAddress
        {
            get => _webAddress;
            set
            {
                SetValue(ref _webAddress, value, nameof(WebAddress));
                ChangeFromChild(this);
            }
        }

        [XmlIgnore]
        [Parent(typeof(MeetingVM))]
        public IMeetingVM Meeting
        {
            get => _meeting;
            set
            {
                SetValue(ref _meeting, value, nameof(Meeting));
                int lp = 1;
                foreach (var s in Sources)
                {
                    s.MeetingPoint = this;
                    s.Sorter.Lp = lp++;
                }
                EventAggregator.Publish(nameof(MeetingPointVM) + "_SourcesChanged", this);
                if (ParameterList == null)
                    ParameterList = new ParametersCollectionVM { Owner = value };

                AudioRecording._meetingPoint = this;
                AudioRecording.OnCommandExecuted -= Meeting.AudioRecording_OnCommandExecuted;
                AudioRecording.OnCommandExecuted += Meeting.AudioRecording_OnCommandExecuted;
            }
        }
        private IMeetingVM _meeting;

        private ParametersCollectionVM _parameterList = new();
        public ParametersCollectionVM ParameterList
        {
            get => _parameterList;
            set => SetValue(ref _parameterList, value, nameof(ParameterList));
        }

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                SetValue(ref _isExpanded, value, nameof(IsExpanded));
                ChangeFromChild(this);
            }
        }

        public override void ChangeFromChild(IBaseVM child)
            => Meeting?.ChangeFromChild(this);

        [XmlIgnore]
        public BaseMediaFileInfo Source
        {
            get => _source;
            set
            {
                SetValue(ref _source, value, nameof(Source));
                if (Sources == null) return;
                foreach (var item in Sources)
                    item.Sorter.IsSelected = item == value;
            }
        }
        private BaseMediaFileInfo _source = null;

        private ObservableCollection<BaseMediaFileInfo> _sources = new();
        public ObservableCollection<BaseMediaFileInfo> Sources
        {
            get => _sources;
            set
            {
                SetValue(ref _sources, value, nameof(Sources));
                EventAggregator.Publish(nameof(MeetingPointVM) + "_SourcesChanged", this);
            }
        }

        public void SetOrder()
        {
            int lp = 0;
            foreach (var source in Sources)
                source.Sorter.Lp = ++lp;
            Sources = Sources;
        }

        private RelayCommand _clearSourcesCommand;
        public RelayCommand ClearSourcesCommand
            => _clearSourcesCommand ??= new RelayCommand(ClearSources, () => Sources?.Any() == true);

        private void ClearSources()
        {
            var dr = DialogHelper.ShowMessageBox("Czy na pewno wyczyścić playlistę?", "Playlista",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (dr == MessageBoxResult.Yes)
            {
                Sources = new ObservableCollection<BaseMediaFileInfo>();
            }
        }

        private RelayCommand _playAllCommand;
        public RelayCommand PlayAllCommand
            => _playAllCommand ??= new RelayCommand(PlayAll, () => Sources?.Any() == true);
        private void PlayAll()
        {
            ContinueOnNext = true;
            Sources.First().PlayCommand.Execute();
        }

        private RelayCommand _OpenWwwCommand;
        public RelayCommand OpenWwwCommand
            => _OpenWwwCommand ??= new RelayCommand(OpenWww, () => !string.IsNullOrEmpty(WebAddress));
        private void OpenWww()
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = WebAddress
            });
        }

        private RelayCommand _changeExpandedCommand;
        public RelayCommand ChangeExpandedCommand
            => _changeExpandedCommand ??= new RelayCommand(ChangeExpanded);

        private void ChangeExpanded()
            => IsExpanded = !IsExpanded;

        private RelayCommand _addIndentCommand;
        public RelayCommand AddIndentCommand
            => _addIndentCommand ??= new RelayCommand(() => Indent = Indent + 1);

        private RelayCommand _RemoveIndentCommand;
        public RelayCommand RemoveIndentCommand
            => _RemoveIndentCommand ??= new RelayCommand(() => Indent = Indent - 1, () => Indent > 0);

        private RelayCommand<bool> _addSourcesCommand;
        public RelayCommand<bool> AddSourcesCommand
            => _addSourcesCommand ??= new RelayCommand<bool>(AddSourcesFromLocal, (p) => true);

        public const string OpenFileDialogTitle = "Wybierz pliki do prezentowania";
        public const string OpenFileDialogFilter = "Wszystkie multimedia|*.mp3;*.mp4;*.jpg;*.tim;*.bcg|Pliki video|*.mp4|Obrazy|*.jpg|Pliki audio|*.mp3|Konfiguracja zegara|*.tim|Tło (gradient)|*.bcg";

        private List<BaseMediaFileInfo> GetSourcesFromLocal(string[] fileNames)
        {
            var result = new List<BaseMediaFileInfo>();
            foreach (string fullFileName in fileNames)
            {
                var fi = new FileInfo(fullFileName);
                string shortFileName = fi.Name;

                var v = BaseMediaFileInfo.GetFileExtensionConfig(fullFileName);
                string correctFileDir = v.MediaLocalFileRepository.RootDirectory;
                if (string.IsNullOrEmpty(correctFileDir))
                {
                    DialogHelper.ShowMessageBar($"Nieobsługiwany format pliku {v.Extension} dla pliku {fullFileName}");
                    continue;
                }
                string correctFullFileName = Path.Combine(correctFileDir, shortFileName);
                if (fullFileName != correctFullFileName)
                {
                    if (!Directory.Exists(correctFileDir))
                        Directory.CreateDirectory(correctFileDir);
                    var dd = DialogHelper.ShowMessageBox($"Czy przenieść plik do miejsca, w którym może się synchronizować z chmurą?", $"Przenoszenie pliku {shortFileName}", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (dd == MessageBoxResult.Yes)
                        fi.MoveTo(correctFullFileName, true);
                    else if (dd == MessageBoxResult.No)
                        fi.CopyTo(correctFullFileName, true);
                    else
                        return new List<BaseMediaFileInfo>();
                }
                try
                {
                    var newMediaFileInfo = BaseMediaFileInfo.Factory.Create(this, shortFileName, null);
                    newMediaFileInfo.FillMetadata();
                    result.Add(newMediaFileInfo);
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowMessageBar($"Nie dodano pliku {fullFileName}. {ex.Message}");
                }
            }
            return result;
        }

        private void AddSourcesFromLocal(bool clearSources)
        {
            bool? result = DialogHelper.ShowOpenFile(OpenFileDialogTitle, OpenFileDialogFilter, true, out string[] fileNames);
            if (fileNames?.Any() != true) return;
            if (clearSources)
                Sources.Clear();

            GetSourcesFromLocal(fileNames)
                .ForEach(s => Sources.Add(s));
            SetOrder();
            ChangeFromChild(this);
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                bool mustStart = !_isCurrent && value;
                bool mustStop = _isCurrent && !value;
                SetValue(ref _isCurrent, value, nameof(IsCurrent));

                if (mustStart)
                {
                    BeginTime = DateTime.Now;
                    _changePositionTimer.Start();
                }
                if (mustStop)
                    _changePositionTimer.Stop();
            }
        }

        public bool Timeout
            => IsCurrent && Position > Duration;

        public bool BeforeTimeout
            => IsCurrent && !Timeout && Position > Duration - TimeSpan.FromSeconds(10);

        private Timer _changePositionTimer;
        public DateTime BeginTime;

        private RelayCommand _setAsCurrentCommand;
        public RelayCommand SetAsCurrentCommand
            => _setAsCurrentCommand ??= new RelayCommand(SetAsCurrent, () => !IsCurrent);

        private void SetAsCurrent()
        {
            Sorter.ContainerItemsSource
                .OrderByDescending(x => x == this)
                .ToList()
                .ForEach(d =>
            {
                d.IsCurrent = d == this;
                bool mustRecord = d.IsCurrent && Meeting.AudioRecording.IsRecording;
                if (mustRecord)
                    d.AudioRecording.StartRecordingCommand.Execute();
                else
                    d.AudioRecording.StopRecordingCommand.Execute();
            });
        }

        private RelayCommand _ClearCurrentCommand;
        public RelayCommand ClearCurrentCommand
            => _ClearCurrentCommand ??= new RelayCommand(ClearCurrent, () => IsCurrent);

        private void ClearCurrent()
            => Sorter.ContainerItemsSource.ToList().ForEach(d =>
            {
                d.IsCurrent = false;
                d.AudioRecording.StopRecordingCommand.Execute();
            });

        private void _changePositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MainVM.Dispatcher.Invoke(() => Position = DateTime.Now - BeginTime);
        }

        public void Dispose()
        {
            _changePositionTimer?.Dispose();
            AudioRecording?.Dispose();
            ParameterList?.Dispose();
        }

        #region IMeetingPointVM

        IRelayCommand IMeetingPointVM.AddIndentCommand => AddIndentCommand;
        IRelayCommand IMeetingPointVM.ChangeExpandedCommand => ChangeExpandedCommand;
        IRelayCommand IMeetingPointVM.ClearCurrentCommand => ClearCurrentCommand;
        IRelayCommand IMeetingPointVM.ClearSourcesCommand => ClearSourcesCommand;
        IRelayCommand IMeetingPointVM.OpenWwwCommand => OpenWwwCommand;
        IRelayCommand IMeetingPointVM.PlayAllCommand => PlayAllCommand;
        IRelayCommand IMeetingPointVM.RemoveIndentCommand => RemoveIndentCommand;
        IRelayCommand IMeetingPointVM.SetAsCurrentCommand => SetAsCurrentCommand;
        IRelayCommand<bool> IMeetingPointVM.AddSourcesCommand => AddSourcesCommand;

        IAudioRecordingProvider IMeetingPointVM.AudioRecording
        {
            get => AudioRecording;
            set => AudioRecording = (MeetingPointAudioRecordingProvider)value;
        }

        IMeetingVM IMeetingPointVM.Meeting
        {
            get => Meeting;
            set => Meeting = (MeetingVM)value;
        }

        IParametersCollectionVM IMeetingPointVM.ParameterList
        {
            get => ParameterList;
            set => ParameterList = (ParametersCollectionVM)value;
        }

        IBaseMediaFileInfo IMeetingPointVM.Source
        {
            get => Source;
            set => Source = (BaseMediaFileInfo)value;
        }

        ObservableCollection<IBaseMediaFileInfo> IMeetingPointVM.Sources
        {
            get => Sources?.Convert<BaseMediaFileInfo, IBaseMediaFileInfo>();
            set => Sources = value.Convert<IBaseMediaFileInfo, BaseMediaFileInfo>();
        }

        #endregion IMeetingPointVM
    }
}