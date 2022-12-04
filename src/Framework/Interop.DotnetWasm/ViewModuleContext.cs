namespace DotVVM.Framework.Interop.DotnetWasm
{
    public class ViewModuleContext : IViewModuleContext
    {
        private readonly string typeName;
        private readonly string instanceName;
        private readonly DotvvmClientSerializer serializer;

        public T GetViewModelSnapshot<T>()
        {
            var json = DotnetWasmInterop.GetViewModelSnapshot();
            return (T)serializer.Deserialize(typeof(T), json)!;
        }

        public void PatchViewModel(object data)
        {
            var json = serializer.Serialize(data);
            DotnetWasmInterop.PatchViewModelSnapshot(json);
        }

        public Task InvokeNamedCommandAsync(string commandName, params object[] args) => InvokeNamedCommandAsync<object>(commandName, args);

        public async Task<T?> InvokeNamedCommandAsync<T>(string commandName, params object[] args)
        {
            var argValues = args.Select(serializer.Serialize).ToArray();
            var json = await DotnetWasmInterop.CallNamedCommand(typeName, instanceName, commandName, argValues);
            return (T?)serializer.Deserialize(typeof(T), json);
        }

        public ViewModuleContext(string typeName, string instanceName, DotvvmClientSerializer serializer)
        {
            this.typeName = typeName;
            this.instanceName = instanceName;
            this.serializer = serializer;
        }
    }
}
