using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

namespace DotVVM.Framework.Hosting
{
    public static class AspNetCoreDotvvmRequestContextExtensions
    {
        /// <summary>
        /// Returns the underlying ASP.NET Core context.
        /// </summary>
        /// <param name="context">The request context.</param>
        public static HttpContext GetAspNetCoreContext(this IDotvvmRequestContext context)
        {
            var concreteContext = context.HttpContext as DotvvmHttpContext;

            if (concreteContext == null)
            {
                throw new PlatformNotSupportedException("This method can be used only in ASP.NET Core hosting!");
            }

            return concreteContext.OriginalContext;
        }

        /// <summary>
        /// Gets the Authentication functionality available on the current request.
        /// </summary>
        /// <param name="context">The request context.</param>
        public static AuthenticationManager GetAuthentication(this IDotvvmRequestContext context)
            => context.GetAspNetCoreContext().Authentication;
    }
}