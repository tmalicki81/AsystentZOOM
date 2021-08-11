using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.ViewModel
{
    [Serializable]
    public class BackgroundVM : SingletonBaseVM, ILayerVM
    {
        public bool IsVisibleInRelease => true;
        public string LayerName => "Tło";

        private bool _IsEnabled = true;
        public bool IsEnabled 
        {
            get => _IsEnabled;
            set => SetValue(ref _IsEnabled, value, nameof(IsEnabled));
        }

        private Color _backgroundColor = Colors.Black;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetValue(ref _backgroundColor, value, nameof(BackgroundColor));
        }

        private GradientDirectionEnum _gradientDirection = GradientDirectionEnum.TopToDown;
        public GradientDirectionEnum GradientDirection
        {
            get => _gradientDirection;
            set => SetValue(ref _gradientDirection, value, nameof(GradientDirection));
        }

        private Color _gradientColor = Colors.Gray;
        public Color GradientColor
        {
            get => _gradientColor;
            set => SetValue(ref _gradientColor, value, nameof(GradientColor));
        }

        private double _gradientOffset = 1;
        public double GradientOffset
        {
            get => _gradientOffset;
            set => SetValue(ref _gradientOffset, value, nameof(GradientOffset));
        }

        private double _opacity = 0.88;
        public double Opacity
        {
            get => _opacity;
            set => SetValue(ref _opacity, value, nameof(Opacity));
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
            => _playCommand ??= new RelayCommand(() => EventAggregator.Publish($"{nameof(BackgroundVM)}_Play"));

        private RelayCommand _stopCommand;
        public RelayCommand StopCommand
            => _stopCommand ??= new RelayCommand(() => EventAggregator.Publish($"{nameof(BackgroundVM)}_Stop"));
    }
}