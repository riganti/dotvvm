using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    /// <summary> Attribute for annotating DotVVM property group. It is important for VS Extension to understand the property groups, it is irrelevant for DotVVM runtime.  </summary>
    public class PropertyGroupAttribute : Attribute
    {
        public string[] Prefixes { get; }
        public Type? ValueType { get; set; }
        public PropertyGroupAttribute(string prefix)
        {
            this.Prefixes = new[] { prefix };
        }

        public PropertyGroupAttribute(params string[] prefixes)
        {
            if (prefixes == null || prefixes.Length == 0) throw new ArgumentException(nameof(prefixes));
            this.Prefixes = prefixes;
        }
    }
}
