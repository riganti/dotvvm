using System;
using System.IO;
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
        private string code;
        private readonly ILocalResourceLocation resourceLocation;
        
        [JsonConstructor]
        public InlineStylesheetResource(ILocalResourceLocation resourceLocation) : base(ResourceRenderPosition.Head)
        {
            this.resourceLocation = resourceLocation;
        }

        /// <inheritdoc/>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            if (code == null)
            {
                using (var resourceStream = resourceLocation.LoadResource(context))
                {
                    using (var resourceStreamReader = new StreamReader(resourceStream))
                    {
                        code = resourceStreamReader.ReadToEnd();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                writer.RenderBeginTag("style");
                writer.WriteUnencodedText(code);
                writer.RenderEndTag();
            }
        }
    }
}
