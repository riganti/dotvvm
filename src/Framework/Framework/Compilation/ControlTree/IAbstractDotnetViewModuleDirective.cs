namespace DotVVM.Framework.Compilation.ControlTree;

public interface IAbstractDotnetViewModuleDirective : IAbstractDirective
{
    /// <summary>Full type name of the module specified</summary>
    ITypeDescriptor ModuleType { get; }
}
