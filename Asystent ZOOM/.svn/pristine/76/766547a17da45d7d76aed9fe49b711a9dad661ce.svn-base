using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBoolConverter : IValueConverter
    {
        private Enum GetMatchingValueFromParameter(Type targetType, object parameter)
            => (Enum)Enum.Parse(targetType, parameter.ToString());

        private Enum GetNoMatchingValueFromParameter(Type targetType, object parameter)
        {
            string name = Enum.GetNames(targetType).Where(x => x != parameter.ToString()).Single();
            return (Enum)Enum.Parse(targetType, name);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value.ToString() == parameter.ToString();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value
                   ? GetMatchingValueFromParameter(targetType, parameter)
                   : GetNoMatchingValueFromParameter(targetType, parameter);
    }
}
