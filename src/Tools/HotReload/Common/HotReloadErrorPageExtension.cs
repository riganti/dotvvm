using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.ErrorPages;
using DotVVM.Framework.Utils;

namespace DotVVM.HotReload
{
    public class HotReloadErrorPageExtension : IErrorPageExtension
    {

        public string GetHeadContents(IDotvvmRequestContext context, Exception ex)
        {
            if (context.Configuration.Debug)
            {
                var ms = new MemoryStream();

                var renderedResources = new HashSet<string>();
                using (var writer = new HtmlWriter(ms, context))
                    RenderResource(context, "dotvvm-hotreload", writer, renderedResources);

                return StringUtils.Utf8Decode(ms.ToArray());
            }
            else
            {
                return string.Empty;
            }
        }

        private void RenderResource(IDotvvmRequestContext context, string resourceName, HtmlWriter writer, HashSet<string> renderedResources)
        {
            if (renderedResources.Contains(resourceName) || resourceName == "dotvvm") return;
            renderedResources.Add(resourceName);

            var resource = context.Configuration.Resources.FindResource(resourceName)!;
            foreach (var dependency in resource.Dependencies)
            {
                RenderResource(context, dependency, writer, renderedResources);
            }

            resource!.Render(writer, context, resourceName);
        }
    }
}
