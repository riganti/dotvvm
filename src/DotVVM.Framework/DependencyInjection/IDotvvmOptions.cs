using DotVVM.Framework.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring DotVVM services.
    /// </summary>
    public interface IDotvvmOptions
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection" /> where DotVVM services are configured.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Gets the <see cref="DotvvmConfiguration" />.
        /// </summary>
        DotvvmConfiguration Configuration { get; }
    }
}