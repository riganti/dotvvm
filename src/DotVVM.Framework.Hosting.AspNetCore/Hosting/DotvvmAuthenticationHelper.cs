using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmAuthenticationHelper
    {
        /// <summary>
        /// Ensures the redirect required by the ASP.NET Core Security middleware is properly handled by DotVVM client library.
        /// </summary>
        public static Task ApplyRedirectResponse(HttpContext context, string redirectUri)
        {
            DotvvmRequestContextExtensions.SetRedirectResponse(DotvvmRequestContext.GetCurrent(DotvvmMiddleware.ConvertHttpContext(context)), redirectUri, (int)HttpStatusCode.Redirect, allowSpaRedirect: false);
            throw new DotvvmInterruptRequestExecutionException();
        }

        /// <summary>
        /// Changes status code of the response to correspond with authentication state.
        /// </summary>
        public static Task ApplyStatusCodeResponse(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            throw new DotvvmInterruptRequestExecutionException();
        }
    }
}