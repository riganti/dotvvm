using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    /// <summary>
    /// Represents configuration point for all DotVVM services. This interface is intended for marking services to be run by compiler. 
    /// </summary>
    public interface IDotvvmServiceConfigurator
    {
        /// <summary>
        /// Configures all services related with DotVVM to ServiceCollection.
        /// Warning: Configure only DotVVM services. This method is used by DotVVM.Compiler which runs this method during compilation time.
        /// </summary>
        /// <remarks>The name "options" was chosen because of easier migration.</remarks>
        void ConfigureServices(IDotvvmServiceCollection options);
    }
}
