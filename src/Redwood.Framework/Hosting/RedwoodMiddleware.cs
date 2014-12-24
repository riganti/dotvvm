using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.ViewModel;

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
                    Parameters = parameters
                });
            }
            else
            {
                // we cannot handle the request, pass it to another component
                await Next.Invoke(context);
            }
        }
    }
}
