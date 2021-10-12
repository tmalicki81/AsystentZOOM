using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Interfaces.Sortable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace AsystentZOOM.VM.Common.Sortable
{
    [Serializable]
    public abstract class SortableItemProvider<TItem> : BaseVM, ISortableItemProvider
        where TItem : class, IBaseVM, ISortableItemVM
    {
        public SortableItemProvider(TItem parameter) => Item = parameter;

        /// <summary>
        /// Kategoria elementu (np. plik, punkt)
        /// </summary>
        public abstract string ItemCategory { get; }

        /// <summary>
        /// Nazwa elmentu (np. Plik2.mp3)
        /// </summary>
        public abstract string ItemName { get; }

        /// <summary>
        /// Elementy kontenera (np. Plik1.mp3, Plik2.mp3, Plik3.mp3)
        /// </summary>
        public abstract ObservableCollection<TItem> ContainerItemsSource { get; }

        /// <summary>
        /// Tworzenie nowego elementu
        /// </summary>
        /// <returns></returns>
        public virtual TItem NewItem() => (TItem)Activator.CreateInstance(typeof(TItem));

        /// <summary>
        /// Wybrany element
        /// </summary>
        public abstract TItem SelectedItem { get; set; }

        /// <summary>
        /// Czy można tworzyć nowe element
        /// </summary>
        public abstract bool CanCreateNewItem { get; }

        /// <summary>
        /// Kontener dla elementów
        /// </summary>
        public abstract object Container { get; set; }

        /// <summary>
        /// Pobieranie wszystkich sorterów (klas zarządzających pozycją elementu w kontenerze)
        /// we wszystkich elementach w kontenerze
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ISortableItemProvider> GetAllSortableItems()
            => new ObservableCollection<ISortableItemProvider>(ContainerItemsSource.Select(x => x.Sorter));

        /// <summary>
        /// Element, który posiada sorter
        /// </summary>
        public TItem Item { get; }

        /// <summary>
        /// Liczba porządkowa w kontenerze
        /// </summary>
        public int Lp
        {
            get => _Lp;
            set => SetValue(ref _Lp, value, nameof(Lp));
        }
        private int _Lp;

        /// <summary>
        /// Czyelement jest nowy
        /// </summary>
        public bool IsNew
        {
            get => _isNew;
            set => SetValue(ref _isNew, value, nameof(IsNew));
        }
        private bool _isNew = true;

        /// <summary>
        /// Czy najechano na element wskaźnikiem myszy
        /// </summary>
        public bool IsMouseOver
        {
            get => _isMouseOver;
            set => SetValue(ref _isMouseOver, value, nameof(IsMouseOver));
        }
        private bool _isMouseOver;

        /// <summary>
        /// Czy element jest edytowany
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set => SetValue(ref _isEditing, value, nameof(IsEditing));
        }
        private bool _isEditing;

        /// <summary>
        /// Czy element jest zaznaczony
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetValue(ref _isSelected, value, nameof(IsSelected));
        }
        private bool _isSelected;

        /// <summary>
        /// Czy element jest zaznaczony (pomimo działania użytkownika)
        /// </summary>
        public bool HoldSelected
        {
            get => _holdSelected;
            set => SetValue(ref _holdSelected, value, nameof(HoldSelected));
        }
        private bool _holdSelected;

        /// <summary>
        /// Przeniesienie elementu na koniec listy
        /// </summary>
        public void MoveToEnd()
        {
            ContainerItemsSource.Remove(Item);
            ContainerItemsSource.Add(Item);
            Sort();
            RaiseCanExecuteChanged4All();
        }

        /// <summary>
        /// Sortowanie elementów
        /// </summary>
        public void Sort()
        {
            int lp = 1;
            foreach (var item in ContainerItemsSource)
                item.Sorter.Lp = lp++;
        }

        /// <summary>
        /// Przeniesienie elementu w górę
        /// </summary>
        public RelayCommand ToUpCommand
            => _toUpCommand ??= new RelayCommand(ToUp, () => ContainerItemsSource?.FirstOrDefault() != Item);
        private RelayCommand _toUpCommand;

        private void ToUp()
        {
            Sort();
            ContainerItemsSource.Remove(Item);
            ContainerItemsSource.Insert(Item.Sorter.Lp - 2, Item);
            Sort();
            RaiseCanExecuteChanged4All();
            Item.CallChangeToParent(this);
        }

        /// <summary>
        /// Przeniesienie elementu w dół
        /// </summary>
        public RelayCommand ToDownCommand
            => _toDownCommand ??= new RelayCommand(ToDown, () => ContainerItemsSource?.LastOrDefault() != Item);
        private RelayCommand _toDownCommand;

        private void ToDown()
        {
            Sort();
            ContainerItemsSource.Remove(Item);
            ContainerItemsSource.Insert(Item.Sorter.Lp, Item);
            Sort();
            RaiseCanExecuteChanged4All();
            Item.CallChangeToParent(this);
        }

        /// <summary>
        /// Dodanie elementu
        /// </summary>
        public RelayCommand InsertCommand
            => _insertCommand ??= new RelayCommand(Insert);
        private RelayCommand _insertCommand;

        private void Insert()
        {
            var newParameter = NewItem();
            Sort();
            ContainerItemsSource.Insert(Item.Sorter.Lp - 1, newParameter);
            Sort();
            Item.CallChangeToParent(this);
        }

        /// <summary>
        /// Edycja elementu
        /// </summary>
        public RelayCommand EditCommand
            => _editCommand ??= new RelayCommand(Edit, () => !IsEditing);
        private RelayCommand _editCommand;

        private void Edit()
            => IsEditing = true;

        /// <summary>
        /// Usunięcie elementu z kontenera
        /// </summary>
        public RelayCommand DeleteCommand
            => _deleteCommand ??= new RelayCommand(Delete);
        private RelayCommand _deleteCommand;

        private void Delete()
        {
            HoldSelected = true;
            try
            {
                IsSelected = true;
                var result = DialogHelper.ShowMessageBox(
                    $"Czy na pewno chcesz usunąć {ItemCategory} {ItemName}?",
                    $"Usuwanie elementu {ItemName}",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    ContainerItemsSource.Remove(Item);
                    Item.CallChangeToParent(this);
                }
            }
            finally
            {
                HoldSelected = false;
                IsSelected = false;
            }
        }

        /// <summary>
        /// Obsługa zrzucania elementu na inny element lub kontener
        /// </summary>
        /// <param name="targetData">Cel zrzucania</param>
        public void DropTo(ISortableItemVM targetData)
        {
            ISortableItemVM droppedData = Item;
            if (droppedData == targetData || droppedData == null)
                return;

            var droppedSorter = droppedData.Sorter; 
            var targetSorter = targetData.Sorter;

            if (droppedSorter.Container.GetType() == targetSorter.Container.GetType())
            {
                var droppedItemsSource = droppedSorter.ContainerItemsSource as IList;
                var targetItemsSource = targetSorter.ContainerItemsSource as IList;

                targetSorter.IsSelected = false;
                droppedSorter.IsSelected = true;

                targetSorter.SelectedItem = droppedData;
                droppedSorter.Sort();
                targetSorter.Sort();
                droppedItemsSource.Remove(droppedData);
                int index = targetSorter.Lp < targetSorter.ContainerItemsSource.Count() || targetSorter.Container == droppedSorter.Container
                    ? targetSorter.Lp - 1
                    : targetSorter.Lp;
                targetItemsSource.Insert(index, droppedData);
                droppedSorter.SelectedItem = null;
                targetSorter.SelectedItem = droppedData;
                droppedSorter.Container = targetSorter.Container;

                int lp = 1;
                foreach (var item in droppedItemsSource)
                    ((ISortableItemVM)item).Sorter.Lp = lp++;
                lp = 1;
                foreach (var item in targetItemsSource)
                    ((ISortableItemVM)item).Sorter.Lp = lp++;
            }
            else if (droppedSorter.Container.GetType() == targetData.GetType())
            {
                var droppedItemsSource = (IList)droppedSorter.ContainerItemsSource;
                var targetItemsSourceProperty = targetData.GetType()
                    .GetProperties()
                    .Where(p => p.PropertyType == droppedData.Sorter.ContainerItemsSource.GetType())
                    .Single();
                var targetItemsSource = (IList)targetItemsSourceProperty.GetValue(targetData);

                droppedItemsSource.Remove(droppedData);
                targetItemsSource.Add(droppedData);
                droppedSorter.Container = targetData;

                int lp = 1;
                foreach (var item in droppedItemsSource)
                    ((ISortableItemVM)item).Sorter.Lp = lp++;
                lp = 1;
                foreach (var item in targetItemsSource)
                    ((ISortableItemVM)item).Sorter.Lp = lp++;
            }
            else
            { 
            }
            Item.CallChangeToParent(this);
        }

        #region Niejawna implementacja interfejsu ISortableItemProvider

        object ISortableItemProvider.SelectedItem
        {
            get => SelectedItem;
            set => SelectedItem = (TItem)value;
        }

        IEnumerable<object> ISortableItemProvider.ContainerItemsSource => ContainerItemsSource.Cast<object>();

        #endregion Niejawna implementacja interfejsu ISortableItemProvider
    }
}
