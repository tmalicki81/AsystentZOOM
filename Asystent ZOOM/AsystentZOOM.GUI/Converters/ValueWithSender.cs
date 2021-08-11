using System;
using System.Globalization;
using System.Windows.Data;

namespace AsystentZOOM.GUI.Converters
{
    public class ValueWithSender<T>
    {
        private class ValueWithSenderConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                => new ValueWithSender<T> { Sender = parameter, Value = (T)value };

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => ((ValueWithSender<T>)value).Value;
        }

        public object Sender { get; set; }
        public T Value { get; set; }
        public static IValueConverter Converter = new ValueWithSenderConverter();
        public static ValueWithSender<T> Default = new ValueWithSender<T>();
    }
}
