using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ShortTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // title, maxLength
            if (!(value is string title) || string.IsNullOrEmpty(title)) return null;
            int maxLength = (parameter is string strParameter) ? int.Parse(strParameter) : 20;

            if (title.Length > maxLength)
                return title.Substring(0, maxLength - 3) + "...";
            return title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
