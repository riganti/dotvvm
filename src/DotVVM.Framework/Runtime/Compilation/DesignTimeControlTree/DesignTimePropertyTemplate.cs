using System.Collections.Generic;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
{
    public class DesignTimePropertyTemplate : DesignTimePropertySetter, IAbstractPropertyTemplate
    {
        public DesignTimePropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> content) : base(property)
        {
            Content = content;
        }

        public IEnumerable<IAbstractControl> Content { get; }
    }
}