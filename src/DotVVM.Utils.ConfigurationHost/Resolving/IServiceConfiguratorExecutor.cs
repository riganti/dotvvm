using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Resolving
{
    public interface IServiceConfiguratorExecutor
    {
        void ConfigureServices(IServiceCollection collection);

    }
}
