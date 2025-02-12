using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

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
        public InlineScriptResource(string code, ResourceRenderPosition renderPosition = ResourceRenderPosition.Body, bool defer = false, bool module = false) : base(renderPosition)
        {
            this.Code = code;
            this.Module = module;
            this.Defer = defer || module;
        }

        public InlineScriptResource(string code, ResourceRenderPosition renderPosition, bool defer) : base(renderPosition)
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
        [JsonIgnore]
        public string Code
        {
            get => code?.Value ?? throw new Exception("`ILocalResourceLocation` cannot be read using property `Code`.");
            set
            {
                InlineScriptContentGuard(value);
                this.resourceLocation = new InlineResourceLocation(value);
                this.code = new Lazy<string>(() => value);
                _ = this.code.Value;
            }
        }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName(nameof(Code))]
        internal string? CodeJsonHack => code?.Value; // ignore if code is in location


        /// <summary> If the script should be executed after the page loads (using the `defer` attribute). </summary>
        public bool Defer { get; }
        /// <summary> If the script should be rendered as type='module'. Module=true implies Defer=true </summary>
        public bool Module { get; }
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
            RenderScript(writer, Code, Defer, Module);
        }

        /// <summary> Renders a &lt;script&gt; element with the <paramref name="code"/> content. </summary>
        public static void RenderScript(IHtmlWriter writer, string code, bool defer) =>
            RenderScript(writer, code, defer, module: false);
        /// <summary> Renders a &lt;script&gt; element with the <paramref name="code"/> content. </summary>
        public static void RenderScript(IHtmlWriter writer, string code, bool defer, bool module)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            var needBase64Hack =
                (defer && !module) || // browsers don't support `defer` attribute on inline script. We can overcome this limitation by using base64 data URI
                InlineScriptContentGuard(code); // or, when the script is XSS-unsafe, we can do the same

            if (defer && !module)
                writer.AddAttribute("defer", null);

            if (module)
                writer.AddAttribute("type", "module");

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
