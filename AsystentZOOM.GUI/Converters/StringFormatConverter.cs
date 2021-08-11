using System;
using System.Globalization;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value ?? string.Empty).ToString() == string.Empty)
                return null;
            if (parameter == null) 
                return (value ?? string.Empty).ToString();
            string parameterString = parameter.ToString();
            dynamic valueDynamic = value;

            if (valueDynamic is TimeSpan valueTimespan)
                return $"{valueTimespan.Hours.ToString().PadLeft(2, '0')}:{valueTimespan.Minutes.ToString().PadLeft(2, '0')}:{valueTimespan.Seconds.ToString().PadLeft(2, '0')}";
            return valueDynamic.ToString(parameterString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
