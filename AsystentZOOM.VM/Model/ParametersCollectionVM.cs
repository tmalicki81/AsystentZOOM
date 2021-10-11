using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Model
{
    public interface IParametersCollectionVM : IBaseVM
    {
        IRelayCommand AddParameterCommand { get; }
        IBaseVM Owner { get; set; }
        ObservableCollection<IParameterVM> Parameters { get; set; }
        bool Trim();
    }

    [Serializable]
    public class ParametersCollectionVM : BaseVM, IParametersCollectionVM
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

        [Parent(typeof(IMeetingVM), typeof(IMeetingPointVM))]
        public IBaseVM Owner
        {
            get => _owner;
            set => SetValue(ref _owner, value, nameof(Owner));
        }
        private IBaseVM _owner;

        #endregion Owner

        #region AddParameterCommand

        private IRelayCommand _addParameterCommand;
        public IRelayCommand AddParameterCommand
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
            if (hasChanged)
                ChangeFromChild(this);
            return hasChanged;
        }

        public override void ChangeFromChild(IBaseVM child)
            => Owner?.ChangeFromChild(this);

        ObservableCollection<IParameterVM> IParametersCollectionVM.Parameters 
        {
            get => Parameters.Convert<ParameterVM, IParameterVM>();
            set => Parameters = value.Convert<IParameterVM, ParameterVM>();
        }

#if(DEBUG)
        public override string ToString() => $"{GetType()}: ( {Parameters?.Count ?? 0} )";
#endif
    }
}
