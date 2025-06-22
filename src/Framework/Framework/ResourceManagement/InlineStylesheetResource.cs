using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// CSS in header. It's perfect for small css. For example critical CSS.
    /// </summary>
    public class InlineStylesheetResource : ResourceBase
    {
        private readonly ILocalResourceLocation? resourceLocation;
        private volatile Lazy<ImmutableArray<byte>>? code;

        /// <summary>
        /// Gets the CSS code that will be embedded in the page.
        /// </summary>
        [JsonIgnore]
        public string Code
        {
            get
            {
                var utf8 = code?.Value ?? throw new Exception("`ILocalResourceLocation` cannot be read using property `Code`.");
                return StringUtils.Utf8Decode(utf8.AsSpan());
            }
        }


        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName(nameof(Code))]
        internal string? CodeJsonHack => code is null ? null : Code; // ignore if code is in location


        [JsonConstructor]
        public InlineStylesheetResource(ILocalResourceLocation resourceLocation) : base(ResourceRenderPosition.Head)
        {
            this.resourceLocation = resourceLocation;
        }

        public InlineStylesheetResource(string code) : this(new InlineResourceLocation(code))
        {
            var utf8 = StringUtils.Utf8.GetBytes(code);
            InlineStyleContentGuard(utf8);
            this.code = new(ImmutableCollectionsMarshal.AsImmutableArray(utf8));
        }


        internal static void InlineStyleContentGuard(ReadOnlySpan<byte> code)
        {
            // We have to make sure, that the element is not ended in the middle.
            // <style> and <script> tags have "raw text" content - https://html.spec.whatwg.org/multipage/syntax.html#raw-text-elements
            // and those element must not contain "</name-of-the-element" substring - https://html.spec.whatwg.org/multipage/syntax.html#cdata-rcdata-restrictions
            if (InlineScriptResource.ContainsEndTag(code, "style"u8))
                throw new Exception($"Inline style can't contain `</style`.");
        }

        /// <inheritdoc/>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            if (this.code is null)
            {
                var newCode = new Lazy<ImmutableArray<byte>>(() => {
                    var c = resourceLocation!.ReadToBytes(context);
                    InlineStyleContentGuard(c);
                    return ImmutableCollectionsMarshal.AsImmutableArray(c);
                });
                // assign the `newValue` into `this.code` iff it's still null
                Interlocked.CompareExchange(ref this.code, value: newCode, comparand: null);
            }
            var code = this.code.Value;

            if (code.Length > 0)
            {
                writer.RenderBeginTag("style");
                writer.WriteUnencodedText(code.AsSpan());
                writer.RenderEndTag();
            }
        }
    }
}
