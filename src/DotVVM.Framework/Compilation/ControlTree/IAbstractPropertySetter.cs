using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertySetter : IAbstractTreeNode
    {
        IPropertyDescriptor Property { get; }   
    }
}