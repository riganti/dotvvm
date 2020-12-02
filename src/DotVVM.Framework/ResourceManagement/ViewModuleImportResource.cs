using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a list of modules that will be registered using dotvvm.registerViewModules for runtime use.
    /// </summary>
    public class ViewModuleImportResource : IResource
    {
        public ResourceRenderPosition RenderPosition => ResourceRenderPosition.Anywhere;
        public string[] ReferencedModules { get; }

        public string[] Dependencies => Array.Empty<string>();

        private string registrationScript;

        public ViewModuleImportResource(string[] referencedModules)
        {
            this.ReferencedModules = referencedModules;

            this.registrationScript =
                $"dotvvm.registerViewModules({{{string.Join(", ", this.ReferencedModules.Select((m, i) => JsonConvert.ToString(m, '\'', StringEscapeHandling.EscapeHtml) + ": m" + i))}}});";
        }

        public void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            writer.AddAttribute("type", "module");
            writer.RenderBeginTag("script");
            int i = 0;
            foreach (var r in this.ReferencedModules)
            {
                var resource = context.ResourceManager.FindResource(r);
                if (resource is null)
                    throw new Exception($"Resource {r} does not exist.");
                if (!(resource is ILinkResource linkResource))
                    throw new Exception($"Resource {r} is not a LinkResource.");

                var location = linkResource.GetLocations().FirstOrDefault()?.GetUrl(context, r);
                if (location is null)
                    throw new Exception($"Could not get location of resource {r}");

                location = context.TranslateVirtualPath(location);

                writer.WriteUnencodedText("import * as m");
                writer.WriteUnencodedText(i.ToString());
                writer.WriteUnencodedText(" from ");
                writer.WriteUnencodedText(JsonConvert.ToString(location, '\'', StringEscapeHandling.EscapeHtml));
                writer.WriteUnencodedText(";");

                i += 1;
            }

            writer.WriteUnencodedText(registrationScript);
            writer.RenderEndTag();
        }
    }
}
