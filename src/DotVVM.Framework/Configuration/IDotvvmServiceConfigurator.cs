using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    public interface IDotvvmServiceConfigurator
    {
        void ConfigureServices(IDotvvmServiceCollection services);
    }
}
