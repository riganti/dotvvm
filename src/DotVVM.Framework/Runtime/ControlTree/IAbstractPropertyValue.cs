namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractPropertyValue : IAbstractPropertySetter
    {
        object Value { get; }
    }
}