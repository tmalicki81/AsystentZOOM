using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.AudioRecording;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.Model;
using FileService.Common;
using FileService.EventArgs;
using FileService.FileRepositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    [Serializable]
    public class MeetingVM : SingletonBaseVM, ICloneable
    {
        private class IoConsts
        {
            internal static string InitialDirectory = MediaLocalFileRepositoryFactory.Meetings.RootDirectory;
            internal const string DefaultExt = "meeting";
            internal const string Filter = "Pliki spotkań|*.meeting";
        }

        [Serializable]
        public class MeetingAudioRecordingProvider : AudioRecordingProvider
        {
            internal MeetingVM _meeting;

            public override bool IsReady
                => _meeting != null;

            public override string Title
                => _meeting.MeetingTitle;
        }

        public MeetingAudioRecordingProvider AudioRecording
        {
            get => _audioRecording;
            set => SetValue(ref _audioRecording, value, nameof(AudioRecording));
        }
        private MeetingAudioRecordingProvider _audioRecording;

        private static readonly Timer _timer;

        static MeetingVM()
        {
            _timer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        public MeetingVM()
        {
            AudioRecording = new MeetingAudioRecordingProvider();
            MeetingPointList = new ObservableCollection<MeetingPointVM>();
            ParameterList = new ParametersCollectionVM { Owner = this };
        }

        public override void ChangeFromChild(BaseVM child)
        {
            TryRegisterSnapshot();
        }

        private List<string> _timePiecesToDeleteWhenExitWithoutSave = new List<string>();

        public static string StartupFileName;

        public static MeetingVM Empty
        {
            get
            {
                var meetingBeginTimespan = new TimeSpan(DateTime.Now.AddHours(1).Hour, 30, 0);
                var meeting = new MeetingVM
                {
                    MeetingTitle = "Nowe spotkanie",
                    MeetingBegin = DateTime.Now.Add(meetingBeginTimespan),
                    MeetingDescription = "Opis spotkania", 
                    MeetingPointList = new ObservableCollection<MeetingPointVM>()
                };
                var parameters = new ParametersCollectionVM { Owner = meeting };
                meeting.ParameterList = parameters;
                parameters.Parameters = new ObservableCollection<ParameterVM> 
                { 
                    new ParameterVM{ ParametersCollection = parameters, Key = "Parametr 1", Value = "Wartość parametru 1" },
                    new ParameterVM{ ParametersCollection = parameters, Key = "Parametr 2", Value = "Wartość parametru 2" },
                    new ParameterVM{ ParametersCollection = parameters, Key = "Parametr 3", Value = "Wartość parametru 3" },
                };
                // Dodaj zegar
                var timePieceVM = new TimePieceVM
                {
                    AlertMinTime = TimeSpan.FromSeconds(10), 
                    TimerFormat = @"hh\:mm\:ss",
                    BreakTime = TimeSpan.FromSeconds(20),
                    Direction = TimePieceDirectionEnum.Back,
                    EndTime = meetingBeginTimespan,
                    Mode = TimePieceModeEnum.Timer,
                    ReferencePoint = TimePieceReferencePointEnum.ToSpecificTime,
                    UseBreak = true,
                    TextAbove = "Czasu do rozpoczęcia spotkania:",
                    TextBelow = $"Tytuł spotkania: {meeting.MeetingTitle}"
                };
                var xmlSerializer = new CustomXmlSerializer(timePieceVM.GetType());
                var timePieceRepository = MediaLocalFileRepositoryFactory.TimePiece;
                string timePieceFileName = $"{DateTime.Now:yyyy-MM-dd__HH_mm_ss}__{PathHelper.NormalizeToFileName(meeting.MeetingTitle)}.tim";
                meeting._timePiecesToDeleteWhenExitWithoutSave.Add(timePieceFileName);
                using (Stream memStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memStream, timePieceVM);
                    timePieceRepository.SaveFile(memStream, timePieceFileName);
                }
                var firstPoint = new MeetingPointVM
                {
                    Duration = TimeSpan.FromMinutes(30),
                    IsExpanded = true,
                    Meeting = meeting,
                    PointTitle = "Punkt pierwszy",
                    TitleColor = Colors.DarkGray, 
                    Sources = new ObservableCollection<BaseMediaFileInfo>()
                };
                meeting.MeetingPointList.Add(firstPoint);
                var timePieceFileInfo = BaseMediaFileInfo.Factory.Create(firstPoint, timePieceFileName, string.Empty);
                timePieceFileInfo.Title = $"Spotkanie o {meetingBeginTimespan.ToString(timePieceVM.TimerFormat)}";
                firstPoint.Sources.Add(timePieceFileInfo);
                
                var pointParemeters = new ParametersCollectionVM { Owner = firstPoint };
                firstPoint.ParameterList = pointParemeters;
                pointParemeters.Parameters = new ObservableCollection<ParameterVM>
                {
                    new ParameterVM{ ParametersCollection = pointParemeters, Key = "Parametr A", Value = "Wartość parametru A" },
                    new ParameterVM{ ParametersCollection = pointParemeters, Key = "Parametr B", Value = "Wartość parametru B" },
                    new ParameterVM{ ParametersCollection = pointParemeters, Key = "Parametr C", Value = "Wartość parametru C" },
                };

                meeting.ConfigureAudioRecording();
                meeting.ClearSnapshots();
                meeting.IsDataReady = true;
                meeting.TryRegisterSnapshot();
                return meeting;
            }
        }

        public void ClearLocalFileName()
            => LocalFileName = null;

        public void SaveLocalFile()
            => SaveLocalFile(false);

        public void SaveLocalFile(bool force)
        {
            if (string.IsNullOrEmpty(LocalFileName))
                return;
            string shortFileName = PathHelper.GetShortFileName(LocalFileName, '\\');
            if (!force)
            {
                var dr = DialogHelper.ShowMessageBox($"Czy zapisać plik {shortFileName}?", "Zapisywanie pliku", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (dr != MessageBoxResult.Yes)
                    return;
            }
            SaveFile(LocalFileName);
        }

        private static readonly object _meetingsSyncLocker = new object();
        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (string.IsNullOrEmpty(LocalFileName))
                return;

            _timer.Stop();
            try
            {
                var local = MediaLocalFileRepositoryFactory.Meetings;
                var remote = MediaFtpFileRepositoryFactory.Meetings;
                var filePath = new string[] { PathHelper.GetShortFileName(LocalFileName, '\\') };
                lock (_meetingsSyncLocker)
                {
                    local.Synchronize(remote, filePath);
                }
            }
            finally
            {
                _timer.Start();
            }
        }

        private string _webAddress;
        public string WebAddress
        {
            get => _webAddress;
            set => SetValue(ref _webAddress, value, nameof(WebAddress));
        }

        [XmlIgnore]
        public bool IsEditing
        {
            get => _isEditing;
            set => SetValue(ref _isEditing, value, nameof(IsEditing));
        }
        private bool _isEditing;

        public static string LocalFileName;

        private DateTime _meetingBegin;
        public DateTime MeetingBegin
        {
            get => _meetingBegin;
            set => SetValue(ref _meetingBegin, value, nameof(MeetingBegin));
        }

        private string _headerImage;
        public string HeaderImage
        {
            get => _headerImage;
            set => SetValue(ref _headerImage, value, nameof(HeaderImage));
        }

        private string _meetingTitle;
        public string MeetingTitle
        {
            get => _meetingTitle;
            set
            {
                SetValue(ref _meetingTitle, value, nameof(MeetingTitle));
                TryRegisterSnapshot();
            }
        }

        private string _meetingDescription;
        public string MeetingDescription
        {
            get => _meetingDescription;
            set => SetValue(ref _meetingDescription, value, nameof(MeetingDescription));
        }

        private ParametersCollectionVM _parameterList;
        public ParametersCollectionVM ParameterList
        {
            get => _parameterList;
            set => SetValue(ref _parameterList, value, nameof(ParameterList));
        }

        private ObservableCollection<MeetingPointVM> _meetingPointList;
        public ObservableCollection<MeetingPointVM> MeetingPointList
        {
            get => _meetingPointList;
            set => SetValue(ref _meetingPointList, value, nameof(MeetingPointList));
        }

        private RelayCommand<bool> _stopAllCommand;
        public RelayCommand<bool> StopAllCommand
            => _stopAllCommand ??= new RelayCommand<bool>(StopAll);

        private void StopAll(bool resetOutputWindow)
        {
            foreach (var layer in SingletonVMFactory.Layers)
            {
                layer.StopCommand.Execute();
                layer.IsEnabled = false;
            }
            foreach (var meetingPoint in MeetingPointList)
            {
                foreach (var source in meetingPoint.Sources)
                {
                    if (source.IsPlaying)
                    {
                        source.IsPlaying = false;
                        source.GetContent().StopCommand.Execute();
                        source.SetContent(null);
                    }
                }
            }
            if(resetOutputWindow)
                EventAggregator.Publish(nameof(MainVM) + "_Reset");
        }

        public override void OnDeserialized(object sender)
        {
            foreach (var meetingPoint in MeetingPointList)
                meetingPoint.Meeting = this;
            if (ParameterList == null)
                ParameterList = new ParametersCollectionVM();
            ParameterList.Owner = this;

            ConfigureAudioRecording();
        }

        private void ConfigureAudioRecording()
        {
            AudioRecording._meeting = this;
            AudioRecording.OnCommandExecuted -= AudioRecording_OnCommandExecuted;
            AudioRecording.OnCommandExecuted += AudioRecording_OnCommandExecuted;
        }

        internal void AudioRecording_OnCommandExecuted(object sender, EventArgs<RelayCommand> e)
        {
            if (!string.IsNullOrEmpty(LocalFileName))
            {
                SaveFile(LocalFileName);
                SendFileToCloud(LocalFileName);
            }
        }

        public override void Dispose()
        {
            _watcher_Dispose();
            AudioRecording.OnCommandExecuted -= AudioRecording_OnCommandExecuted; 
            AudioRecording?.Dispose();
            ParameterList?.Dispose();

            foreach (var meetingPoint in MeetingPointList)
                meetingPoint.Dispose();

            if (string.IsNullOrEmpty(LocalFileName))
            {
                var local = MediaLocalFileRepositoryFactory.TimePiece;
                var cloud = MediaFtpFileRepositoryFactory.TimePiece;

                foreach (string f in _timePiecesToDeleteWhenExitWithoutSave)
                {
                    try
                    {
                        local.Delete(f);
                        cloud.Delete(f);
                    }
                    catch { }
                }
            }

            base.Dispose();
        }

        private RelayCommand _syncAllMeetingsCommand;
        public RelayCommand SyncAllMeetingsCommand
            => _syncAllMeetingsCommand ??= new RelayCommand(SyncAllMeetings);
        
        private void SyncAllMeetings()
            => DialogHelper.RunAsync(
                null, false, null, 
                (p) =>
                {
                    SyncAll(p, "Synchronizacja spotkań", MediaFtpFileRepositoryFactory.Meetings, MediaLocalFileRepositoryFactory.Meetings);
                    SyncAll(p, "Synchronizacja zegarów", MediaFtpFileRepositoryFactory.TimePiece, MediaLocalFileRepositoryFactory.TimePiece);
                });

        private RelayCommand _syncAllRecordingsCommand;
        public RelayCommand SyncAllRecordingsCommand
            => _syncAllRecordingsCommand ??= new RelayCommand(SyncAllRecordings);

        private void SyncAllRecordings()
            => DialogHelper.RunAsync(
                null, false, null,
                (p) => SyncAll(p, "Synchronizacja nagrań", MediaFtpFileRepositoryFactory.AudioRecording, MediaLocalFileRepositoryFactory.AudioRecording));

        private void SyncAll(ProgressInfoVM p, string operationName, BaseFileRepository cloud, BaseFileRepository local)
        {
            p.OperationName = operationName;
            cloud.OnFileException += Sync_OnFileException;
            cloud.OnPushedFile += Sync_OnPushedFile; 
            local.OnFileException += Sync_OnFileException;
            local.OnPushedFile += Sync_OnPushedFile;            

            try
            {
                p.TaskName = "Z chmury na dysk";
                p.PercentCompletted = 0;
                cloud.PushToTarget(local, null); 
                
                p.TaskName = "Z dysku do chmury";
                p.PercentCompletted = 0;
                local.PushToTarget(cloud, null);                
            }
            finally
            {
                cloud.OnFileException -= Sync_OnFileException;
                cloud.OnPushedFile -= Sync_OnPushedFile; 
                local.OnFileException -= Sync_OnFileException;
                local.OnPushedFile -= Sync_OnPushedFile;                
            }
        }

        private void Sync_OnPushedFile(object sender, PushedFileEventArgs e)
        {
            var p = DialogHelper.GetProgressInfo();
            p.PercentCompletted = e.PercentCompletted;
        }

        private void Sync_OnFileException(object sender, FileExceptionEventArgs e)
            => DialogHelper.ShowMessageBar($"Błąd synchronizacji pliku {e.FileName}: {e.Exception.Message}");

        private RelayCommand _colapseAllMeetingPointsCommand;
        public RelayCommand ColapseAllMeetingPointsCommand
            => _colapseAllMeetingPointsCommand ??= new RelayCommand(
                ColapseAllMeetingPoints, 
                () => MeetingPointList?.Any(mp => mp.IsExpanded) == true);

        private void ColapseAllMeetingPoints()
        {
            MeetingPointList.Where(mp => mp.IsExpanded).ToList().ForEach(mp => mp.IsExpanded = false);
            TryRegisterSnapshot(); 
            RaiseCanExecuteChanged4All();
        }

        private RelayCommand _expandAllMeetingPointsCommand;
        public RelayCommand ExpandAllMeetingPointsCommand
            => _expandAllMeetingPointsCommand ??= new RelayCommand(
                ExpandAllMeetingPoints,
                () => MeetingPointList?.Any(mp => !mp.IsExpanded) == true);

        private void ExpandAllMeetingPoints()
        {
            MeetingPointList.Where(mp => !mp.IsExpanded).ToList().ForEach(mp => mp.IsExpanded = true);
            TryRegisterSnapshot(); 
            RaiseCanExecuteChanged4All();
        }

        private RelayCommand _addNewMeetingPointCommand;
        public RelayCommand AddNewMeetingPointCommand
            => _addNewMeetingPointCommand ??= new RelayCommand(AddNewMeetingPoint);

        private void AddNewMeetingPoint()
        {
            var newMeetingPoint = new MeetingPointVM { Meeting = this };
            MeetingPointList.Add(newMeetingPoint);
            newMeetingPoint.Sorter.Sort();
            TryRegisterSnapshot();
        }

        private RelayCommand _openFromLocalCommand;
        public RelayCommand OpenFromLocalCommand
            => _openFromLocalCommand ??= new RelayCommand(OpenFromLocal);

        public void DownloadAndFillMetadata(ProgressInfoVM progress)
        {
            _bytesDownloaded = 0;
            _bytesToDownload = MeetingPointList.Sum(x => x.Sources.Sum(u => u.GetBytesToDownload()));

            if (!string.IsNullOrEmpty(HeaderImage))
            {
                var headerImage = BaseMediaFileInfo.Factory.Create(null, HeaderImage, null);
                headerImage.CheckFileExist();
            }

            foreach (var meetingPoint in MeetingPointList)
            {
                foreach (var source in meetingPoint.Sources.ToList())
                {
                    if (source == meetingPoint.Sources.First())
                        MainVM.Dispatcher.Invoke(() =>
                        {
                            source.Sorter.Sort();
                        });
                    try
                    {
                        source.OnLoadMediaFile += Source_OnLoadMediaFile;
                        source.CheckFileExist();
                        MainVM.Dispatcher.Invoke(() =>
                        {
                            source.FillMetadata();
                        });
                        _bytesDownloaded += source.GetBytesToDownload();
                    }
                    finally
                    {
                        source.OnLoadMediaFile -= Source_OnLoadMediaFile;
                    }
                }
            }
        }

        private long _bytesToDownload;
        private long _bytesDownloaded;

        private void Source_OnLoadMediaFile(object sender, LoadingFileEventArgs e)
        {
            long notSavedBytesDownloaded = _bytesDownloaded + e.BytesDownloaded;
            var mediaFileInfo = sender as BaseMediaFileInfo;
            
            var progress = DialogHelper.GetProgressInfo();
            progress.TaskName = $"Pobieranie pliku {mediaFileInfo.FileName}...";
            progress.PercentCompletted = (int)(100 * notSavedBytesDownloaded / _bytesToDownload);
        }

        private static FileSystemWatcher _watcher;

        private void OpenFromLocal()
        {
            bool? result = DialogHelper.ShowOpenFile("Otwieranie dokumentu ze spotkaniem", IoConsts.Filter, false, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, out string[] fileNames);
            string fileName = fileNames?.FirstOrDefault();
            if (result != true || string.IsNullOrEmpty(fileName))
                return;

            OpenFromLocal(fileName);
        }

        public void OpenFromLocal(string fileName)
        {
            LocalFileName = fileName;
            _warcher_Create();
            string shortFileName = PathHelper.GetShortFileName(fileName, '\\');
            DialogHelper.RunAsync($"{shortFileName}..", true, "Ładowanie ", (p) =>
            {
                _timer.Stop();
                p.TaskName = "Zwalnianie blokady";
                lock (_meetingsSyncLocker)
                {
                    try
                    {
                        p.TaskName = "Synchronizacja pliku";
                        var local = MediaLocalFileRepositoryFactory.Meetings;
                        var remote = MediaFtpFileRepositoryFactory.Meetings;
                        string shortFileName = PathHelper.GetShortFileName(fileName, '\\');
                        local.Synchronize(remote, new string[] { shortFileName });

                        p.TaskName = "Otwieranie pliku";
                        var xmlSerializer = new CustomXmlSerializer(GetType());
                        using (var file = File.OpenRead(fileName))
                        {
                            var source = (MeetingVM)xmlSerializer.Deserialize(file);
                            var target = SingletonVMFactory.Meeting;

                            p.TaskName = "Szukanie różnic";
                            MainVM.Dispatcher.Invoke(() =>
                            {
                                if (source.InstanceId == target.InstanceId)
                                    SingletonVMFactory.CopyValuesWhenDifferent(source, ref target);
                                else
                                    SingletonVMFactory.SetSingletonValues(source);
                            });
                            target.AudioRecording.ExecuteRequests();
                            foreach (var meetingPoint in target.MeetingPointList)
                            {
                                meetingPoint.AudioRecording.ExecuteRequests();
                            }
                        }
                    }
                    finally
                    {
                        _timer.Start();
                        MainVM.Dispatcher.Invoke(() =>
                        {
                            DialogHelper.ShowMessageBar($"Załadowano dokument {shortFileName}");
                        });
                    }
                }
                p.TaskName = "Pobieranie metadanych";
                DownloadAndFillMetadata(p);
                ClearSnapshots();
                TryRegisterSnapshot();
            });
        }

        private void _watcher_Dispose() 
        {
            if (_watcher == null)
                return;

            _watcher.Changed -= _watcher_Changed; 
            _watcher.Dispose();            
        }

        private void _warcher_Create()
        {
            _watcher_Dispose();
            var fi = new FileInfo(LocalFileName);
            _watcher = new FileSystemWatcher(fi.DirectoryName, fi.Name)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _watcher.Changed += _watcher_Changed;
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string shortFileName = PathHelper.GetShortFileName(LocalFileName, '\\');
            MainVM.Dispatcher.Invoke(() =>
                {
                    DialogHelper.ShowMessageBar($"Wykryto modyfikację dokumentu {shortFileName}.");
                    OpenFromLocal(e.FullPath);
                });
        }

        private RelayCommand _saveToLocalCommand;
        public RelayCommand SaveToLocalCommand
            => _saveToLocalCommand ??= new RelayCommand(SaveToLocal);

        private void SaveToLocal()
        {
            if (!Directory.Exists(IoConsts.InitialDirectory))
                Directory.CreateDirectory(IoConsts.InitialDirectory);

            string fileName;
            if (string.IsNullOrEmpty(LocalFileName))
            {
                bool? result = DialogHelper.ShowSaveFile("Zapisanie dokumentu ze spotkaniem", IoConsts.Filter, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, out string[] fileNames);
                fileName = fileNames?.FirstOrDefault();
                if (result != true || string.IsNullOrEmpty(fileName))
                    return;
            }
            else
            {
                fileName = LocalFileName;
            }
            var sourcesToSend = new List<BaseMediaFileInfo>();
            foreach (var meetingPoint in MeetingPointList)
                sourcesToSend.AddRange(meetingPoint.Sources.Where(source => string.IsNullOrEmpty(source.WebAddress)));

            DialogHelper.RunAsync("Zapisywanie spotkania", false, string.Empty, (p) =>
            {                
                if (sourcesToSend.Any())
                {
                    p.ProgressBarVisibility = false;
                    var dr = DialogHelper.ShowMessageBox(
                        $"Czy wysłać niewysłane multimedia ({sourcesToSend.Count()}) do chmury?",
                        "Wysyłanie lokalnych multimediów do chmury",
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    p.ProgressBarVisibility = true;

                    if (dr == MessageBoxResult.Yes)
                    {
                        foreach (BaseMediaFileInfo d in sourcesToSend)
                        {
                            string shortFileName = d.FileName.Split('\\').Last();
                            var v = BaseMediaFileInfo.GetFileExtensionConfig(shortFileName);
                            v.MediaFtpFileRepository.OnSavingFile += MediaFtpFileRepository_OnSavingFile;
                            string[] destFilePath;
                            try
                            {
                                destFilePath = v.MediaLocalFileRepository.CopyTo(v.MediaFtpFileRepository, shortFileName);
                            }
                            finally
                            {
                                v.MediaFtpFileRepository.OnSavingFile -= MediaFtpFileRepository_OnSavingFile;
                            }
                            string destFileName = destFilePath.Aggregate((a, b) => a + "/" + b);
                            var sessionInfo = v.MediaFtpFileRepository.SessionInfo;
                            MainVM.Dispatcher.Invoke(() =>
                            {
                                d.FileName = shortFileName;
                                d.WebAddress = $"ftp://{sessionInfo.HostName}/{sessionInfo.RemoteDirectory}/{destFileName}";
                            });
                        }
                    }
                }

                SaveFile(fileName);
                p.TaskName = "Wysyłanie dokumentu spotkania do chmury";
                SendFileToCloud(fileName);
            });
        }

        /// <summary>
        /// Zapisanie pliku tymczasowego na dysk lokalny
        /// </summary>
        public void SaveTempFile()
        {
            string tmpMeetingFile = Path.Combine(MediaLocalFileRepositoryFactory.Meetings.RootDirectory, $"{InstanceId}.meeting");
            SaveFile(tmpMeetingFile);
        }

        /// <summary>
        /// Zapisanie pliku na dysk lokalny
        /// </summary>
        /// <param name="fileName">Pełna nazwa zapisywanego pliku</param>
        private void SaveFile(string fileName)
        {
            lock (_meetingsSyncLocker)
            {
                _watcher_Dispose();

                var xmlSerializer = new CustomXmlSerializer(GetType());
                if (File.Exists(fileName))
                    File.Delete(fileName);
                using (var fileStream = File.OpenWrite(fileName))
                {
                    xmlSerializer.Serialize(fileStream, this);
                }
                LocalFileName = fileName;
                _warcher_Create();
            }
            _isChanged = false;
        }

        private void SendFileToCloud(string fileName)
        {
            var meetingExConfig = BaseMediaFileInfo.GetFileExtensionConfig(fileName);

            try
            {
                meetingExConfig.MediaLocalFileRepository.OnSavingFile += MediaFtpFileRepository_OnSavingFile;
                lock (_meetingsSyncLocker)
                {
                    meetingExConfig.MediaLocalFileRepository.CopyTo(meetingExConfig.MediaFtpFileRepository, fileName.Split('\\').Last());
                }
            }
            finally
            {
                meetingExConfig.MediaLocalFileRepository.OnSavingFile -= MediaFtpFileRepository_OnSavingFile;
            }
        }

        private void MediaFtpFileRepository_OnSavingFile(object sender, SavingFileEventArgs e)
        {
            var p = DialogHelper.GetProgressInfo();
            p.PercentCompletted = e.PercentCompleted;
            p.TaskName = $"Wysyłanie pliku {e.FileName}";
        }

        private static UndoRedoManager<MeetingVM> _undoRedoManager = new();

        private bool _isChanged;

        private void ClearSnapshots()
        {
            _undoRedoManager.ClearSnapshots();
        }

        private bool TryRegisterSnapshot()
        {
            if (!IsDataReady)
                return false;
            if (!_undoRedoManager.AddSnapshot(this))
                return false;
            _isChanged = true;
            return true;
        }

        private RelayCommand _undoCommand;
        public RelayCommand UndoCommand
            => _undoCommand ??= new RelayCommand(UndoExecute, () => _undoRedoManager.CanUndo);

        private void UndoExecute()
        {
            var undoMeeting = _undoRedoManager.GetUndo();
            var target = this;
            //ParameterList.Owner = null;
            SingletonVMFactory.CopyValuesWhenDifferent(undoMeeting, ref target);
            RaiseCanExecuteChanged4All();
        }

        private RelayCommand _redoCommand;
        public RelayCommand RedoCommand
            => _redoCommand ??= new RelayCommand(RedoExecute, () => _undoRedoManager.CanRedo);

        private void RedoExecute()
        {
            var RedoMeeting = _undoRedoManager.GetRedo();
            var target = this;
            SingletonVMFactory.CopyValuesWhenDifferent(RedoMeeting, ref target);
            RaiseCanExecuteChanged4All();
        }

        public object Clone()
        {
            var serializer = new CustomXmlSerializer(GetType());
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, this);
                ms.Position = 0;
                return serializer.Deserialize(ms);
            }
        }
    }
}
