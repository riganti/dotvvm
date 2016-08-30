using System.Net;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmAuthenticationHelper
    {
        /// <summary>
        /// Fixes the response created by the OWIN Security Challenge call to be accepted by DotVVM client library.
        /// </summary>
			
        public static void ApplyRedirectResponse(IHttpContext context, string redirectUri)
        {

			if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                DotvvmRequestContext.SetRedirectResponse(context, redirectUri, (int)HttpStatusCode.Redirect, forceRefresh: true);
            }
        }
    }
}