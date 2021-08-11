using AsystentZOOM.VM.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AsystentZOOM.VM.Model
{
    [Serializable]
    public class ParametersCollectionVM : BaseVM, IDisposable
    {
        private ObservableCollection<ParameterVM> _parameters = new ObservableCollection<ParameterVM>();
        public ObservableCollection<ParameterVM> Parameters
        {
            get => _parameters;
            set => SetValue(ref _parameters, value, nameof(Parameters));
        }

        private RelayCommand _addParameterCommand;
        public RelayCommand AddParameterCommand
            => _addParameterCommand ?? (_addParameterCommand = new RelayCommand(AddParameter));

        private void AddParameter()
        {
            var newParameter = new ParameterVM
            {
                ParametersCollection = this,
            };
            newParameter.Sorter.MoveToEnd();
        }

        public void Trim()
        {
            if (Parameters == null) return;
            foreach (var i in Parameters.ToList())
                if (string.IsNullOrEmpty(i.Key) && string.IsNullOrEmpty(i.Value))
                    Parameters.Remove(i);
            SetOrder();
        }

        public void SetOrder()
        {
            int lp = 1;
            foreach (var i in Parameters)
                i.Sorter.Lp = lp++;
        }

        public override void OnDeserialized(object sender)
        {
            if (Parameters == null) return;
            foreach (var p in Parameters)
                p.ParametersCollection = this;
        }

        public void Dispose()
        {
        }

#if(DEBUG)
        public override string ToString() => $"{GetType()}: ( {Parameters?.Count ?? 0} )";
#endif
    }
}
