using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Framework.ResourceManagement
{
    public static class ResourcesRenderer
    {
        public static void RenderResource(this NamedResource resource, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            resource.Resource.Render(writer, context, resource.Name);
        }

        static void WriteResourceInfo(NamedResource resource, IHtmlWriter writer, bool preload)
        {
            var comment = $"Resource {resource.Name} of type {resource.Resource.GetType().Name}.";
            if (resource.Resource is ILinkResource linkResource)
                comment += $" Pointing to {string.Join(", ", linkResource.GetLocations().Select(l => l.GetType().Name))}.";

            if (preload) comment = "[preload link] " + comment;

            writer.WriteUnencodedText("\n    <!-- ");
            writer.WriteText(comment);
            writer.WriteUnencodedText(" -->\n    ");
            //                               ^~~~ most likely this info will be written directly in the <body> or <head>, so it should be indented by one level.
            //                                    we don't have any better way to know how we should indent
        }

        static bool HasAllDependencies(ResourceManager manager, IResource resource)
        {
            foreach (var d in resource.Dependencies)
                if (!manager.IsRendered(d))
                    return false;
            return true;
        }

        public static void RenderResources(ResourceManager resourceManager, IHtmlWriter writer, IDotvvmRequestContext context, ResourceRenderPosition position) =>
            RenderResources(resourceManager, resourceManager.GetNamedResourcesInOrder(), writer, context, position);
        public static void RenderResources(ResourceManager resourceManager, IEnumerable<NamedResource> resources, IHtmlWriter writer, IDotvvmRequestContext context, ResourceRenderPosition position)
        {
            var writeDebugInfo = context.Configuration.Debug;
            foreach (var resource in resources)
            {
                var resourcePosition = resource.Resource.RenderPosition;
                if (resourcePosition == position || resourcePosition == ResourceRenderPosition.Anywhere)
                {
                    if (resourceManager.IsRendered(resource.Name)) continue;

                    // check for all dependencies of Anywhere resource
                    if (resourcePosition == ResourceRenderPosition.Anywhere && position != ResourceRenderPosition.Body)
                    {
                        if (!HasAllDependencies(resourceManager, resource.Resource))
                            continue;
                    }

                    if (writeDebugInfo) WriteResourceInfo(resource, writer, preload: false);
                    // TODO: warning for missing dependencies
                    resourceManager.MarkRendered(resource);
                    resource.RenderResource(writer, context);
                }
                else if (position == ResourceRenderPosition.Head && resourcePosition == ResourceRenderPosition.Body && resource.Resource is IPreloadResource preloadResource)
                {
                    if (resourceManager.IsRendered(resource.Name)) continue;

                    if (writeDebugInfo) WriteResourceInfo(resource, writer, preload: true);
                    preloadResource.RenderPreloadLink(writer, context, resource.Name);
                }
            }

            if (writeDebugInfo)
                writer.WriteUnencodedText("\n");
        }

        [Obsolete("Use .RenderToString (cache is no longer available)")]
        public static string GetRenderedTextCached(this NamedResource resource, IDotvvmRequestContext context) =>
            RenderToString(resource, context);

        public static string RenderToString(this NamedResource resource, IDotvvmRequestContext context)
        {
            using (var text = new StringWriter())
            {
                resource.Resource.Render(new HtmlWriter(text, context), context, resource.Name);
                return text.ToString();
            }
        }

        public static Memory<byte> RenderToStringUtf8(this NamedResource resource, IDotvvmRequestContext context)
        {
            var ms = new MemoryStream();
            using (var text = new StreamWriter(ms, encoding: StringUtils.Utf8))
            {
                resource.Resource.Render(new HtmlWriter(text, context), context, resource.Name);
            }
            return ms.ToMemory();
        }
    }
}
