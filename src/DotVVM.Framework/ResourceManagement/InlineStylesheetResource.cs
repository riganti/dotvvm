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
        private readonly ILocalResourceLocation resourceLocation;
        private volatile Lazy<string> code;

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
            this.code = new Lazy<string>(() => code);
        }

        /// <inheritdoc/>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            if (this.code == null)
            {
                Interlocked.CompareExchange(ref this.code, new Lazy<string>(() => resourceLocation.ReadToString(context)), null);
            }
            var code = this.code.Value;

            if (!string.IsNullOrWhiteSpace(code))
            {
                writer.RenderBeginTag("style");
                writer.WriteText(code);
                writer.RenderEndTag();
            }
        }
    }
}
