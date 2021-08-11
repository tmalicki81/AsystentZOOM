using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using static AsystentZOOM.VM.Common.FilePropertiesRoot.File;

namespace AsystentZOOM.VM.Common
{
    [Serializable]
    [XmlRoot("file-properties")]
    public class FilePropertiesRoot
    {
        [XmlAttribute("last-changed")]
        public DateTime LastChanged { get; set; }

        [XmlElement("file")]
        public List<File> FileList { get; set; }

        [Serializable]
        public class File
        {
            [XmlAttribute("file-name")]
            public string FileName { get; set; }

            [XmlElement("property")]
            public List<Property> PropertyList { get; set; }

            [Serializable]
            public class Property
            {
                [XmlAttribute("property-name")]
                public string PropertyName { get; set; }

                [XmlAttribute("value")]
                public string Value { get; set; }

                [XmlAttribute("last-changed")]
                public DateTime LastChanged { get; set; }
            }
        }

        public static File GetFileProperties(string fileName)
        {
            FilePropertiesRoot meta = IsolatedStorageHelper.LoadObject<FilePropertiesRoot>();
            return meta.FileList.FirstOrDefault(x => x.FileName == fileName.Split('\\').Last());
        }

        private static Property GetProperty(string fileName, string propertyName, out FilePropertiesRoot root)
        {
            root = IsolatedStorageHelper.LoadObject<FilePropertiesRoot>();
            var file = root.FileList?.FirstOrDefault(x => x.FileName == fileName.Split('\\').Last());
            if (file == null)
            {
                file = new File
                {
                    FileName = fileName.Split('\\').Last(),
                    PropertyList = new List<Property>()
                };
                if (root.FileList == null)
                    root.FileList = new List<File>();
                root.FileList.Add(file);
                IsolatedStorageHelper.SaveObject(root);
            }
            Property property = file.PropertyList.FirstOrDefault(p => p.PropertyName == propertyName);
            if (property == null)
            {
                property = new Property
                {
                    PropertyName = propertyName
                };
            }
            return property;
        }

        public static void SetProperty(string fileName, string propertyName, string value)
        {
            Property property = GetProperty(fileName, propertyName, out FilePropertiesRoot root);
            property.Value = value;
            root.LastChanged = DateTime.Now;
            property.LastChanged = DateTime.Now;
            IsolatedStorageHelper.SaveObject(root);
        }

        public static string GetString(string fileName, string propertyName)
            => GetProperty(fileName, propertyName, out _)?.Value;        
    }
}
