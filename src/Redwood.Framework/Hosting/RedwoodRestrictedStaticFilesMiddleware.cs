using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// Restricts access to static files that shouldn't be downloaded.
    /// </summary>
    public class RedwoodRestrictedStaticFilesMiddleware : OwinMiddleware
    {
        public RedwoodRestrictedStaticFilesMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            // try resolve the route
            var url = RedwoodMiddleware.GetCleanRequestUrl(context);
            
            // disable access to the redwood.json file
            if (url.StartsWith("redwood.json", StringComparison.CurrentCultureIgnoreCase))
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                throw new UnauthorizedAccessException("The redwood.json cannot be served!");
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}
