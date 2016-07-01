using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// A resource that is not rendered. Use this class if you load the scripts or styles yourself using hte script or style element in the page.
    /// </summary>
    [ResourceConfigurationCollectionName("null")]
    public class NullResource : ResourceBase
    {
        public override ResourceRenderPosition GetRenderPosition()
        {
            return ResourceRenderPosition.Body;
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }
    }
}