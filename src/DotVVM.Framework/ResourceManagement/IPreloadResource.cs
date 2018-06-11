using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a resource thas is preloaded. 
    /// </summary>
    public interface IPreloadResource : ILinkResource
    {
        string ContentType { get; }
        string GetUrlLocation(IDotvvmRequestContext context, string resourceName);
    }
}
