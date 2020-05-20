#nullable enable
using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// A resource that is not rendered. Use this class if you load the scripts or styles yourself using the script or style element in the page.
    /// </summary>
    public class NullResource : ResourceBase
    {
        public NullResource() : base(ResourceRenderPosition.Body)
        { }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        { }
    }
}
