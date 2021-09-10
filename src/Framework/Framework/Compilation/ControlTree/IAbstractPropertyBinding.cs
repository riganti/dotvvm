using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyBinding : IAbstractPropertySetter
    {
        IAbstractBinding Binding { get; }
    }
}