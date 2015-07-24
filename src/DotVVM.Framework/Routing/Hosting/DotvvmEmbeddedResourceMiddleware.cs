using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Configuration;
using System.Collections.Concurrent;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// Provides access to embedded resources in the DotVVM.Framework assembly.
    /// </summary>
    public class DotvvmEmbeddedResourceMiddleware : OwinMiddleware
    {
        public DotvvmEmbeddedResourceMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            // try resolve the route
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);

            // disable access to the dotvvm.json file
            if (url.StartsWith("dotvvm.json", StringComparison.CurrentCultureIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                throw new UnauthorizedAccessException("The dotvvm.json cannot be served!");
            }

            // embedded resource handler URL
            if (url.StartsWith(Constants.ResourceHandlerMatchUrl))
            {
                var resourceName = url.Substring(url.LastIndexOf('/') + 1);
                return RenderEmbeddedResource(context, resourceName);
            }
            else
            {
                return Next.Invoke(context);
            }
        }



        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private async Task RenderEmbeddedResource(IOwinContext context, string name)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var res = NameToResource[name];
            if(res.MimeType != null)
            {
                context.Response.ContentType = res.MimeType;
            }
            if (res.Name.EndsWith(".js"))
            {
                context.Response.ContentType = "text/javascript";
            }
            else if (res.Name.EndsWith(".css"))
            {
                context.Response.ContentType = "text/css";
            }
            else
            {
                context.Response.ContentType = "application/octet-stream";
            }

            using (var resourceStream = res.Assembly.GetManifestResourceStream(res.Name))
            {
                await resourceStream.CopyToAsync(context.Response.Body);
            }
        }


        public static string RegisterHandledResource(string assemblyName, string embededResourceName, string friendlyName = null)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            return RegisterHandledResource(assembly, embededResourceName, friendlyName);
        }

        private static ConcurrentDictionary<EmbededResourceInfo, string> ResourceToName = new ConcurrentDictionary<EmbededResourceInfo, string>();
        private static ConcurrentDictionary<string, EmbededResourceInfo> NameToResource = new ConcurrentDictionary<string, EmbededResourceInfo>();

        public static string RegisterHandledResource(Assembly assembly, string embededResourceName, string friendlyName = null)
        {
            return ResourceToName.GetOrAdd(
                new EmbededResourceInfo { Assembly = assembly, Name = embededResourceName },
                res => AddToNtr(res, friendlyName ?? embededResourceName.Replace(assembly.GetName().Name + ".", "").Replace('.', '_')));
        }

        private static string AddToNtr(EmbededResourceInfo res, string friendlyName)
        {
            if (NameToResource.TryAdd(friendlyName, res))
                return friendlyName;
            for (int i = 2; i < 1000; i++)
            {
                if (NameToResource.TryAdd(friendlyName + i, res))
                    return friendlyName + i;
            }
            throw new Exception("resource with the name alredy registered too many times");
        }

        class EmbededResourceInfo : IEquatable<EmbededResourceInfo>
        {
            public Assembly Assembly { get; set; }
            public string Name { get; set; }
            public string MimeType { get; set; }

            public bool Equals(EmbededResourceInfo other)
            {
                if (other == null) return false;
                return Assembly.Equals(other.Assembly) && Name.Equals(other.Name);
            }

            public override bool Equals(object obj)
            {
                return object.ReferenceEquals(this, obj) || Equals(obj as EmbededResourceInfo);
            }

            public override int GetHashCode()
            {
                return Assembly.GetHashCode() ^ Name.GetHashCode();
            }
        }
    }
}
