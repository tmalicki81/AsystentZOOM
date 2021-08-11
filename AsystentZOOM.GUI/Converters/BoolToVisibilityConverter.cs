using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = ((parameter as string) ?? string.Empty).IndexOf("INVERT") != -1;
            bool colapse = ((parameter as string) ?? string.Empty).IndexOf("COLAPSE") != -1;
            if (!(value is bool boolValue))
                throw new ArgumentException(nameof(value));
            if (invert) boolValue = !boolValue;
            var result = boolValue
                ? Visibility.Visible
                : (colapse)
                    ? Visibility.Collapsed
                    : Visibility.Hidden;

            if (((parameter as string) ?? string.Empty).IndexOf("IsCurrent") != -1)
            { 
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
