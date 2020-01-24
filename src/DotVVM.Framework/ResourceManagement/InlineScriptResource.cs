using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Piece of javascript code that is used in the page.
    /// </summary>
    public class InlineScriptResource : ResourceBase, IDeferableResource
    {
        [Obsolete("Code parameter is required, please provide it in the constructor.")]
        public InlineScriptResource(ResourceRenderPosition renderPosition = ResourceRenderPosition.Body) : base(renderPosition)
        {
        }

        [JsonConstructor]
        public InlineScriptResource(string code, ResourceRenderPosition renderPosition = ResourceRenderPosition.Body, bool defer = false) : base(renderPosition)
        {
            this.Code = code;
            this.Defer = defer;
        }

        /// <summary>
        /// Gets or sets the javascript code that will be embedded in the page.
        /// </summary>
        public string Code { get; set; }

        /// <summary> If the script should be executed after the page loads (using the `defer` attribute). </summary>
        public bool Defer { get; }

        internal static bool IsUnsafeInlineScript(string code)
        {
            return code?.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            var code = Code;
            if (string.IsNullOrWhiteSpace(code)) return;

            var needBase64Hack =
                Defer || // browsers don't support `defer` attribute on inline script. We can overcome this limitation by using base64 data URI
                IsUnsafeInlineScript(code); // or, when the script is XSS-unsafe, we can do the same

            if (Defer)
                writer.AddAttribute("defer", null);

            if (needBase64Hack)
                RenderDataUriString(writer, code);
            else
                RenderClassicScript(writer, code);
        }

        internal static void RenderClassicScript(IHtmlWriter writer, string code)
        {
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(code);
            writer.RenderEndTag();
        }

        internal static void RenderDataUriString(IHtmlWriter writer, string code)
        {
            var uri = "data:text/javascript;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
            writer.AddAttribute("src", uri);
            writer.RenderBeginTag("script");
            writer.RenderEndTag();
        }
    }
}
