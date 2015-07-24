using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// Restricts access to static files that shouldn't be downloaded.
    /// </summary>
    public class DotvvmRestrictedStaticFilesMiddleware : OwinMiddleware
    {
        public DotvvmRestrictedStaticFilesMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            // try resolve the route
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);
            
            // disable access to the dotvvm.json file
            if (url.StartsWith("dotvvm.json", StringComparison.CurrentCultureIgnoreCase))
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                throw new UnauthorizedAccessException("The dotvvm.json cannot be served!");
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}
