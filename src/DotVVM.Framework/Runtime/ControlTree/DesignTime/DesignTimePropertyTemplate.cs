using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
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