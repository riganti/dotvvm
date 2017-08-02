using System;
using DotVVM.Framework.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows fine grained configuration of DotVVM services.
    /// </summary>
    public class DotvvmOptions : IDotvvmOptions
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmOptions" /> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public DotvvmOptions(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}