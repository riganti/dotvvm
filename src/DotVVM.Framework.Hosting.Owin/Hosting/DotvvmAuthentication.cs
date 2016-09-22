using System.IO;
using System.Net;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmAuthentication
    {
        /// <summary>
        /// Ensures the redirect required by the OWIN Security middleware is properly handled by DotVVM client library.
        /// </summary>
        public static void ApplyRedirect(IOwinContext context, string redirectUri)
        {
            if (context.Response.StatusCode == 401)
            {
                DotvvmRequestContext.SetRedirectResponse(DotvvmMiddleware.ConvertHttpContext(context), redirectUri, (int)HttpStatusCode.Redirect, true);
            }
        }
    }
}