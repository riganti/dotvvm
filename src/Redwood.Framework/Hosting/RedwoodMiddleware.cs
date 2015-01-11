using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Parser;
using Redwood.Framework.ViewModel;
using Redwood.Framework.ResourceManagement;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// A middleware that handles Redwood HTTP requests.
    /// </summary>
    public class RedwoodMiddleware : OwinMiddleware
    {
        private readonly RedwoodConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodMiddleware"/> class.
        /// </summary>
        public RedwoodMiddleware(OwinMiddleware next, RedwoodConfiguration configuration) : base(next)
        {
            this.configuration = configuration;
        }
        
        /// <summary>
        /// Process an individual request.
        /// </summary>
        public override async Task Invoke(IOwinContext context)
        {
            // try resolve the route
            var url = context.Request.Path.Value.TrimStart('/').TrimEnd('/');

            // disable access to the redwood.json file
            if (url.StartsWith("redwood.json", StringComparison.CurrentCultureIgnoreCase))
            {
                await RedwoodPresenter.RenderErrorResponse(new RedwoodRequestContext()
                {
                    Configuration = configuration,
                    OwinContext = context
                }, HttpStatusCode.NotFound, new UnauthorizedAccessException("The redwood.json cannot be served!"));
                return;
            }

            // embedded resource handler URL
            if (url.StartsWith(Constants.ResourceHandlerMatchUrl))
            {
                RenderEmbeddedResource(context);
                return;
            }

            // find the route
            IDictionary<string, object> parameters = null;
            var route = configuration.RouteTable.FirstOrDefault(r => r.IsMatch(url, out parameters));

            if (route != null)
            {
                // handle the request
                await route.ProcessRequest(new RedwoodRequestContext()
                {
                    Route = route,
                    OwinContext = context, 
                    Configuration = configuration,
                    Parameters = parameters,
                    ResourceManager = new ResourceManager(configuration)
                });
            }
            else
            {
                // we cannot handle the request, pass it to another component
                await Next.Invoke(context);
            }
        }

        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private static void RenderEmbeddedResource(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/javascript";

            var resourceName = context.Request.Query["file"];
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                resourceStream.CopyTo(context.Response.Body);
            }
        }
    }
}
