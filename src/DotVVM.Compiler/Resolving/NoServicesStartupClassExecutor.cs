using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler.Resolving
{
    public class NoServicesStartupClassExecutor : IServicesStartupClassExecutor
    {
        public void ConfigureServices(IServiceCollection collection)
        {

        }
    }
}
