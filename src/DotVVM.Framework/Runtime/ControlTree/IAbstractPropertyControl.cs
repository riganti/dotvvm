namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractPropertyControl : IAbstractPropertySetter
    {
        IAbstractControl Control { get; }
    }
}