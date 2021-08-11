using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class IsNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = (parameter as string) == "INVERT";
            if (!(value is string stringValue))
            {
                if (value == null)
                    stringValue = null;
                else
                    throw new ArgumentException(nameof(value));
            }
            bool boolValue = !string.IsNullOrEmpty(stringValue);
            if (invert) boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
