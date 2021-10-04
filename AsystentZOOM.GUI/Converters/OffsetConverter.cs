using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    public class OffsetConverter : IValueConverter
    {
        public static OffsetConverter Instance = new OffsetConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dValue = (double)value;
            double offset = double.Parse(parameter.ToString());
            return dValue + offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dValue = (double)value;
            double offset = double.Parse(parameter.ToString());
            return dValue - offset;
        }
    }
}
