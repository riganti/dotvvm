#nullable enable
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a resource that will be preloaded using a link element with attribute rel="preload" rendered into header.  
    /// For more information about this technique see <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Preloading_content">MDN web docs</see>.
    /// If the resource is added after the HEAD element is rendered, the resource is not going to be preloaded.
    /// </summary>
    public interface IPreloadResource : ILinkResource
    {
        void RenderPreloadLink(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName);
    }
}
