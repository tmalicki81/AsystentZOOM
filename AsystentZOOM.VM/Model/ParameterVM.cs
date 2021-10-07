using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Sortable;
using AsystentZOOM.VM.Interfaces.Sortable;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Model
{
    [Serializable]
    public class ParameterVM : BaseVM, ISortableItemVM
    {
        public class SortableParameterProvider : SortableItemProvider<ParameterVM>
        {
            public SortableParameterProvider(ParameterVM parameter) : base(parameter) { }
            public override string ItemCategory => "Parametr";
            public override string ItemName => Item.Key;
            public override bool CanCreateNewItem => true;
            public override ObservableCollection<ParameterVM> ContainerItemsSource => Item.ParametersCollection.Parameters;
            public override ParameterVM NewItem() => new ParameterVM { ParametersCollection = Item.ParametersCollection };
            public override ParameterVM SelectedItem
            {
                get => ContainerItemsSource.FirstOrDefault(x => x.Sorter.IsSelected);
                set
                {
                    foreach (var d in ContainerItemsSource)
                        d.Sorter.IsSelected = d == value;
                }
            }
            public override object Container
            {
                get => Item.ParametersCollection;
                set => Item.ParametersCollection = (ParametersCollectionVM)value;
            }
        }

        public ParameterVM()
            => Sorter = new SortableParameterProvider(this);

        [XmlIgnore]
        public SortableParameterProvider Sorter
        {
            get => _sorter;
            set => SetValue(ref _sorter, value, nameof(Sorter));
        }
        private SortableParameterProvider _sorter;

        ISortableItemProvider ISortableItemVM.Sorter => Sorter;

        [XmlIgnore]
        public ParametersCollectionVM ParametersCollection
        {
            get => _parametersCollection;
            set => SetValue(ref _parametersCollection, value, nameof(ParametersCollection));
        }
        private ParametersCollectionVM _parametersCollection;

        private string _key = "Nowy parametr";
        public string Key
        {
            get => _key;
            set
            {
                if (_key == value) return;
                SetValue(ref _key, value, nameof(Key));
                Sorter.IsNew = false;
                if(ParametersCollection?.Trim() == false)
                    ChangeFromChild(this);
            }
        }

        private string _value = "Nowa wartość";
        public string Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                SetValue(ref _value, value, nameof(Value));
                Sorter.IsNew = false;
                if (ParametersCollection?.Trim() == false)
                    ChangeFromChild(this);
            }
        }

        public override void ChangeFromChild(BaseVM child)
        {
            ParametersCollection?.ChangeFromChild(this);
        }

#if (DEBUG)
        public override string ToString() => $"{Key} : {Value}";
#endif
    }
}
