using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmAuthentication
    {
        /// <summary>
        /// Ensures the redirect required by the ASP.NET Core Security middleware is properly handled by DotVVM client library.
        /// </summary>
        public static Task ApplyRedirect(HttpContext context, string redirectUri)
        {
            DotvvmRequestContext.SetRedirectResponse(DotvvmMiddleware.ConvertHttpContext(context), redirectUri, (int)HttpStatusCode.Redirect, true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Changes status code of the response to correspond with authentication state.
        /// </summary>
        public static Task SetStatusCode(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        }
    }
}