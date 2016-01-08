namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractPropertyControl : IAbstractPropertySetter
    {
        IAbstractControl Control { get; }
    }
}