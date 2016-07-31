using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractControl : IAbstractContentNode
    {
        IEnumerable<IPropertyDescriptor> PropertyNames { get; }

        bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value);

        IEnumerable<IAbstractHtmlAttributeSetter> HtmlAttributes { get; }

        object[] ConstructorParameters { get; set; }
        
    }
}