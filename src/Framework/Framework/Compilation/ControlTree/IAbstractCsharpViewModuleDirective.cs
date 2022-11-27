namespace DotVVM.Framework.Compilation.ControlTree;

public interface IAbstractCsharpViewModuleDirective : IAbstractDirective
{
    /// <summary>Full type name of the module specified</summary>
    ITypeDescriptor ModuleType { get; }
}
