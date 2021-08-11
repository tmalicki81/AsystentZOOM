using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;

namespace AsystentZOOM.VM.Model
{
    [Serializable]
    public class AudioFileInfo : PlayerFileInfo<AudioVM>
    {
        public override ILayerVM CreateContent(string fileName)
            => new AudioVM 
                { 
                    Source = fileName ,
                    FileInfo = this
                };
    }
}