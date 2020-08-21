#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public class TemplateResource : IResource
    {
        public ResourceRenderPosition RenderPosition => ResourceRenderPosition.Body;
        public string[] Dependencies { get; } = new string[0];

        public string? Template { get; set; }

        public TemplateResource(string template)
        {
            Template = template;
        }

        public void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            var template = Template;
            var needBase64Hack = InlineScriptResource.IsUnsafeInlineScript(template);

            writer.AddAttribute("type", "text/html");
            writer.AddAttribute("id", resourceName);

            if (needBase64Hack)
                InlineScriptResource.RenderDataUriString(writer, template);
            else
                InlineScriptResource.RenderClassicScript(writer, template);
        }
    }
}
