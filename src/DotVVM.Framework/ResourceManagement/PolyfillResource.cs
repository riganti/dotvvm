using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    internal class PolyfillResource : ResourceBase
    {
        public PolyfillResource() : base(ResourceRenderPosition.Body)
        {
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            var resource = (ScriptResource)context.ResourceManager.FindResource(ResourceConstants.PolyfillBundleResourceName);

            var resourceUrl = context.TranslateVirtualPath(
                resource.Location.GetUrl(context, ResourceConstants.PolyfillBundleResourceName));

            writer.AddAttribute("type", "text/javascript");
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText($"dotvvm__polyfillUrl = {JsonConvert.ToString(resourceUrl, '\'', StringEscapeHandling.EscapeHtml)};");
            writer.RenderEndTag();
        }
    }
}
