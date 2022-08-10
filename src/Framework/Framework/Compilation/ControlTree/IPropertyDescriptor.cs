using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IPropertyDescriptor: IControlAttributeDescriptor
    {
        bool IsBindingProperty { get; }
        string FullName { get; }
    }

    public interface IGroupedPropertyDescriptor: IPropertyDescriptor
    {
        string GroupMemberName { get; }
        IPropertyGroupDescriptor PropertyGroup { get; }
    }
}
