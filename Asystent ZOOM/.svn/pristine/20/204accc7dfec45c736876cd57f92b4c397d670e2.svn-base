using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.IO;

namespace AsystentZOOM.VM.Model
{
    public class BackgroundFileInfo : BaseMediaFileInfo<BackgroundVM>
    {
        public override ILayerVM CreateContent(string fileName)
        {
            Type t = typeof(BackgroundVM);
            var xmlSerializer = new CustomXmlSerializer(t);
            using (var stream = File.OpenRead(fileName))
            using (new SingletonInstanceHelper(t))
            {
                stream.Position = 0;
                return (BackgroundVM)xmlSerializer.Deserialize(stream);
            }
        }
    }
}
