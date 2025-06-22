using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using DotVVM.Core.Storage;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

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
        private volatile Lazy<ImmutableArray<byte>>? code;

        /// <summary>
        /// Gets or sets the javascript code that will be embedded in the page.
        /// </summary>
        [JsonIgnore]
        public string Code
        {
            get => StringUtils.Utf8Decode(code!.Value.AsSpan());
            [MemberNotNull(nameof(code))]
            set
            {
                var utf8 = value.ToUtf8Bytes();
                InlineScriptContentGuard(utf8);
                resourceLocation = null;
                this.code = new Lazy<ImmutableArray<byte>>(ImmutableCollectionsMarshal.AsImmutableArray(utf8));
            }
        }

        /// <summary>
        /// Gets or sets the javascript code that will be embedded in the page.
        /// </summary>
        [JsonIgnore]
        public ImmutableArray<byte> CodeUtf8
        {
            get => code!.Value;
            set
            {
                InlineScriptContentGuard(value.AsSpan());
                this.resourceLocation = null;
                this.code = new(value);
            }
        }




        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName(nameof(Code))]
        internal string? CodeJsonHack => resourceLocation is null ? this.Code : null; // ignore if code is in location


        /// <summary> If the script should be executed after the page loads (using the `defer` attribute). </summary>
        public bool Defer { get; }
        /// <summary> If the script should be rendered as type='module'. Module=true implies Defer=true </summary>
        public bool Module { get; }

        static bool InlineScriptContentGuard(ReadOnlySpan<byte> code)
        {
            // We have to make sure, that the element is not ended in the middle.
            // <style> and <script> tags have "raw text" content - https://html.spec.whatwg.org/multipage/syntax.html#raw-text-elements
            // and those element must not contain "</name-of-the-element" substring - https://html.spec.whatwg.org/multipage/syntax.html#cdata-rcdata-restrictions
            return ContainsEndTag(code, "script"u8);
        }

        internal static bool ContainsEndTag(ReadOnlySpan<byte> code, ReadOnlySpan<byte> endTagAscii)
        {
            // TODO: test this garbage
            while (code.Length >= endTagAscii.Length + 2)
            {
                var index = code.Slice(0, code.Length - endTagAscii.Length).IndexOf("</"u8);

                if (index < 0) return false;

                if (index + 2 + endTagAscii.Length > code.Length) return false;

                // case-insensitive ASCII compare
                if (CaseInsensitiveAsciiTextStartsWith(code.Slice(index + 2), endTagAscii))
                    return true;

                code = code.Slice(index + 2);
            }
            return false;
        }

        private static bool CaseInsensitiveAsciiTextStartsWith(ReadOnlySpan<byte> code, ReadOnlySpan<byte> start)
        {
            if (code.Length < start.Length) return false;

            for (var i = 0; i < start.Length; i++)
            {
                if ((code[i] | 32) != (start[i] | 32))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            RenderScript(writer, CodeUtf8.AsSpan(), Defer, Module);
        }

        /// <summary> Renders a &lt;script&gt; element with the <paramref name="code"/> content. </summary>
        public static void RenderScript(IHtmlWriter writer, ReadOnlySpan<byte> code, bool defer) =>
            RenderScript(writer, code, defer, module: false);
        /// <summary> Renders a &lt;script&gt; element with the <paramref name="code"/> content. </summary>
        public static void RenderScript(IHtmlWriter writer, ReadOnlySpan<byte> code, bool defer, bool module)
        {
            if (code.Length == 0) return;

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

        static void RenderClassicScript(IHtmlWriter writer, ReadOnlySpan<byte> code)
        {
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(code);
            writer.RenderEndTag();
        }

        internal static void RenderDataUriString(IHtmlWriter writer, ReadOnlySpan<byte> code)
        {
            var uri = "data:text/javascript;base64," + Convert.ToBase64String(code);
            writer.AddAttribute("src", uri);
            writer.RenderBeginTag("script");
            writer.RenderEndTag();
        }
    }
}
