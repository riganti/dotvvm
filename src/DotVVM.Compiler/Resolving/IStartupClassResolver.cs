using System.Reflection;

namespace DotVVM.Compiler.Resolving
{
    public interface IStartupClassResolver
    {
        IServicesStartupClassExecutor GetServiceConfigureExecutor(Assembly assembly);
    }
}