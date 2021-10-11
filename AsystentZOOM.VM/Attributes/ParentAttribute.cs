using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AsystentZOOM.VM.Attributes
{
    public class ParentAttribute : XmlIgnoreAttribute
    {
        public ParentAttribute(params Type[] type)
            => Type = type;

        public Type[] Type { get; set; }

    }
}
