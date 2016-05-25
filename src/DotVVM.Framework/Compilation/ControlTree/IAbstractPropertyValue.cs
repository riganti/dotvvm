namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyValue : IAbstractPropertySetter
    {
        object Value { get; }
    }
}