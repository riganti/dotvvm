using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
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
    }
}
