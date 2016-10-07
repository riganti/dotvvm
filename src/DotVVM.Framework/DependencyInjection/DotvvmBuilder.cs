using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows fine grained configuration of DotVVM services.
    /// </summary>
    public class DotvvmBuilder : IDotvvmBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmBuilder" /> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public DotvvmBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            Services = services;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }
    }
}