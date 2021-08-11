using AsystentZOOM.VM.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AsystentZOOM.VM.Interfaces.Sortable
{
    public interface ISortableItemProvider : INotifyPropertyChanged
    {
        bool HoldSelected { get; set; }
        bool IsEditing { get; set; }
        bool IsMouseOver { get; set; }
        bool IsNew { get; set; }
        bool IsSelected { get; set; }
        string ItemCategory { get; }
        string ItemName { get; }
        int Lp { get; set; }

        object SelectedItem { get; set; }
        IEnumerable<object> ContainerItemsSource { get; }
        object Container { get; set; }

        RelayCommand DeleteCommand { get; }
        RelayCommand EditCommand { get; }
        RelayCommand ToDownCommand { get; }
        RelayCommand ToUpCommand { get; }
        RelayCommand InsertCommand { get; }
        
        void MoveToEnd();
        void Sort();
        void DropTo(ISortableItemVM targetData);
        ObservableCollection<ISortableItemProvider> GetAllSortableItems();
    }
}
