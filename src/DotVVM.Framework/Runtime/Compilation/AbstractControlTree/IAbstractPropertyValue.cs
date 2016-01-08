namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractPropertyValue : IAbstractPropertySetter
    {
        object Value { get; }
    }
}