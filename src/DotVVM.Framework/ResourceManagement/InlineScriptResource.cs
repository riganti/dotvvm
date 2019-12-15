#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            }
        }

        internal static void InlineScriptContentGuard(string? code)
        {
            // We have to make sure, that the element is not ended in the middle.
            // <style> and <script> tags have "raw text" content - https://html.spec.whatwg.org/multipage/syntax.html#raw-text-elements
            // and those element must not contain "</name-of-the-element" substring - https://html.spec.whatwg.org/multipage/syntax.html#cdata-rcdata-restrictions
            if (code?.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new Exception($"Inline script can't contain `</script`.");
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            if (this.code == null)
            {
                var newCode = new Lazy<string>(() => {
                    var c = resourceLocation!.ReadToString(context);
                    InlineScriptContentGuard(c);
                    return c;
                });
                // assign the `newValue` into `this.code` iff it's still null
                Interlocked.CompareExchange(ref this.code, value: newCode, comparand: null);
            }
            var code = this.code.Value;

            if (string.IsNullOrWhiteSpace(code)) return;
            writer.AddAttribute("type", "text/javascript");
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(code);
            writer.RenderEndTag();
        }
    }
}
