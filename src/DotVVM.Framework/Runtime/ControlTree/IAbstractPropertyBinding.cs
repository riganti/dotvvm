namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractPropertyBinding : IAbstractPropertySetter
    {
        IAbstractBinding Binding { get; }
    }
}