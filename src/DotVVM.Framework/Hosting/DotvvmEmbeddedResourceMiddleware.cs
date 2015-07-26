using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Parser;

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
                return RenderEmbeddedResource(context);
            }
            else
            {
                return Next.Invoke(context);
            }
        }



        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private async Task RenderEmbeddedResource(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var resourceName = context.Request.Query["name"];
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == context.Request.Query["assembly"]);

            if (resourceName.EndsWith(".js"))
            {
                context.Response.ContentType = "text/javascript";
            }
            else if (resourceName.EndsWith(".css"))
            {
                context.Response.ContentType = "text/css";
            }
            else
            {
                context.Response.ContentType = "application/octet-stream";
            }

            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                await resourceStream.CopyToAsync(context.Response.Body);
            }
        }
    }
}
