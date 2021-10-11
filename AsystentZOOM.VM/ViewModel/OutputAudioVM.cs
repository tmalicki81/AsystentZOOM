using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;

namespace AsystentZOOM.VM.ViewModel
{
    public class OutputAudioVM : OutputPlayerVM, ILayerVM
    {
        public string LayerName => "Audio";

        public bool IsVisibleInRelease => true;

        BaseMediaFileInfo ILayerVM.FileInfo
        {
            get => (BaseMediaFileInfo)FileInfo;
            set => FileInfo = (IMovable)value;
        }
    }
}