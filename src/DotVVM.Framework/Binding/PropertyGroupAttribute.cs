#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class PropertyGroupAttribute : Attribute
    {
        public string[] Prefixes { get; }
        public Type? ValueType { get; set; }
        public PropertyGroupAttribute(string prefix)
        {
            this.Prefixes = new[] { prefix };
        }

        public PropertyGroupAttribute(string[] prefixes)
        {
            this.Prefixes = prefixes;
        }
    }
}
