using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Piece of javascript code that is used in the page.
    /// </summary>
    public class InlineScriptResource : ResourceBase, IDeferrableResource
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
        public InlineScriptResource(ILocalResourceLocation resourceLocation, ResourceRenderPosition renderPosition = ResourceRenderPosition.Body) : base(renderPosition)
        {
            this.resourceLocation = resourceLocation;
        }

        private ILocalResourceLocation? resourceLocation;
        private volatile Lazy<string>? code;

        /// <summary>
        /// Gets or sets the javascript code that will be embedded in the page.
        /// </summary>
        public string Code
        {
            get => code?.Value ?? throw new Exception("`ILocalResourceLocation` can not be read using property `Code`.");
            set
            {
                InlineScriptContentGuard(value);
                this.resourceLocation = new InlineResourceLocation(value);
                this.code = new Lazy<string>(() => value);
                _ = this.code.Value;
            }
        }

        /// <summary> If the script should be executed after the page loads (using the `defer` attribute). </summary>
        public bool Defer { get; }
        public bool ShouldSerializeCode() => code?.IsValueCreated == true;

        static bool InlineScriptContentGuard(string? code)
        {
            // We have to make sure, that the element is not ended in the middle.
            // <style> and <script> tags have "raw text" content - https://html.spec.whatwg.org/multipage/syntax.html#raw-text-elements
            // and those element must not contain "</name-of-the-element" substring - https://html.spec.whatwg.org/multipage/syntax.html#cdata-rcdata-restrictions
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
                InlineScriptContentGuard(code); // or, when the script is XSS-unsafe, we can do the same

            if (Defer)
                writer.AddAttribute("defer", null);

            if (needBase64Hack)
                RenderDataUriString(writer, code);
            else
                RenderClassicScript(writer, code);
        }

        static void RenderClassicScript(IHtmlWriter writer, string code)
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
