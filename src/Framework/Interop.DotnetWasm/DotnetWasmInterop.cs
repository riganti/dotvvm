using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

namespace DotVVM.Framework.Interop.DotnetWasm;

internal static partial class DotnetWasmInterop
{
    private static Dictionary<ViewModuleInstanceKey, object> instances = new();
    private static DotvvmClientSerializer serializer = new();

    [JSExport]
    internal static void CreateViewModuleInstance(string typeName, string instanceName, string[] namedCommandNames)
    {
        var type = Type.GetType(typeName, true);
        var context = new ViewModuleContext(typeName, instanceName, namedCommandNames, serializer);

        var instance = Activator.CreateInstance(type, context);
        instances.Add(new ViewModuleInstanceKey(typeName, instanceName), instance);
    }

    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    internal static object? CallViewModuleCommand(string typeName, string instanceName, string methodName, string[] args)
    {
        var instance = GetInstance(typeName, instanceName);
        var method = instance.GetType().GetMethod(methodName);
        if (method == null)
        {
            throw new Exception($"The method {methodName} was not found!");
        }

        var parameters = method.GetParameters();
        var argValues = args
            .Select((json, index) => serializer.Deserialize(parameters[index].ParameterType, json))
            .ToArray();

        try
        {
            return method.Invoke(instance, argValues);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }
    }

    [JSImport("callNamedCommand", "dotvvmResource/dotvvm--interop--dotnet-wasm/dotvvm--interop--dotnet-wasm")]
    internal static partial Task<string> CallNamedCommand(string typeName, string instanceName, string commandName, string[] args);

    [JSImport("getViewModelSnapshot", "dotvvmResource/dotvvm--interop--dotnet-wasm/dotvvm--interop--dotnet-wasm")]
    internal static partial string GetViewModelSnapshot();

    [JSImport("patchViewModel", "dotvvmResource/dotvvm--interop--dotnet-wasm/dotvvm--interop--dotnet-wasm")]
    internal static partial void PatchViewModelSnapshot(string patchJson);

    [JSExport]
    internal static void DisposeViewModuleInstance(string typeName, string instanceName)
    {
        instances.Remove(new ViewModuleInstanceKey(typeName, instanceName));
    }

    private static object GetInstance(string typeName, string instanceName)
    {
        return instances[new ViewModuleInstanceKey(typeName, instanceName)];
    }

    record ViewModuleInstanceKey(string TypeName, string InstanceName);

}
