using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.FileRepositories;
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

        public override bool IsTemporaryFile 
        { 
            get => base.IsTemporaryFile;
            set
            {
                if (base.IsTemporaryFile == value)
                    return;

                // Zmień z pliku tymczasowego na normalny
                if (!value)
                    ChangeFileExtension(FileExtensionEnum.TIM);
                else
                    ChangeFileExtension(FileExtensionEnum.TMP_TIM);
                base.IsTemporaryFile = value;
            }
        }

        private void ChangeFileExtension(FileExtensionEnum fileExtension)
        {
            if (FileExtension == fileExtension)
                return;
            
            string fileExtensionName = Enum.GetName(typeof(FileExtensionEnum), fileExtension).ToLower();
            string newFileName = Path.ChangeExtension(FileName, fileExtensionName);
            
            if (newFileName != FileName)
            {
                var serializer = new CustomXmlSerializer(Content.GetType());
                var local = MediaLocalFileRepositoryFactory.TimePiece;

                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, Content);
                    stream.Position = 0;
                    local.SaveFile(stream, newFileName);
                }
                local.Delete(FileName);
                FileName = newFileName;
            }
            FileExtension = fileExtension;
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
