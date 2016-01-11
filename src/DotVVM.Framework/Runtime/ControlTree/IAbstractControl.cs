using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractControl : IAbstractContentNode
    {

        IEnumerable<IPropertyDescriptor> PropertyNames { get; }

        bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value);

        IReadOnlyDictionary<string, object> HtmlAttributes { get; }

        object[] ConstructorParameters { get; set; }
        
    }
}