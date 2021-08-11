using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.IO;
using System.Text;

namespace AsystentZOOM.VM.Model
{
    public class TimePieceFileInfo : BaseMediaFileInfo<TimePieceVM>
    {
        public override ILayerVM CreateContent(string fileName)
        {
            var fileExtensionConfig = GetFileExtensionConfig(fileName);
            string fullFileName = Path.Combine(fileExtensionConfig.MediaLocalFileRepository.RootDirectory, fileName);
            Type t = typeof(TimePieceVM);
            var xmlSerializer = new CustomXmlSerializer(t);
            using (var stream = File.OpenRead(fullFileName))
            using (new SingletonInstanceHelper(t))
            {
                stream.Position = 0;
                return (TimePieceVM)xmlSerializer.Deserialize(stream);
            }
        }

        public override void FillMetadata()
        {
            base.FillMetadata();
            if (string.IsNullOrEmpty(Title))
            {
                Title = new StringBuilder()
                    .Append(Content.TextAbove)
                    .Append(" ... ")
                    .Append(Content.TextBelow)
                    .ToString();
            }
        }
    }
}
