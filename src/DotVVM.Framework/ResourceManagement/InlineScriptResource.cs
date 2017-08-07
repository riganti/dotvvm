using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Piece of javascript code that is used in the page.
    /// </summary>
    public class InlineScriptResource : ResourceBase
    {
        public InlineScriptResource(ResourceRenderPosition renderPosition = ResourceRenderPosition.Body) : base(renderPosition)
        {
        }

        /// <summary>
        /// Gets or sets the javascript code that will be embedded in the page.
        /// </summary>
        private string _code;
        public string Code
        {
            get => _code;
            set
            {
                if (value?.Contains("</script>") == true) throw new Exception($"Inline script can't contain `</script>`.");
                _code = value;
            }
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            if (string.IsNullOrWhiteSpace(Code)) return;
            writer.AddAttribute("type", "text/javascript");
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(Code);
            writer.RenderEndTag();
        }
    }
}