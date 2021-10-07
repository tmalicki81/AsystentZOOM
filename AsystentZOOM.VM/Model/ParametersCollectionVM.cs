using AsystentZOOM.VM.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Model
{
    [Serializable]
    public class ParametersCollectionVM : BaseVM, IDisposable
    {
        #region Parameters

        private ObservableCollection<ParameterVM> _parameters = new();
        public ObservableCollection<ParameterVM> Parameters
        {
            get => _parameters;
            set
            {
                SetValue(ref _parameters, value, nameof(Parameters));
                ChangeFromChild(this);
            }
        }

        #endregion Parameters

        #region Owner

        [XmlIgnore]
        public BaseVM Owner
        {
            get => _owner;
            set => SetValue(ref _owner, value, nameof(Owner));
        }
        private BaseVM _owner;

        #endregion Owner

        #region AddParameterCommand

        private RelayCommand _addParameterCommand;
        public RelayCommand AddParameterCommand
            => _addParameterCommand ??= new RelayCommand(AddParameter);

        private void AddParameter()
        {
            var newParameter = new ParameterVM
            {
                ParametersCollection = this,
            };
            newParameter.Sorter.MoveToEnd();
            ChangeFromChild(newParameter);
        }

        #endregion AddParameterCommand

        public bool Trim()
        {
            if (Parameters == null) return false;
            bool hasChanged = false;
            foreach (var i in Parameters.ToList())
            {
                if (string.IsNullOrEmpty(i.Key) && string.IsNullOrEmpty(i.Value))
                {
                    Parameters.Remove(i);
                    hasChanged = true;
                }
            }

            int lp = 1;
            foreach (var i in Parameters)
            {
                lp++;
                if (i.Sorter.Lp != lp)
                {
                    i.Sorter.Lp = lp;
                    hasChanged = true;
                }
            }
            if(hasChanged)
                ChangeFromChild(this);
            return hasChanged;
        }

        public override void OnDeserialized(object sender)
        {
            if (Parameters == null) return;
            foreach (var p in Parameters)
                p.ParametersCollection = this;
        }

        public override void ChangeFromChild(BaseVM child)
        {
            Owner?.ChangeFromChild(child);
        }

        public void Dispose()
        {
        }

#if(DEBUG)
        public override string ToString() => $"{GetType()}: ( {Parameters?.Count ?? 0} )";
#endif
    }
}
