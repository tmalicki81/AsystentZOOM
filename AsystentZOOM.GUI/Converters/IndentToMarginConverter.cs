using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    public class IndentToMarginConverter : IValueConverter
    {
        private const int IndentToPixelsMultipler = 40;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueAsString = (value ?? string.Empty).ToString();
            string parameterAsString = (parameter ?? string.Empty).ToString().Replace(",", " "); 
            
            var conv = new ThicknessConverter();
            
            Thickness thickness = conv.IsValid(parameterAsString)
                ? (Thickness)conv.ConvertFromString(parameterAsString)
                : new Thickness();
            int.TryParse(valueAsString, out int indent);
            return new Thickness(thickness.Left + indent * IndentToPixelsMultipler, thickness.Top, thickness.Right, thickness.Bottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
