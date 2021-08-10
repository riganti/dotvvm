using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public struct PropertyGroupMatcher
    {
        public readonly string Prefix;
        public readonly IPropertyGroupDescriptor PropertyGroup;

        public PropertyGroupMatcher(string prefix, IPropertyGroupDescriptor propertyGroup)
        {
            this.Prefix = prefix;
            this.PropertyGroup = propertyGroup;
        }
    }
}
