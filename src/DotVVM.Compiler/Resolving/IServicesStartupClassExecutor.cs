using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Resolving
{
    public interface IServicesStartupClassExecutor
    {
        void ConfigureServices(IServiceCollection collection);

    }
}
