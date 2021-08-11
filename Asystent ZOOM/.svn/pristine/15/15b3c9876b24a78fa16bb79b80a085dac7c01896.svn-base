using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using System;
using System.Windows;

namespace AsystentZOOM.VM.ViewModel
{
    [Serializable]
    public class VideoVM : PlayerVM, ILayerVM
    {
        public bool IsVisibleInRelease => true;
        public string LayerName => "Video";

        private int _naturalVideoWidth;
        public int NaturalVideoWidth
        {
            get => _naturalVideoWidth;
            set => SetValue(ref _naturalVideoWidth, value, nameof(NaturalVideoWidth));
        }

        private int _naturalVideoHeight;
        public int NaturalVideoHeight
        {
            get => _naturalVideoHeight;
            set => SetValue(ref _naturalVideoHeight, value, nameof(NaturalVideoHeight));
        }

        private RelayCommand _changeOutputSizeCommand;
        public RelayCommand ChangeOutputSizeCommand
            => _changeOutputSizeCommand ??= new RelayCommand(() =>
            {
                EventAggregator.Publish($"{typeof(ILayerVM)}_ChangeOutputSize", new Size(NaturalVideoWidth, NaturalVideoHeight));
            },
            () => PlayerState != PlayerStateEnum.Stopped);

        BaseMediaFileInfo ILayerVM.FileInfo
        {
            get => (BaseMediaFileInfo)base.FileInfo;
            set => base.FileInfo = (IMovable)value;
        }
    }
}