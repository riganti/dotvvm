using System.Reflection;

namespace DotVVM.Compiler.Resolving
{
    public interface IDotvvmServiceConfiguratorResolver
    {
        IServiceConfiguratorExecutor GetServiceConfiguratorExecutor(Assembly assembly);
    }
}
