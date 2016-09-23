using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class PropertyGroupAttribute : Attribute
    {
        public string Prefix { get; }
        public Type ValueType { get; }
        public PropertyGroupAttribute(string prefix)
        {
            this.Prefix = prefix;
        }
    }
}
