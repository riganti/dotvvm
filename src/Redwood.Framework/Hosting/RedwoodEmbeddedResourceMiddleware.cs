using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// Provides access to embedded resources in the Redwood.Framework assembly.
    /// </summary>
    public class RedwoodEmbeddedResourceMiddleware : OwinMiddleware
    {
        public RedwoodEmbeddedResourceMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            // try resolve the route
            var url = context.Request.Path.Value.TrimStart('/').TrimEnd('/');

            // disable access to the redwood.json file
            if (url.StartsWith("redwood.json", StringComparison.CurrentCultureIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                throw new UnauthorizedAccessException("The redwood.json cannot be served!");
            }

            // embedded resource handler URL
            if (url.StartsWith(Constants.ResourceHandlerMatchUrl))
            {
                RenderEmbeddedResource(context);
            }
            else
            {
                await Next.Invoke(context);
            }
        }



        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private void RenderEmbeddedResource(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/javascript";

            var resourceName = context.Request.Query["name"];
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == context.Request.Query["assembly"]);

            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                resourceStream.CopyTo(context.Response.Body);
            }
        }
    }
}
