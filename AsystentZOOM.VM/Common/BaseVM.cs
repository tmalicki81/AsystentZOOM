using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Common
{
    public interface IBaseVM : IXmlDeserializationCallback
    {
        string InstanceId { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
        void ChangeFromChild(IBaseVM child);
        void RaiseCanExecuteChanged4All();
        void RaisePropertyChanged(string propertyName);
    }

    /// <summary>
    /// Klasa bazowa dla wszystkich ViewModel-i
    /// </summary>
    [Serializable]
    public abstract class BaseVM : INotifyPropertyChanged, IBaseVM
    {
        /// <summary>
        /// Lista poleceń
        /// </summary>
        private List<IRelayCommand> _commandList;

        /// <summary>
        /// Identyfikator GUID obiektu
        /// </summary>
        [XmlAttribute("instance-id")]
        public string InstanceId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Czy dane są juz kompletne (po serializacji lub po kopiowaniu wartosci z innego VM)
        /// </summary>
        [XmlIgnore]
        public bool IsDataReady { get; set; }

        /// <summary>
        /// Sygnał o zmianie stanu dziecka przekazywany rodzicowi
        /// </summary>
        public virtual void ChangeFromChild(IBaseVM child)
        {
        }

        /// <summary>
        /// Odświeżenie możliwości wykonania wszystkich poleceń
        /// </summary>
        public void RaiseCanExecuteChanged4All()
        {
            if (_commandList == null)
            {
                _commandList = GetType().GetProperties()
                    .Where(x => x.PropertyType.GetInterfaces().Any(i => i == typeof(IRelayCommand)))
                    .Where(x => x.GetMethod != null)
                    .Select(x => (IRelayCommand)x.GetValue(this))
                    .ToList();
            }
            foreach (var command in _commandList)
                command.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Zdarzenie zmiany wartości właściwości
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Ustawienie nowej wartości własciwosci
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">Pole przechowujące wartość</param>
        /// <param name="value">Nowa wartość</param>
        /// <param name="propertyName">Nazwa właściwości</param>
        /// <param name="raiseCanExecuteChanged4All">Czy odświeżać możliwość wykonania wszystkich poleceń</param>
        /// <returns></returns>
        protected virtual bool SetValue<T>(ref T field, T value, string propertyName, bool raiseCanExecuteChanged4All = true)
        {
            //bool wasSetup;
            //if (field is IComparable fieldComp && value is IComparable valueComp)
            //    wasSetup = fieldComp.CompareTo(valueComp) != 0;
            //else
            //    wasSetup = field != null && !field.Equals(value);
            //if (wasSetup)
            //{
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //}
            //return wasSetup;

            if (raiseCanExecuteChanged4All)
                RaiseCanExecuteChanged4All();
            return true;
        }

        /// <summary>
        /// Wymuszenie poinformowania widoku o zmienia wartości właściwości
        /// </summary>
        /// <param name="propertyName"></param>
        public void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Operacja, którą należy wykonac po deserializacji
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnDeserialized(object sender)
        {
        }
    }
}
