namespace DotVVM.Framework.Interop.DotnetWasm;

public interface IViewModuleContext
{
    T GetViewModelSnapshot<T>();

    void PatchViewModel(object data);

    Task InvokeNamedCommandAsync(string commandName, params object[] args);

    Task<T?> InvokeNamedCommandAsync<T>(string commandName, params object[] args);

}
