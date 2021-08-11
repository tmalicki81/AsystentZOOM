using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class AnyValueNotContainsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueString = (value ?? string.Empty).ToString();
            string parameterString = (parameter ?? string.Empty).ToString();
            return valueString != parameterString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
