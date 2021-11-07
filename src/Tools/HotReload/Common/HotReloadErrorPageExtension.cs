using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.ErrorPages;

namespace DotVVM.HotReload
{
    public class HotReloadErrorPageExtension : IErrorPageExtension
    {

        public string GetHeadContents(IDotvvmRequestContext context, Exception ex)
        {
            using var textWriter = new StringWriter();
            var writer = new HtmlWriter(textWriter, context);
            var renderedResources = new HashSet<string>();

            RenderResource(context, "dotvvm-hotreload", writer, renderedResources);

            return textWriter.ToString();
        }

        private void RenderResource(IDotvvmRequestContext context, string resourceName, HtmlWriter? writer, HashSet<string> renderedResources)
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
