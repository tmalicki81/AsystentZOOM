using AsystentZOOM.VM.Model;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    public class SourceFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string shortFileName = (string)value;
            if (string.IsNullOrEmpty(shortFileName))
                return null;

            string correctFileDir = shortFileName.Contains("://")
                ? string.Empty
                : BaseMediaFileInfo.GetFileExtensionConfig(shortFileName).MediaLocalFileRepository.RootDirectory;
            return Path.Combine(correctFileDir, shortFileName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
