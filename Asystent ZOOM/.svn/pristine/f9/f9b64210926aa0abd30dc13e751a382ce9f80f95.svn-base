using AsystentZOOM.VM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace AsystentZOOM.GUI.Converters
{
    public class BookmarksToDoubleCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<BookmarkVM> bookmarks))
                throw new ArgumentException(nameof(value));
            return bookmarks?.Any() == true 
                ? new DoubleCollection(bookmarks.Select(x => x.Position.TotalSeconds))
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
