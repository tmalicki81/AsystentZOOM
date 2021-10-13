using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Common.Sortable;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Interfaces.Sortable;
using AsystentZOOM.VM.ViewModel;
using FileService.Clients;
using FileService.EventArgs;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Model
{
    public interface IBaseMediaFileInfo : IBaseVM, ISortableItemVM
    {
        BitmapSource Bitmap { get; set; }
        string Description { get; set; }
        FileExtensionEnum FileExtension { get; set; }
        string FileName { get; set; }
        bool FileNotFound { get; set; }
        bool IsPlaying { get; set; }
        IMeetingPointVM MeetingPoint { get; set; }
        bool MetadataCompleted { get; set; }
        IParametersCollectionVM ParameterList { get; set; }
        int PercentCompletted { get; set; }
        IRelayCommand PlayCommand { get; }
        bool ProgressBarVisibility { get; set; }
        IRelayCommand RefreshCommand { get; }
        string Title { get; set; }
        string WebAddress { get; set; }

        event EventHandler<LoadingFileEventArgs> OnLoadMediaFile;

        void CheckFileExist();
        ILayerVM CreateContent(string fileName);
        void FillMetadata();
        long GetBytesToDownload();
        ILayerVM GetContent();
        void SetContent(ILayerVM content);
    }

    [Serializable]
    [XmlInclude(typeof(VideoFileInfo))]
    [XmlInclude(typeof(TimePieceFileInfo))]
    [XmlInclude(typeof(AudioFileInfo))]
    [XmlInclude(typeof(ImageFileInfo))]
    [XmlInclude(typeof(BackgroundFileInfo))]
    public abstract class BaseMediaFileInfo : BaseVM, IBaseMediaFileInfo
    {
        public class Factory
        {
            public static BaseMediaFileInfo Create(MeetingPointVM meetingPoint, string fileName, string webAddress)
            {
                FileExtensionConfigAttribute fileExtensionConfig = GetFileExtensionConfig(fileName);
                if (fileExtensionConfig == null)
                    throw new NotImplementedException($"Nierozpoznane rozszerzenie pliku {fileExtensionConfig.Extension}.");

                BaseMediaFileInfo result = (BaseMediaFileInfo)Activator.CreateInstance(fileExtensionConfig.BaseMediaFileInfoType);

                result.WebAddress = webAddress;
                result.FileName = fileName;
                result.MeetingPoint = meetingPoint;
                result.FileExtension = fileExtensionConfig.FileExtension;
                return result;
            }
        }

        [Serializable]
        public class SortableMediaFileInfoProvider : SortableItemProvider<BaseMediaFileInfo>
        {
            public SortableMediaFileInfoProvider(BaseMediaFileInfo parameter) : base(parameter) { }
            public override string ItemCategory => "Plik";
            public override string ItemName => !string.IsNullOrEmpty(Item.Title) ? Item.Title : Item.FileName?.Split('\\').Last();
            public override bool CanCreateNewItem => false;
            public override ObservableCollection<BaseMediaFileInfo> ContainerItemsSource => (Item.MeetingPoint as MeetingPointVM)?.Sources;
            public override BaseMediaFileInfo NewItem() => null;
            public override BaseMediaFileInfo SelectedItem
            {
                get => (BaseMediaFileInfo)Item.MeetingPoint?.Source;
                set
                {
                    if (Item.MeetingPoint != null) 
                        Item.MeetingPoint.Source = value;
                }
            }
            public override object Container
            {
                get => Item.MeetingPoint;
                set => Item.MeetingPoint = (IMeetingPointVM)value;
            }
        }

        public static FileExtensionConfigAttribute GetFileExtensionConfig(string fileOrExtension)
            => typeof(FileExtensionEnum)
                    .GetFields()
                    .Select(x => x.GetCustomAttributes(typeof(FileExtensionConfigAttribute), false).FirstOrDefault() as FileExtensionConfigAttribute)
                    .FirstOrDefault(x => x?.Extension == fileOrExtension.Split('.').Last().ToUpper());

        private bool _fileWhenNotExistsChecked;

        public event EventHandler<LoadingFileEventArgs> OnLoadMediaFile;

        private void CallOnLoadMediaFile(object sender, LoadingFileEventArgs e)
        {
            PercentCompletted = e.PercentCompleted;
            OnLoadMediaFile?.Invoke(this, e);
        }

        private long? _bytesToDownload;
        public long GetBytesToDownload()
        {
            if (_bytesToDownload.HasValue)
                return _bytesToDownload.Value;

            var fileExtensionConfig = GetFileExtensionConfig(FileName);
            string fullFileName = Path.Combine(fileExtensionConfig.MediaLocalFileRepository.RootDirectory, FileName);
            if (!File.Exists(fullFileName) && !string.IsNullOrEmpty(WebAddress))
            {
                if (WebAddress.StartsWith("ftp://"))
                {
                    FtpClient ftpClient = new FtpClient(fileExtensionConfig.MediaFtpFileRepository.SessionInfo);
                    _bytesToDownload = ftpClient.GetFileSize(FileName);
                }
                else if (WebAddress.StartsWith("http://") || WebAddress.StartsWith("https://"))
                {
                    HttpFileClient httpFileClient = new HttpFileClient();
                    _bytesToDownload = httpFileClient.GetFileSize(WebAddress);
                }
            }
            else
            {
                _bytesToDownload = 0;
            }
            return _bytesToDownload.Value;
        }

        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
            => _refreshCommand ??= new RelayCommand(RefreshExecute);
        private void RefreshExecute()
        {
            CheckFileExist();
            FillMetadata();
        }

        public void CheckFileExist()
        {
            if (_fileWhenNotExistsChecked || string.IsNullOrEmpty(FileName))
                return;

            var fileExtensionConfig = GetFileExtensionConfig(FileName);
            string localFileDirectory = fileExtensionConfig.MediaLocalFileRepository.RootDirectory;
            string fullFileName = Path.Combine(localFileDirectory, FileName);

            // Jeśli nie ma pliku => pobierz go
            if (!File.Exists(fullFileName) && !string.IsNullOrEmpty(WebAddress))
            {
                try
                {
                    if (WebAddress.StartsWith("ftp://"))
                    {
                        ProgressBarVisibility = true;
                        fileExtensionConfig.MediaFtpFileRepository.OnLoadingFile += CallOnLoadMediaFile;
                        try
                        {
                            fileExtensionConfig.MediaFtpFileRepository.CopyTo(fileExtensionConfig.MediaLocalFileRepository, FileName);
                        }
                        finally
                        {
                            ProgressBarVisibility = false;
                            fileExtensionConfig.MediaFtpFileRepository.OnLoadingFile -= CallOnLoadMediaFile;
                        }
                    }
                    else if (WebAddress.StartsWith("http://") || WebAddress.StartsWith("https://"))
                    {
                        ProgressBarVisibility = true;
                        var httpFileClient = new HttpFileClient();
                        httpFileClient.OnDownloadingFile += CallOnLoadMediaFile;
                        try
                        {
                            byte[] fileBytes = httpFileClient.DownloadFile(WebAddress);
                            if (!Directory.Exists(localFileDirectory))
                                Directory.CreateDirectory(localFileDirectory);
                            File.WriteAllBytes(fullFileName, fileBytes);
                        }
                        finally
                        {
                            ProgressBarVisibility = false;
                            httpFileClient.OnDownloadingFile -= CallOnLoadMediaFile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowMessageBar(ex.Message);
                    return;
                }
            }
            _fileWhenNotExistsChecked = true;
        }

        public virtual void FillMetadata()
        {
            if (MetadataCompleted)
                return;

            var fileExtensionConfig = GetFileExtensionConfig(FileName);
            string fullFileName = Path.Combine(fileExtensionConfig.MediaLocalFileRepository.RootDirectory, FileName);

            if (File.Exists(fullFileName))
            {
                // Pobierz dane z systemu plików
                using (ShellObject shell = ShellObject.FromParsingName(fullFileName))
                {
                    if (string.IsNullOrEmpty(_titleFromFileSystem))
                        _titleFromFileSystem = shell.Properties.System.Title.Value;
                    if (string.IsNullOrEmpty(_titleFromFileSystem))
                        _titleFromFileSystem = fullFileName.Split('\\').Last();
                    Bitmap = GetBitmapSource(shell.Thumbnail.ExtraLargeBitmap);
                    if (this is IMovable movable)
                    {
                        var t = (ulong)shell.Properties.System.Media.Duration.ValueAsObject;
                        movable.Duration = TimeSpan.FromTicks((long)t);
                    }
                    if (this is IResizable resizable)
                    {
                        var image = shell.Properties.System.Image;
                        resizable.NaturalWidth = image.HorizontalSize.Value ?? 0;
                        resizable.NaturalHeight = image.VerticalSize.Value ?? 0;
                    }
                }
                FileNotFound = false;
            }
            else
            {
                FileNotFound = true;
            }

            try
            {
                Description = FilePropertiesRoot.GetString(fullFileName, nameof(Description));
            }
            catch { }

            MetadataCompleted = true;
            if (string.IsNullOrEmpty(Title))
                Title = _titleFromFileSystem;
            _content = null;
        }

        public BaseMediaFileInfo()
        {
            Sorter = new SortableMediaFileInfoProvider(this);
        }

        [XmlIgnore]
        public SortableMediaFileInfoProvider Sorter
        {
            get => _sorter;
            set => SetValue(ref _sorter, value, nameof(Sorter));
        }
        private SortableMediaFileInfoProvider _sorter;

        ISortableItemProvider ISortableItemVM.Sorter => Sorter;

        [XmlIgnore]
        public int PercentCompletted
        {
            get => _percentCompletted;
            set => SetValue(ref _percentCompletted, value, nameof(PercentCompletted));
        }
        private int _percentCompletted;

        [XmlIgnore]
        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetValue(ref _progressBarVisibility, value, nameof(ProgressBarVisibility));
        }
        private bool _progressBarVisibility;

        protected string _titleFromFileSystem;
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                SetValue(ref _title, value, nameof(Title));
                CallChangeToParent(this, $"Zmieniono tytuł na pliku {value}");
            }
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetValue(ref _fileName, value, nameof(FileName));
        }

        [XmlIgnore]
        public bool MetadataCompleted
        {
            get => _metadataCompleted;
            set => SetValue(ref _metadataCompleted, value, nameof(MetadataCompleted));
        }
        private bool _metadataCompleted;

        private FileExtensionEnum _fileExtension;
        public FileExtensionEnum FileExtension
        {
            get => _fileExtension;
            set => SetValue(ref _fileExtension, value, nameof(FileExtension));
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value, nameof(Description));
        }

        [XmlIgnore]
        public bool FileNotFound
        {
            get => _fileNotFound;
            set => SetValue(ref _fileNotFound, value, nameof(FileNotFound));
        }
        private bool _fileNotFound;

        [XmlIgnore]
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetValue(ref _isPlaying, value, nameof(IsPlaying));
        }
        private bool _isPlaying;

        private string _webAddress;
        public string WebAddress
        {
            get => _webAddress;
            set => SetValue(ref _webAddress, value, nameof(WebAddress));
        }

        private ParametersCollectionVM _parameterList;
        public ParametersCollectionVM ParameterList
        {
            get => _parameterList;
            set => SetValue(ref _parameterList, value, nameof(ParameterList));
        }

        [Parent(typeof(IMeetingPointVM))]
        public IMeetingPointVM MeetingPoint
        {
            get => _meetingPoint;
            set
            {
                SetValue(ref _meetingPoint, value, nameof(MeetingPoint));
                if (string.IsNullOrEmpty(Title))
                    Title = _titleFromFileSystem;
            }
        }
        private IMeetingPointVM _meetingPoint;

        [XmlIgnore]
        public BitmapSource Bitmap
        {
            get => _bitmap;
            set => SetValue(ref _bitmap, value, nameof(Bitmap));
        }
        private BitmapSource _bitmap;

        protected BitmapSource GetBitmapSource(System.Drawing.Bitmap bmp)
        {
            var result = Imaging.CreateBitmapSourceFromHBitmap(
               bmp.GetHbitmap(),
               IntPtr.Zero,
               Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(96, 96));
            return result;
        }

        public abstract ILayerVM CreateContent(string fileName);

        protected ILayerVM _content;
        public ILayerVM GetContent()
        {
            if (_content == null)
                _content = CreateContent(FileName);
            _content.FileInfo = this;
            return _content;
        }

        public void SetContent(ILayerVM content)
            => _content = content;

        private RelayCommand _playCommand;
        public RelayCommand PlayCommand
            => _playCommand ??= new RelayCommand(Play);

        private void Play()
        {
            foreach (var d in MeetingPoint.Sources)
                d.Sorter.IsSelected = d == MeetingPoint.Source;
            MeetingPoint.Source = this;
            SetContent(null);
            var newLayer = GetContent();
            newLayer.IsEnabled = true;
            Type newLayerType = newLayer.GetType();

            foreach (var layer in SingletonVMFactory.Layers)
            {
                Type layerType = layer.GetType();
                if (layerType == newLayerType)
                {
                    var singletonValue = (ILayerVM)SingletonVMFactory.SetSingletonValues(newLayer);
                    singletonValue.FileInfo = this;
                    SetContent(singletonValue);

                    layer.PlayCommand.Execute();
                    IsPlaying = true;
                }
                else
                {
                    layer.StopCommand.Execute();
                    layer.IsEnabled = false;
                }
            }
        }

        public override string ToString()
            => FileName;

        IRelayCommand IBaseMediaFileInfo.PlayCommand => PlayCommand;
        IRelayCommand IBaseMediaFileInfo.RefreshCommand => RefreshCommand; 
        IParametersCollectionVM IBaseMediaFileInfo.ParameterList
        {
            get => ParameterList;
            set => ParameterList = (ParametersCollectionVM)value;
        }        
    }

    [Serializable]
    public abstract class BaseMediaFileInfo<TContent> : BaseMediaFileInfo
        where TContent : class, ILayerVM
    {
        [XmlIgnore]
        public TContent Content
        {
            get => (TContent)GetContent();
            set => SetValue(ref _content, value, nameof(Content));
        }
    }
}
