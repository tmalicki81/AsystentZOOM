using AsystentZOOM.VM.Attributes;
using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Common
{
    public interface IBaseVM : IXmlDeserializationCallback, INotifyPropertyChanged
    {
        string InstanceId { get; set; }
        void CallChangeToParent(IBaseVM child, string description);
        void RaiseCanExecuteChanged4All();
        void RaisePropertyChanged(string propertyName);
    }

    /// <summary>
    /// Klasa bazowa dla wszystkich ViewModel-i
    /// </summary>
    [Serializable]
    public abstract class BaseVM : INotifyPropertyChanged, IBaseVM
    {
        internal class PropertyAndParents
        {
            internal static Dictionary<Type, List<PropertyAndParents>> Types = new(); 
            
            internal PropertyInfo Property { get; set; }
            internal bool IsCollection { get; set; }
            internal PropertyInfo[] ParentProperties { get; set; }

            
            public override string ToString()
                => $"{Property.Name}";
        }

        public BaseVM()
        {
            Type parentType = GetType();
            if (!PropertyAndParents.Types.ContainsKey(parentType))
            {
                PropertyAndParents.Types.Add(parentType, new List<PropertyAndParents>());
                var properties = PropertyAndParents.Types[parentType];
                foreach (PropertyInfo property in parentType.GetProperties())
                {
                    Type propertyType = property.PropertyType;
                    var propertyInterfaces = propertyType.GetInterfaces();
                    var parentInterfaces = parentType.GetInterfaces();
                    if (propertyInterfaces.Any(i => i == typeof(ICollection)) &&
                        propertyType.GenericTypeArguments.Count() == 1 &&
                        propertyType.GenericTypeArguments.First() is Type childType &&
                        GetParentProperties(childType, parentInterfaces) is PropertyInfo[] parentProperties &&
                        parentProperties.Any())
                    {
                        properties.Add(new PropertyAndParents
                        {
                            Property = property,
                            IsCollection = true,
                            ParentProperties = parentProperties
                        });
                    }
                    else
                    {
                        PropertyInfo[] parentProperties1 = GetParentProperties(propertyType, parentInterfaces);
                        if (parentProperties1.Any())
                        {
                            properties.Add(new PropertyAndParents
                            {
                                Property = property,
                                IsCollection = false,
                                ParentProperties = parentProperties1
                            });
                        }
                    }
                }
            }
        }

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
            Type type = GetType();
            PropertyInfo property = type.GetProperty(propertyName);
            var mappedProperty = PropertyAndParents.Types[type].FirstOrDefault(x => x.Property == property);
            if (value != null && mappedProperty != null)
            {
                if (!mappedProperty.IsCollection)
                {
                    foreach (var pi in mappedProperty.ParentProperties)
                        pi.SetValue(value, this);
                }
                else
                {
                    // Przypnij zdarzenia zmiany kolekcji
                    if (field is INotifyCollectionChanged oldCollection)
                        oldCollection.CollectionChanged -= (s, e) => Value_CollectionChanged(property, s, e);
                    if (value is INotifyCollectionChanged newCollection)
                        newCollection.CollectionChanged += (s, e) => Value_CollectionChanged(property, s, e);

                    // Podmień wartości Parent
                    if (value is ICollection collection)
                    {
                        foreach (var item in collection)
                            foreach (var pi in mappedProperty.ParentProperties)
                                pi.SetValue(item, this);
                    }
                }
            }

            // Zmień wartość i poinformuj subskrybenta
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Odśwież dostępność poleceń
            if (raiseCanExecuteChanged4All)
                RaiseCanExecuteChanged4All();

            // Zawsze zwracaj true
            return true;
        }

        private void Value_CollectionChanged(PropertyInfo property, object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = e.NewItems;
            if (collection != null && collection.Count > 0)
            {
                Type type = GetType();
                var mappedProperty = PropertyAndParents.Types[type].FirstOrDefault(x => x.Property == property);
                if (mappedProperty != null)
                {
                    // Podmień wartości Parent
                    foreach (var item in collection)
                        foreach (var pi in mappedProperty.ParentProperties)
                            pi.SetValue(item, this);
                }
            }
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
            if (sender == null)
                return;
            Type type = sender.GetType();
            List<PropertyAndParents> mappedProperties = PropertyAndParents.Types[type];
            foreach (var mappedProperty in mappedProperties)
            {
                object value = mappedProperty.Property.GetValue(sender);
                if (value == null)
                    continue;
                if (!mappedProperty.IsCollection)
                {
                    foreach (var pi in mappedProperty.ParentProperties)
                        pi.SetValue(value, sender);
                }
                else
                {
                    if (value is ICollection collection)
                    {
                        foreach (var item in collection)
                            foreach (var pi in mappedProperty.ParentProperties)
                                pi.SetValue(item, sender);
                    }
                }
            }
        }

        private PropertyInfo[] GetParentProperties(Type childType, Type[] parentTypeInterfaces)
        {
            return childType.GetProperties().Where(p =>
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

        private PropertyInfo[] _parentProperties;

        private PropertyInfo[] GetParentProperties()
            => GetParentProperties(GetType(), null);

        /// <summary>
        /// Sygnał o zmianie stanu dziecka przekazywany rodzicowi
        /// </summary>
        public virtual void CallChangeToParent(IBaseVM child, string description)
        {
            if (_parentProperties == null)
                _parentProperties = GetParentProperties();
            foreach (PropertyInfo p in _parentProperties)
            {
                var parentPropertyValue = p.GetValue(this);
                if (parentPropertyValue != null)
                {
                    MethodInfo method = parentPropertyValue.GetType().GetMethod(nameof(CallChangeToParent));
                    method.Invoke(parentPropertyValue, new object[] { child, description });
                }
            }
        }
    }
}