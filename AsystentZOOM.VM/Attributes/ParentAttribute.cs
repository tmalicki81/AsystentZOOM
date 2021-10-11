using System;
using System.Linq;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Attributes
{
    public class ParentAttribute : XmlIgnoreAttribute
    {
        public ParentAttribute(params Type[] interfaces)
            => Interfaces = interfaces;

        public Type[] Interfaces
        {
            get => _interfaces;
            set
            {
                if (value?.Any(x => !x.IsInterface) == true)
                    throw new ArgumentException($"Na liście interfejsów wystąpiła konkretna klasa.", nameof(value));
                _interfaces = value;
            }
        }

        private Type[] _interfaces;
    }
}
