namespace DotVVM.Framework.Interop.DotnetWasm
{
    public class ViewModuleContext : IViewModuleContext
    {
        private readonly DotvvmClientSerializer serializer;

        public IReadOnlyDictionary<string, IViewModuleCommand> NamedCommands { get; }

        public T? GetViewModelSnapshot<T>()
        {
            var json = DotnetWasmInterop.GetViewModelSnapshot();
            return (T?)serializer.Deserialize(typeof(T), json);
        }

        public void PatchViewModel(object data)
        {
            var json = serializer.Serialize(data);
            DotnetWasmInterop.PatchViewModelSnapshot(json);
        }

        public ViewModuleContext(string typeName, string instanceName, IEnumerable<string> namedCommandNames, DotvvmClientSerializer serializer)
        {
            this.serializer = serializer;

            NamedCommands = namedCommandNames
                .ToDictionary(c => c, c => (IViewModuleCommand)new ViewModuleCommand(typeName, instanceName, c, serializer));
        }
    }
}
