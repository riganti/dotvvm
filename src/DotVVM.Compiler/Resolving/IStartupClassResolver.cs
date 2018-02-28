using System.Reflection;

namespace DotVVM.Compiler.Resolving
{
    public interface IStartupClassResolver
    {
        IServicesConfiguratorExecutor GetServiceConfiguratorExecutor(Assembly assembly);
    }
}
