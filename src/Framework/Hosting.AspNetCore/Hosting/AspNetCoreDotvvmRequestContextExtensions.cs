using System;
using DotVVM.Framework.Hosting.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
        /// Gets the Authentication Functionality available on current request
        /// </summary>
        /// <param name="context">The request context.</param>
        public static AuthenticationManager GetAuthentication(this IDotvvmRequestContext context)
        {
            var concreteContext = context.HttpContext as DotvvmHttpContext;

            if (concreteContext == null)
            {
                throw new PlatformNotSupportedException("This method can be used only in ASP.NET Core hosting!");
            }
            return new AuthenticationManager(concreteContext.OriginalContext);
        }


        /// <summary>
        /// Gets the <see cref="IDotvvmRequestContext"/> bound to the specified <see cref="HttpContext"/>.
        /// </summary>
        public static IDotvvmRequestContext GetDotvvmContext(this HttpContext httpContext)
        {
            return httpContext.Items.TryGetValue(HostingConstants.DotvvmRequestContextKey, out var value) ? value as IDotvvmRequestContext : null;
        }
    }
}
