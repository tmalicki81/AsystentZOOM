using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FileService.Common
{
    [XmlRoot("sync-data")]
    public class SyncData
    {
        [XmlAttribute("last-sync")]
        public DateTime LastSync { get; set; }

        [XmlElement("file-sync")]
        public List<FileSync> FileSyncList { get; set; } = new List<FileSync>();

        public class FileSync 
        {
            [XmlAttribute("file-path")]
            public string FilePathAsString { get; set; }

            [XmlElement("source")]
            public FileInRepository Source { get; set; }

            [XmlElement("target")]
            public FileInRepository Target { get; set; }

            public class FileInRepository
            {
                [XmlAttribute("repository")]
                public string Repository { get; set; }

                [XmlAttribute("file-date-mod")]
                public DateTime FileDateMod { get; set; }
            }
        }
    }
}
