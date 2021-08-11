using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ShortFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // fileName, maxLength
            if (!(value is string fileName) || string.IsNullOrEmpty(fileName)) return null;
            int maxLength = (parameter is string strParameter) ? int.Parse(strParameter) : 20;

            string fileShortName = fileName.Split('\\').Last();
            string fileExtension = fileShortName.Split('.').Last();
            string fileWithoutExtension = fileShortName.Substring(0, fileShortName.Length - fileExtension.Length);

            if (fileShortName.Length > maxLength)
                return fileWithoutExtension.Substring(0, maxLength - fileExtension.Length - 3) + "..." + fileExtension;
            return fileShortName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
