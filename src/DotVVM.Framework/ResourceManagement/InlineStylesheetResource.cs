#nullable enable
using System;
using System.IO;
using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// CSS in header. It's perfect for small css. For example critical CSS.
    /// </summary>
    public class InlineStylesheetResource : ResourceBase
    {
        private readonly ILocalResourceLocation? resourceLocation;
        private volatile Lazy<string>? code;

        /// <summary>
        /// Gets the CSS code that will be embedded in the page.
        /// </summary>
        public string Code => code?.Value ?? throw new Exception("`ILocalResourceLocation` can not be read using property `Code`.");

        [JsonConstructor]
        public InlineStylesheetResource(ILocalResourceLocation resourceLocation) : base(ResourceRenderPosition.Head)
        {
            this.resourceLocation = resourceLocation;
        }

        public InlineStylesheetResource(string code) : this(new InlineResourceLocation(code))
        {
            InlineStyleContentGuard(code);
            this.code = new Lazy<string>(() => code);
            _ = this.code.Value;
        }

        public bool ShouldSerializeCode() => code?.IsValueCreated == true;

        internal static void InlineStyleContentGuard(string code)
        {
            // We have to make sure, that the element is not ended in the middle.
            // <style> and <script> tags have "raw text" content - https://html.spec.whatwg.org/multipage/syntax.html#raw-text-elements
            // and those element must not contain "</name-of-the-element" substring - https://html.spec.whatwg.org/multipage/syntax.html#cdata-rcdata-restrictions
            if (code?.IndexOf("</style", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new Exception($"Inline style can't contain `</style`.");
        }

        /// <inheritdoc/>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            if (this.code == null)
            {
                var newCode = new Lazy<string>(() => {
                    var c = resourceLocation!.ReadToString(context);
                    InlineStyleContentGuard(c);
                    return c;
                });
                // assign the `newValue` into `this.code` iff it's still null
                Interlocked.CompareExchange(ref this.code, value: newCode, comparand: null);
            }
            var code = this.code.Value;

            if (!string.IsNullOrWhiteSpace(code))
            {
                writer.RenderBeginTag("style");
                writer.WriteUnencodedText(code);
                writer.RenderEndTag();
            }
        }
    }
}
