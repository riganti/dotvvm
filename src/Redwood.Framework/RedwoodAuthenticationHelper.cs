using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Owin;
using Redwood.Framework.Hosting;

namespace Redwood.Framework
{
    public class RedwoodAuthenticationHelper
    {

        /// <summary>
        /// Fixes the response created by the OWIN Security Challenge call to be accepted by Redwood client library.
        /// </summary>
        public static void ApplyRedirectResponse(IOwinContext context, string redirectUri)
        {
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                RedwoodRequestContext.SetRedirectResponse(context, redirectUri, (int)HttpStatusCode.Redirect);
            }
        }

    }
}