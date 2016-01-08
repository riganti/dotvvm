namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractPropertyBinding : IAbstractPropertySetter
    {
        IAbstractBinding Binding { get; }
    }
}