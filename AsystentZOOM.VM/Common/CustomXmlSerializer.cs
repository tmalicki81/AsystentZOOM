using AsystentZOOM.VM.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Common
{
    /// <summary>
    /// Serializer z obsługą 
    /// </summary>
    public class CustomXmlSerializer : XmlSerializer
    {
        public CustomXmlSerializer(Type type) : base(type)
        {
        }

        /// <summary>
        /// Deserializuj wgłąb
        /// </summary>
        /// <param name="value">Wartość</param>
        /// <param name="visitatedObjects">Obiekty, które zostały zdeserializowane</param>
        private void DeserializePropertyValue(object value, List<object> visitatedObjects)
        {
            if (value == null || visitatedObjects.Any(x => x == value))
                return;

            visitatedObjects.Add(value);

            // Jeśli trzeba wykonaj metodę po serializacji obiektu
            if (value is IXmlDeserializationCallback vm)
                vm.OnDeserialized(vm);

            // Deserializuj wgłąb
            if (value is ICollection collection)
            {
                // Jeśli kolekcja => deserializuj elementy kolekcji
                IEnumerator enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DeserializePropertyValue(enumerator.Current, visitatedObjects);
                }
            }
            else
            {
                // Jeśli właściwość => deserializuj
                var properties = value.GetType()
                    .GetProperties()
                    .Where(p => p.GetMethod != null && p.SetMethod != null);
                foreach (var property in properties)
                {
                    var childValue = property.GetValue(value);

                    DeserializePropertyValue(childValue, visitatedObjects);
                }
            }
        }

        public new object Deserialize(Stream stream)
        {
            var result = base.Deserialize(stream);
            var visitatedObjects = new List<object>();
            DeserializePropertyValue(result, visitatedObjects);
            if (result is IXmlDeserializationCallback vm)
                vm.IsDataReady = true;
            return result;
        }
    }
}
