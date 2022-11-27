namespace DotVVM.Framework.Interop.DotnetWasm;

public interface IViewModuleCommand
{
    Task InvokeAsync(params object[] args);
    Task<T?> InvokeAsync<T>(params object[] args);
}