using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.AudioRecording;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.FileRepositories;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using FileService.Common;
using FileService.EventArgs;
using FileService.FileRepositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    public interface IMeetingVM : IBaseVM, ICloneable, IDisposable
    {
        IRelayCommand AddNewMeetingPointCommand { get; }
        IAudioRecordingProvider AudioRecording { get; set; }
        IRelayCommand ColapseAllMeetingPointsCommand { get; }
        IRelayCommand ExpandAllMeetingPointsCommand { get; }
        string HeaderImage { get; set; }
        bool IsEditing { get; set; }
        DateTime MeetingBegin { get; set; }
        string MeetingDescription { get; set; }
        ObservableCollection<IMeetingPointVM> MeetingPointList { get; set; }
        string MeetingTitle { get; set; }
        IRelayCommand OpenFromLocalCommand { get; }
        IParametersCollectionVM ParameterList { get; set; }
        IRelayCommand RedoCommand { get; }
        IRelayCommand SaveToLocalCommand { get; }
        IRelayCommand<bool> StopAllCommand { get; }
        IRelayCommand SyncAllMeetingsCommand { get; }
        IRelayCommand SyncAllRecordingsCommand { get; }
        IRelayCommand UndoCommand { get; }
        void AudioRecording_OnCommandExecuted(object sender, EventArgs<IRelayCommand> e);
        string WebAddress { get; set; }
        void ClearLocalFileName();
        Task DownloadAndFillMetadata(IProgressInfoVM progress);
        Task OpenFromLocal(string fileName);
        Task SaveLocalFile(bool force);
        void SaveTempFile();
    }

    [Serializable]
    public class MeetingVM : SingletonBaseVM, IMeetingVM
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
            [Parent(typeof(IMeetingVM))]
            public IMeetingVM Meeting { get; set; }

            public override bool IsReady
                => Meeting != null;

            public override string Title
                => Meeting.MeetingTitle;
        }

        public MeetingAudioRecordingProvider AudioRecording
        {
            get => _audioRecording;
            set => SetValue(ref _audioRecording, value, nameof(AudioRecording));
        }
        private MeetingAudioRecordingProvider _audioRecording;

        private static readonly Timer _timer;

        [XmlIgnore]
        public override bool IsDataReady
        {
            get => _isDataReady;
            set => _isDataReady = value;
        }
        private static bool _isDataReady;

        static MeetingVM()
        {
            _timer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        public MeetingVM()
        {
            AudioRecording = new MeetingAudioRecordingProvider();
            IsDataReady = false;
        }

        public override void CallChangeToParent(IBaseVM child, string description)
        {
            base.CallChangeToParent(child, description);
            TryRegisterSnapshot(description, false);
        }

        public static string StartupFileName;

        public static IMeetingVM CreateMeeting
        {
            get
            {
                var meetingBeginTimespan = new TimeSpan(DateTime.Now.AddHours(1).Hour, 30, 0);
                var meeting = new MeetingVM
                {
                    MeetingTitle = "Nowe spotkanie",
                    MeetingBegin = DateTime.Now.Add(meetingBeginTimespan),
                    MeetingDescription = "Opis spotkania",
                };
                var parameters = new ParametersCollectionVM();
                meeting.ParameterList = parameters;
                parameters.Parameters = new ObservableCollection<ParameterVM>
                {
                    new ParameterVM { Key = "Parametr 1", Value = "Wartość parametru 1" },
                    new ParameterVM { Key = "Parametr 2", Value = "Wartość parametru 2" },
                    new ParameterVM { Key = "Parametr 3", Value = "Wartość parametru 3" },
                };

                var firstPoint = MeetingPointVM.CreateMeetingPoint();

                meeting.MeetingPointList.Add(firstPoint);
                meeting.ConfigureAudioRecording();
                meeting.ClearSnapshots();
                meeting.IsDataReady = true;
                meeting.TryRegisterSnapshot("Utworzono nowy dokument", true);
                meeting.IsChanged = false;
                return meeting;
            }
        }

        public void ClearLocalFileName()
            => LocalFileName = null;

        public async Task SaveLocalFile(bool force)
        {
            if (string.IsNullOrEmpty(LocalFileName))
                return;
            string shortFileName = PathHelper.GetShortFileName(LocalFileName, '\\');
            if (!force)
            {
                bool dr = await DialogHelper.ShowMessageBoxAsync(
                    $"Czy zapisać plik {shortFileName}?", "Zapisywanie pliku", ImageEnum.Question, false,
                    new MsgBoxButtonVM<bool>[]
                    {
                        new(true,  "Tak, zapisz", ImageEnum.Yes),
                        new(false, "Nie zapisuj", ImageEnum.No),
                    });
                if (!dr)
                    return;
            }
            SaveMeetingDocument(LocalFileName);
        }

        private static readonly object _meetingsSyncLocker = new object();
        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string localFileName = SingletonVMFactory.Meeting.LocalFileName;

            if (string.IsNullOrEmpty(localFileName) ||
                BaseMediaFileInfo.GetFileExtensionConfig(localFileName).FileExtension != FileExtensionEnum.MEETING)
                return;

            _timer.Stop();
            try
            {
                if (SingletonVMFactory.Main.IsAutoSyncEnabled)
                {
                    var local = MediaLocalFileRepositoryFactory.Meetings;
                    var remote = MediaFtpFileRepositoryFactory.Meetings;
                    var filePath = new string[] { PathHelper.GetShortFileName(localFileName, '\\') };
                    lock (_meetingsSyncLocker)
                    {
                        remote.PushToTarget(local, filePath);
                        //local.Synchronize(remote, filePath);
                    }
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

        [XmlIgnore]
        public string LocalFileName
        {
            get => _localFileName;
            set => SetValue(ref _localFileName, value, nameof(LocalFileName));
        }
        private string _localFileName;

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
                TryRegisterSnapshot($"Zmieniono tytuł spotkania na {value}", false);
            }
        }

        private string _meetingDescription;
        public string MeetingDescription
        {
            get => _meetingDescription;
            set => SetValue(ref _meetingDescription, value, nameof(MeetingDescription));
        }

        private ParametersCollectionVM _parameterList = new();
        public ParametersCollectionVM ParameterList
        {
            get => _parameterList;
            set => SetValue(ref _parameterList, value, nameof(ParameterList));
        }

        private ObservableCollection<MeetingPointVM> _meetingPointList = new();
        public ObservableCollection<MeetingPointVM> MeetingPointList
        {
            get => _meetingPointList;
            set => SetValue(ref _meetingPointList, value, nameof(MeetingPointList));
        }

        private RelayCommand<bool> _stopAllCommand;
        public IRelayCommand<bool> StopAllCommand
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
            if (resetOutputWindow)
                EventAggregator.Publish(nameof(MainVM) + "_Reset");
        }

        public override void OnDeserialized(object sender)
        {
            base.OnDeserialized(sender);
            ConfigureAudioRecording();
        }

        private void ConfigureAudioRecording()
        {
            AudioRecording.OnCommandExecuted -= AudioRecording_OnCommandExecuted;
            AudioRecording.OnCommandExecuted += AudioRecording_OnCommandExecuted;
        }

        public void AudioRecording_OnCommandExecuted(object sender, EventArgs<IRelayCommand> e)
        {
            if (!string.IsNullOrEmpty(LocalFileName))
            {
                SaveMeetingDocument(LocalFileName);
            }
        }

        public override void Dispose()
        {
            if (IsChanged)
            {
                bool dr = DialogHelper.ShowMessageBoxAsync(
                    $"Czy zapisać plik przed zamknięciem aplikacji?", "Zapisywanie pliku",
                    ImageEnum.Question, false,
                    new MsgBoxButtonVM<bool>[]
                    {
                        new(true,  "Tak, zapisz", ImageEnum.Yes),
                        new(false, "Nie zapisuj", ImageEnum.No),
                    }).Result;
                if (dr)
                    SaveToLocal(Save);
            }

            _watcher_Dispose();
            AudioRecording.OnCommandExecuted -= AudioRecording_OnCommandExecuted;
            AudioRecording?.Dispose();

            foreach (var meetingPoint in MeetingPointList)
                meetingPoint.Dispose();

            base.Dispose();
        }

        private IRelayCommand _syncAllMeetingsCommand;
        public IRelayCommand SyncAllMeetingsCommand
            => _syncAllMeetingsCommand ??= new RelayCommand(SyncAllMeetings);

        private async void SyncAllMeetings()
        {
            try
            {
                using (var progress = new ShowProgressInfo())
                {
                    await SyncAllAsync(progress, "Synchronizacja spotkań", MediaFtpFileRepositoryFactory.Meetings, MediaLocalFileRepositoryFactory.Meetings);
                    await SyncAllAsync(progress, "Synchronizacja zegarów", MediaFtpFileRepositoryFactory.TimePiece, MediaLocalFileRepositoryFactory.TimePiece);
                }
            }
            catch (Exception ex)
            {
                await HandleException(ex);
            }
        }

        private IRelayCommand _syncAllRecordingsCommand;
        public IRelayCommand SyncAllRecordingsCommand
            => _syncAllRecordingsCommand ??= new RelayCommand(SyncAllRecordings);

        private async void SyncAllRecordings()
        {
            try
            {
                using (var progress = new ShowProgressInfo())
                {
                    await SyncAllAsync(progress, "Synchronizacja nagrań", MediaFtpFileRepositoryFactory.AudioRecording, MediaLocalFileRepositoryFactory.AudioRecording);
                }
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageBoxAsync(ex.ToString(), "Błąd", ImageEnum.Error);
            }
        }

        private async Task SyncAllAsync(IProgressInfoVM p, string operationName, BaseFileRepository cloud, BaseFileRepository local)
        {
            p.OperationName = operationName;
            cloud.OnFileException += Sync_OnFileException;
            local.OnFileException += Sync_OnFileException;

            var onPushedFileHandler = new EventHandler<PushedFileEventArgs>((s, e) => p.PercentCompletted = e.PercentCompletted);
            cloud.OnPushedFile += onPushedFileHandler;
            local.OnPushedFile += onPushedFileHandler;

            try
            {
                p.TaskName = "Z chmury na dysk";
                p.PercentCompletted = 0;
                await Task.Run(() => cloud.PushToTarget(local, null));

                p.TaskName = "Z dysku do chmury";
                p.PercentCompletted = 0;
                await Task.Run(() => local.PushToTarget(cloud, null));
            }
            finally
            {
                cloud.OnFileException -= Sync_OnFileException;
                local.OnFileException -= Sync_OnFileException;
                cloud.OnPushedFile -= onPushedFileHandler;
                local.OnPushedFile -= onPushedFileHandler;
            }
        }

        private void Sync_OnFileException(object sender, FileExceptionEventArgs e)
            => DialogHelper.ShowMessageBar($"Błąd synchronizacji pliku {e.FileName}: {e.Exception.Message}");

        private IRelayCommand _colapseAllMeetingPointsCommand;
        public IRelayCommand ColapseAllMeetingPointsCommand
            => _colapseAllMeetingPointsCommand ??= new RelayCommand(
                ColapseAllMeetingPoints,
                () => MeetingPointList?.Any(mp => mp.IsExpanded) == true);

        private void ColapseAllMeetingPoints()
        {
            MeetingPointList.Where(mp => mp.IsExpanded).ToList().ForEach(mp => mp.IsExpanded = false);
            RaiseCanExecuteChanged4All();
        }

        private IRelayCommand _expandAllMeetingPointsCommand;
        public IRelayCommand ExpandAllMeetingPointsCommand
            => _expandAllMeetingPointsCommand ??= new RelayCommand(
                ExpandAllMeetingPoints,
                () => MeetingPointList?.Any(mp => !mp.IsExpanded) == true);

        private void ExpandAllMeetingPoints()
        {
            MeetingPointList.Where(mp => !mp.IsExpanded).ToList().ForEach(mp => mp.IsExpanded = true);
            RaiseCanExecuteChanged4All();
        }

        private IRelayCommand _addNewMeetingPointCommand;
        public IRelayCommand AddNewMeetingPointCommand
            => _addNewMeetingPointCommand ??= new RelayCommand(AddNewMeetingPoint);

        private void AddNewMeetingPoint()
        {
            var newMeetingPoint = MeetingPointVM.CreateMeetingPoint();
            MeetingPointList.Add(newMeetingPoint);
            newMeetingPoint.Sorter.Sort();
            TryRegisterSnapshot("Dodano nowy punkt", false);
        }

        private IRelayCommand _refreshAllCommand;
        public IRelayCommand RefreshAllCommand
            => _refreshAllCommand ??= new RelayCommand(
                RefreshAll);

        private void RefreshAll()
        {
            foreach (var meeting in MeetingPointList)
                foreach (var source in meeting.Sources)
                    source.RefreshCommand.Execute();
        }

        private IRelayCommand _openFromLocalCommand;
        public IRelayCommand OpenFromLocalCommand
            => _openFromLocalCommand ??= new RelayCommand(OpenFromLocal);

        public async Task DownloadAndFillMetadata(IProgressInfoVM progress)
        {
            var allSources = new List<IBaseMediaFileInfo>();
            foreach (var meetingPoint in MeetingPointList)
                foreach (var source in meetingPoint.Sources)
                    allSources.Add(source);

            progress.TaskName = "Liczenie...";
            _bytesDownloaded = 0;
            _bytesToDownload = allSources.Sum(u => u.GetBytesToDownload());

            progress.TaskName = "Pobieranie tła...";
            if (!string.IsNullOrEmpty(HeaderImage))
            {
                var headerImage = BaseMediaFileInfo.Factory.Create(null, HeaderImage, null);
                headerImage.CheckFileExist();
            }

            progress.TaskName = "Sortowanie...";
            MainVM.Dispatcher.Invoke(() =>
                MeetingPointList
                    .ToList()
                    .ForEach(meetingPoint => meetingPoint.Sources?.FirstOrDefault()?.Sorter.Sort()));

            var handler = new EventHandler<LoadingFileEventArgs>((s, e) =>
                Source_OnLoadMediaFile(s, e, progress));

            progress.TaskName = "Pobieranie multimediów...";
            foreach (var source in allSources)
            {
                try
                {
                    source.OnLoadMediaFile += handler;
                    await Task.Run(() => source.CheckFileExist());
                    _bytesDownloaded += source.GetBytesToDownload();
                }
                finally
                {
                    source.OnLoadMediaFile -= handler;
                }
            }

            progress.TaskName = "Uzupełnianie...";
            MainVM.Dispatcher.Invoke(() =>
                allSources.ForEach(s => s.FillMetadata()));
        }

        private long _bytesToDownload;
        private long _bytesDownloaded;

        private void Source_OnLoadMediaFile(object sender, LoadingFileEventArgs e, IProgressInfoVM progress)
        {
            long notSavedBytesDownloaded = _bytesDownloaded + e.BytesDownloaded;
            var mediaFileInfo = sender as BaseMediaFileInfo;

            progress.TaskName = $"Pobieranie pliku {mediaFileInfo.FileName}...";
            progress.PercentCompletted = (int)(100 * notSavedBytesDownloaded / _bytesToDownload);
        }

        private static FileSystemWatcher _watcher;

        private async void OpenFromLocal()
        {
            bool? result = DialogHelper.ShowOpenFile("Otwieranie dokumentu ze spotkaniem", IoConsts.Filter, false, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, out string[] fileNames);
            string fileName = fileNames?.FirstOrDefault();
            if (result != true || string.IsNullOrEmpty(fileName))
                return;

            await OpenFromLocal(fileName);
        }

        public async Task OpenFromLocal(string fileName)
        {
            LocalFileName = fileName;
            _warcher_Create();
            string shortFileName = PathHelper.GetShortFileName(fileName, '\\');
            using (var p = new ShowProgressInfo($"{shortFileName}..", true, "Ładowanie "))
            {
                _timer.Stop();
                p.TaskName = "Zwalnianie blokady";

                try
                {
                    p.TaskName = "Synchronizacja pliku";
                    var local = MediaLocalFileRepositoryFactory.Meetings;
                    var remote = MediaFtpFileRepositoryFactory.Meetings;
                    await Task.Run(() =>
                    {
                        lock (_meetingsSyncLocker)
                        {
                            var c = BaseMediaFileInfo.GetFileExtensionConfig(shortFileName);
                            if (c.FileExtension == FileExtensionEnum.MEETING)
                            {
                                local.Synchronize(remote, new string[] { shortFileName });
                            }
                        }
                    });

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
                    DialogHelper.ShowMessageBar($"Załadowano dokument {shortFileName}");
                }
                p.TaskName = "Pobieranie metadanych";
                await DownloadAndFillMetadata(p);
                ClearSnapshots();
                TryRegisterSnapshot($"Załadowano dokument {shortFileName}", true);
                _isChanged = false;
            }
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
            DialogHelper.ShowMessageBar($"Wykryto modyfikację dokumentu {shortFileName}.");
            MainVM.Dispatcher.Invoke(() => OpenFromLocal(e.FullPath));
        }

        private IRelayCommand _saveToLocalCommand;
        public IRelayCommand SaveToLocalCommand
            => _saveToLocalCommand ??= new RelayCommand<bool>(SaveToLocal);

        public static bool SaveAs = true;
        public static bool Save = false;

        private async void SaveToLocal(bool saveAs)
        {
            if (!Directory.Exists(IoConsts.InitialDirectory))
                Directory.CreateDirectory(IoConsts.InitialDirectory);

            bool isTempMeetingDocument = PathHelper.GetFileExtension(LocalFileName) == nameof(FileExtensionEnum.TMP_MEETING);
            string fileName;
            if (saveAs ||
                string.IsNullOrEmpty(LocalFileName) ||
                isTempMeetingDocument)
            {
                var fileNames = !string.IsNullOrEmpty(LocalFileName)
                    ? new string[]
                        {
                            Path.ChangeExtension(LocalFileName, nameof(FileExtensionEnum.MEETING).ToLower())
                        }
                    : new string[] { };

                string dialogTitle = "Zapisanie dokumentu ze spotkaniem" + (saveAs ? " jako..." : null);
                bool? result = DialogHelper.ShowSaveFile(dialogTitle, IoConsts.Filter, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, ref fileNames);
                fileName = fileNames?.FirstOrDefault();
                if (result != true || string.IsNullOrEmpty(fileName))
                    return;
            }
            else
            {
                fileName = LocalFileName;
            }

            // Jeśli plik jest plikiem tymczasowym, zmień go na zwykły
            foreach (var meetingPoint in MeetingPointList)
                foreach (var source in meetingPoint.Sources)
                    source.IsTemporaryFile = false;

            // Lista multimediów do wysłania do chmury
            var sourcesToSend = new List<IBaseMediaFileInfo>();
            foreach (var meetingPoint in MeetingPointList)
                sourcesToSend.AddRange(meetingPoint.Sources.Where(source => string.IsNullOrEmpty(source.WebAddress)));

            if (sourcesToSend.Any())
            {
                bool dr = await DialogHelper.ShowMessageBoxAsync(
                    $"Czy wysłać niewysłane multimedia ({sourcesToSend.Count}) do chmury?",
                    "Wysyłanie lokalnych multimediów do chmury",
                    ImageEnum.Question, false,
                    new MsgBoxButtonVM<bool>[]
                    {
                            new(true,  "Tak, wyślij", ImageEnum.Yes),
                            new(false, "Nie wysyłaj", ImageEnum.No),
                    });

                if (dr)
                {
                    using (var p = new ShowProgressInfo("Wysyłanie multimediów", false, string.Empty))
                    {
                        await Task.Run(() => SendSources(sourcesToSend, p));
                    }
                }
            }
            using (var p = new ShowProgressInfo("Zapisywanie spotkania", false, string.Empty))
            {
                SaveMeetingDocument(fileName);
            }
        }

        private void SendSources(List<IBaseMediaFileInfo> sourcesToSend, IProgressInfoVM p)
        {
            foreach (IBaseMediaFileInfo d in sourcesToSend)
            {
                string shortFileName = d.FileName.Split('\\').Last();
                var v = BaseMediaFileInfo.GetFileExtensionConfig(shortFileName);
                v.MediaFtpFileRepository.OnSavingFile += (s, e) => MediaFtpFileRepository_OnSavingFile(s, e, p);
                string[] destFilePath;
                try
                {
                    destFilePath = v.MediaLocalFileRepository.CopyTo(v.MediaFtpFileRepository, shortFileName);
                }
                finally
                {
                    v.MediaFtpFileRepository.OnSavingFile -= (s, e) => MediaFtpFileRepository_OnSavingFile(s, e, p);
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

        private void AutoSave()
        {
            if (SingletonVMFactory.Main.IsAutoSaveEnabled &&
                !string.IsNullOrEmpty(LocalFileName))
            {
                SaveMeetingDocument(LocalFileName);
            }
        }

        /// <summary>
        /// Zapisanie pliku tymczasowego na dysk lokalny
        /// </summary>
        public void SaveTempFile()
        {
            string location = MediaLocalFileRepositoryFactory.Meetings.RootDirectory;
            string shortFileName = PathHelper.NormalizeToFileName(MeetingTitle);
            string extension = nameof(FileExtensionEnum.TMP_MEETING).ToLower();
            string tmpMeetingFile = $"{location}\\{shortFileName}.{extension}";

            SaveMeetingDocument(tmpMeetingFile);
            File.SetAttributes(tmpMeetingFile, FileAttributes.Hidden);
        }

        /// <summary>
        /// Zapisanie dokumentu na dysk lokalny
        /// </summary>
        /// <param name="fileName">Pełna nazwa zapisywanego pliku</param>
        private void SaveMeetingDocument(string fileName)
        {
            var fileExtension = BaseMediaFileInfo.GetFileExtensionConfig(fileName).FileExtension;
            string shortFileName = PathHelper.GetShortFileName(fileName, '\\');

            lock (_meetingsSyncLocker)
            {
                _watcher_Dispose();

                // Zapisz plik lokalnie
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

            if (fileExtension == FileExtensionEnum.MEETING)
            {
                // Zapisz plik w chmurze
                SendFileToCloud(shortFileName);
            }

            // Resetuj zmiany
            _isChanged = false;

            DialogHelper.ShowMessageBar($"Zapisano plik {shortFileName}");
        }

        private void SendFileToCloud(string fileName)
        {
            var meetingExConfig = BaseMediaFileInfo.GetFileExtensionConfig(fileName);

            lock (_meetingsSyncLocker)
            {
                meetingExConfig.MediaLocalFileRepository.CopyTo(meetingExConfig.MediaFtpFileRepository, fileName.Split('\\').Last());
            }
        }

        private void MediaFtpFileRepository_OnSavingFile(object sender, SavingFileEventArgs e, IProgressInfoVM p)
        {
            p.PercentCompletted = e.PercentCompleted;
            p.TaskName = $"Wysyłanie pliku {e.FileName}";
        }

        private static UndoRedoManager<MeetingVM> _undoRedoManager = new();

        private static bool _isChanged;
        private bool IsChanged
        {
            get => _isChanged;
            set => _isChanged = value;
        }

        private void ClearSnapshots()
        {
            _undoRedoManager.ClearSnapshots();
        }

        private bool TryRegisterSnapshot(string description, bool isFirstUse)
        {
            if (!IsDataReady)
                return false;
            if (!_undoRedoManager.AddSnapshot(this, description))
                return false;
            _isChanged = true;

            if (!isFirstUse)
                AutoSave();

            MainVM.Dispatcher.Invoke(() =>
            {
                if (SingletonVMFactory.Main.IsShowChangesEnabled)
                    DialogHelper.ShowMessageBar(description);
            });
            return true;
        }

        private IRelayCommand _undoCommand;
        public IRelayCommand UndoCommand
            => _undoCommand ??= new RelayCommand(UndoExecute, () => _undoRedoManager.CanUndo);

        private void UndoExecute()
        {
            var undoMeeting = _undoRedoManager.GetUndo();
            var target = this;
            SingletonVMFactory.CopyValuesWhenDifferent(undoMeeting.Value, ref target);
            AutoSave();
            RaiseCanExecuteChanged4All();
            MainVM.Dispatcher.Invoke(() =>
            {
                if (SingletonVMFactory.Main.IsShowChangesEnabled)
                    DialogHelper.ShowMessageBar($"Przywrócono stan do: {undoMeeting.Description}");
            });
        }

        private IRelayCommand _redoCommand;
        public IRelayCommand RedoCommand
            => _redoCommand ??= new RelayCommand(RedoExecute, () => _undoRedoManager.CanRedo);

        private void RedoExecute()
        {
            var redoMeeting = _undoRedoManager.GetRedo();
            var target = this;
            SingletonVMFactory.CopyValuesWhenDifferent(redoMeeting.Value, ref target);
            AutoSave();
            RaiseCanExecuteChanged4All();
            MainVM.Dispatcher.Invoke(() =>
            {
                if (SingletonVMFactory.Main.IsShowChangesEnabled)
                    DialogHelper.ShowMessageBar($"Przywrócono stan do: {redoMeeting.Description}");
            });
        }

        public object Clone()
        {
            var serializer = new XmlSerializer(GetType());
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, this);
                ms.Position = 0;
                return serializer.Deserialize(ms);
            }
        }

        #region IMeetingVM

        ObservableCollection<IMeetingPointVM> IMeetingVM.MeetingPointList
        {
            get => MeetingPointList?.Convert<MeetingPointVM, IMeetingPointVM>();
            set => MeetingPointList = value?.Convert<IMeetingPointVM, MeetingPointVM>();
        }

        IParametersCollectionVM IMeetingVM.ParameterList
        {
            get => ParameterList;
            set => ParameterList = (ParametersCollectionVM)value;
        }

        IAudioRecordingProvider IMeetingVM.AudioRecording
        {
            get => AudioRecording;
            set => AudioRecording = (MeetingAudioRecordingProvider)value;
        }

        #endregion IMeetingVM
    }
}
