using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    public class TemplateResource : IResource
    {
        public ResourceRenderPosition RenderPosition => ResourceRenderPosition.Body;
        public string[] Dependencies { get; } = new string[0];

        public string Template
        {
            get => StringUtils.Utf8Decode(TemplateUtf8.Span);
            set => TemplateUtf8 = StringUtils.Utf8.GetBytes(value);
        }

        [JsonIgnore]
        public ReadOnlyMemory<byte> TemplateUtf8 { get; set; }

        public TemplateResource(ReadOnlyMemory<byte> templateUtf8)
        {
            TemplateUtf8 = templateUtf8;
        }

        public void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            writer.AddAttribute("id", resourceName);

            writer.RenderBeginTag("template");
            writer.WriteUnencodedText(TemplateUtf8.Span);
            writer.RenderEndTag();
        }
    }
}
