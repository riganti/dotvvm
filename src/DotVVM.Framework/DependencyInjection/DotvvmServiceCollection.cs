using System;
using DotVVM.Framework.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows fine grained configuration of DotVVM services.
    /// </summary>
    public class DotvvmServiceCollection : IDotvvmServiceCollection
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmServiceCollection" /> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public DotvvmServiceCollection(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
