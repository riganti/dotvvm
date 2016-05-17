namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyBinding : IAbstractPropertySetter
    {
        IAbstractBinding Binding { get; }
    }
}