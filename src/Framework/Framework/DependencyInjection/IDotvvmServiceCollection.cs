using System.Collections.Generic;
using DotVVM.Framework.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring DotVVM services.
    /// </summary>
    public interface IDotvvmServiceCollection : IList<ServiceDescriptor>
    {
        /// <summary>
        /// Gets the underlying <see cref="IServiceCollection"/> that is being configured.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Indicates whether DotvvmStartup was invoked by the DotVVM compiler.
        /// DotVVM compiler doesn't run Program.cs or Startup.cs, so some services may not be available.
        /// This property can be used to conditionally register services that are only needed for runtime.
        /// </summary>
        bool IsDotvvmCompiler { get; }
    }
}
