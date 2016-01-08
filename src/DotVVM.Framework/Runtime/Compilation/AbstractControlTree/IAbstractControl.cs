using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractControl : IAbstractContentNode
    {

        IEnumerable<IPropertyDescriptor> PropertyNames { get; }

        bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value);

        IReadOnlyDictionary<string, object> HtmlAttributes { get; }

        IEnumerable<object> ContructorParameters { get; }

    }
}