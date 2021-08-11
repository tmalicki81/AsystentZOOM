using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Model;

namespace AsystentZOOM.VM.Interfaces
{
    public interface ILayerVM
    {
        string LayerName { get; }
        bool IsVisibleInRelease { get; }
        bool IsEnabled { get; set; }
        public BaseMediaFileInfo FileInfo { get; set; }
        RelayCommand PlayCommand { get; }
        RelayCommand StopCommand { get; }
    }
}
