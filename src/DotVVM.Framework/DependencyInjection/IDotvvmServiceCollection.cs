#nullable enable
using System.Collections.Generic;
using DotVVM.Framework.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring DotVVM services.
    /// </summary>
    public interface IDotvvmServiceCollection : IList<ServiceDescriptor>
    {
        IServiceCollection Services { get; }
    }
}
