using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using System.Windows;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    public class OutputImageVM : SingletonBaseVM, ILayerVM
    {
        public bool IsVisibleInRelease => true;
        public string LayerName => "Obraz";

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetValue(ref _isEnabled, value, nameof(IsEnabled));
        }

        private string _source = "https://gallery.dpcdn.pl/imgc/UGC/78161/g_-_-x-_-_-_x20171016194714_0.jpg";
        public string Source
        {
            get => _source;
            set => SetValue(ref _source, value, nameof(Source));
        }

        [XmlIgnore]
        public BaseMediaFileInfo FileInfo
        {
            get => _fileInfo;
            set => SetValue(ref _fileInfo, value, nameof(FileInfo));
        }
        private BaseMediaFileInfo _fileInfo;

        private RelayCommand _playCommand;
        public RelayCommand PlayCommand
            => _playCommand ?? (_playCommand = new RelayCommand(() =>
            {
                ChangeOutputSize();
                EventAggregator.Publish($"{nameof(OutputImageVM)}_Play");                
            }));

        private RelayCommand _stopCommand;
        public RelayCommand StopCommand
            => _stopCommand ?? (_stopCommand = new RelayCommand(() => EventAggregator.Publish($"{nameof(OutputImageVM)}_Stop")));

        private RelayCommand _changeOutputSizeCommand;
        public RelayCommand ChangeOutputSizeCommand
            => _changeOutputSizeCommand ?? (_changeOutputSizeCommand = new RelayCommand(ChangeOutputSize));

        private void ChangeOutputSize()
        {
            if (FileInfo is IResizable resizable)
            {
                EventAggregator.Publish($"{typeof(ILayerVM)}_ChangeOutputSize", new Size(resizable.NaturalWidth, resizable.NaturalHeight));
            }
        }
    }
}
