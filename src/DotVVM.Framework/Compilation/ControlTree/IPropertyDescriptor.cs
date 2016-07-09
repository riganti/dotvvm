using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IPropertyDescriptor
    {

        string Name { get; }

        ITypeDescriptor DeclaringType { get; }

        ITypeDescriptor PropertyType { get; }
        
        MarkupOptionsAttribute MarkupOptions { get; }

        bool IsBindingProperty { get; }

        string FullName { get; }
        DataContextChangeAttribute[] DataContextChangeAttributes { get; }
		DataContextStackManipulationAttribute DataContextManipulationAttribute { get; }

        bool IsVirtual { get; }
    }
}