using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;

namespace AsystentZOOM.VM.Model
{
    [Serializable]
    public class VideoFileInfo : PlayerFileInfo<VideoVM>
    {
        public override ILayerVM CreateContent(string fileName)
            => new VideoVM { Source = fileName };
    }
}