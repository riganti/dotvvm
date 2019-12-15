using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Piece of javascript code that is used in the page.
    /// </summary>
    public class InlineScriptResource : ResourceBase
    {
        [Obsolete("Code parameter is required, please provide it in the constructor.")]
        public InlineScriptResource(ResourceRenderPosition renderPosition = ResourceRenderPosition.Body) : base(renderPosition)
        {
        }

        [JsonConstructor]
        public InlineScriptResource(string code, ResourceRenderPosition renderPosition = ResourceRenderPosition.Body) : base(renderPosition)
        {
            this.Code = code;
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
                InlineScriptContentGuard(value);
                _code = value;
            }
        }

        internal static void InlineScriptContentGuard(string code)
        {
            if (code?.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new Exception($"Inline script can't contain `</script>`.");
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
