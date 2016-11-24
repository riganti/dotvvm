using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public static class ResourcesRenderer
    {
        private static ConditionalWeakTable<IResource, string> renderedCache = new ConditionalWeakTable<IResource, string>();

        public static void RenderResourceCached(this NamedResource resource, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.WriteUnencodedText(resource.GetRenderedTextCached(context));
        }

        public static string GetRenderedTextCached(this NamedResource resource, IDotvvmRequestContext context)
        {
            return renderedCache.GetValue(resource.Resource,
                _ =>
                {
                    using (var text = new StringWriter())
                    {
                        resource.Resource.Render(new HtmlWriter(text, context), context, resource.Name);
                        return text.ToString();
                    }
                });
        }
    }
}
