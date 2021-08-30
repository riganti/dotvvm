namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyControl : IAbstractPropertySetter
    {
        IAbstractControl? Control { get; }
    }
}
