using System.Reflection;

namespace DotVVM.Compiler.Resolving
{
    public interface IDotvvmServiceConfiguratorResolver
    {
        IServicesConfiguratorExecutor GetServiceConfiguratorExecutor(Assembly assembly);
    }
}
