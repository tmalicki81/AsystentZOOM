using AsystentZOOM.VM.Common;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    public class ImageEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ImageEnum image))
                return null;
            return Application.Current.FindResource(Enum.GetName(typeof(ImageEnum), image));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
