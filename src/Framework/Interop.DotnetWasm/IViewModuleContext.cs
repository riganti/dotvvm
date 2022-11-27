namespace DotVVM.Framework.Interop.DotnetWasm;

public interface IViewModuleContext
{
    T? GetViewModelSnapshot<T>();

    void PatchViewModel(object data);

    IReadOnlyDictionary<string, IViewModuleCommand> NamedCommands { get; }

}
