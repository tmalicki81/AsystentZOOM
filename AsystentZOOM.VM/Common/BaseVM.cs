using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Common
{
    public interface IBaseVM : IXmlDeserializationCallback, INotifyPropertyChanged
    {
        string InstanceId { get; set; }
        void CallChangeToParent(IBaseVM child);
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
        public virtual bool IsDataReady { get; set; }

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
            SetParentValuesInChild(sender);
        }

        private void SetParentValuesInChild(object parentValue)
        {
            if (parentValue == null)
                return;

            Type parentType = parentValue.GetType();
            var properties = parentType.GetProperties().Where(p =>
                    p.DeclaringType == parentType &&
                    p.GetMethod != null &&
                    p.SetMethod != null)
                .ToArray();

            foreach (var p in properties)
            {
                object value = p.GetValue(this);
                if (value is ICollection collection)
                    foreach (var item in collection)
                        SetParentValuesInChild(parentValue, item);
                else
                    SetParentValuesInChild(parentValue, p.GetValue(parentValue));
            }
        }

        private void SetParentValuesInChild(object parentValue, object childValue)
        {
            if (parentValue == null || childValue == null)
                return;

            Type parentType = parentValue.GetType();
            Type childType = childValue.GetType();

            var parentTypeInterfaces = parentType.GetInterfaces();
            if (!parentTypeInterfaces.Any())
                return;

            var parentProperties = GetParentProperties(childType, parentTypeInterfaces);

            foreach (var parentProperty in parentProperties)
                parentProperty.SetValue(childValue, parentValue);
        }

        private PropertyInfo[] GetParentProperties(Type childType, Type[] parentTypeInterfaces)
        {
            return childType.GetProperties().Where(p =>
                p.DeclaringType == childType &&
                p.GetMethod != null &&
                p.SetMethod != null &&
                (p
                    .GetCustomAttributes(typeof(ParentAttribute), false)
                    .FirstOrDefault() as ParentAttribute
                )?.Interfaces?.Any(itemInterface => parentTypeInterfaces == null ||
                                                    parentTypeInterfaces.Any(i => i == itemInterface)
                                  ) == true)
                .ToArray();
        }

        private PropertyInfo[] GetParentProperties()
            => GetParentProperties(GetType(), null);

        /// <summary>
        /// Sygnał o zmianie stanu dziecka przekazywany rodzicowi
        /// </summary>
        public virtual void CallChangeToParent(IBaseVM child)
        {
            var parentProperties = GetParentProperties();
            foreach (PropertyInfo p in parentProperties)
            {
                var parentPropertyValue = p.GetValue(this);
                if (parentPropertyValue != null)
                {
                    MethodInfo method = parentPropertyValue.GetType().GetMethod(nameof(CallChangeToParent));
                    method.Invoke(parentPropertyValue, new object[] { this });
                }
            }
        }
    }
}
