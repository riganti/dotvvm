using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Resolving
{
    public interface IServicesConfiguratorExecutor
    {
        void ConfigureServices(IServiceCollection collection);

    }
}
