namespace DotVVM.Framework.Interop.DotnetWasm;

public class ViewModuleCommand : IViewModuleCommand
{
    private readonly string typeName;
    private readonly string instanceName;
    private readonly string commandName;
    private readonly DotvvmClientSerializer serializer;

    public ViewModuleCommand(string typeName, string instanceName, string commandName, DotvvmClientSerializer serializer)
    {
        this.typeName = typeName;
        this.instanceName = instanceName;
        this.commandName = commandName;
        this.serializer = serializer;
    }

    public Task InvokeAsync(params object[] args) => InvokeAsync<object?>(args);

    public async Task<T?> InvokeAsync<T>(params object[] args)
    {
        var argValues = args.Select(serializer.Serialize).ToArray();
        var json = await DotnetWasmInterop.CallNamedCommand(typeName, instanceName, commandName, argValues);
        return (T?)serializer.Deserialize(typeof(T), json);
    }
        
}
