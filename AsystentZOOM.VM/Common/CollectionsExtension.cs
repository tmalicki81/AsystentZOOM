using System.Collections.ObjectModel;
using System.Linq;

namespace AsystentZOOM.VM.Common
{
    public static class CollectionsExtension
    {
        public static ObservableCollection<To> Convert<From, To>(
            this ObservableCollection<From> collection)
        {
            if (collection == null)
                return null;
            if (!collection.Any())
                return new ObservableCollection<To>();
            return new ObservableCollection<To>(collection.Cast<To>());
        }
    }
}
