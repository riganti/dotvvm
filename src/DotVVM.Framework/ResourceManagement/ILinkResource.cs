#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a resource that is loaded from a location (possibly multiple as failover).
    /// </summary>
    public interface ILinkResource : IResource
    {
        IEnumerable<IResourceLocation> GetLocations();
        string MimeType { get; }
    }
}
