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
        private readonly string _template;

        public ResourceRenderPosition RenderPosition => ResourceRenderPosition.Body;
        public string[] Dependencies { get; } = new string[0];

        public TemplateResource(string template)
        {
            _template = template;
        }

        public void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            writer.AddAttribute("type", "text/html");
            writer.AddAttribute("id", resourceName);
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(_template);
            writer.RenderEndTag();
        }
    }
}
