using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace DotVVM.Framework.Hosting
{
    public static class DotvvmRequestContextExtensions
    {
        /// <summary>
        /// Returns the underlying OWIN environment context.
        /// </summary>
        /// <param name="context">The request context.</param>
        public static IOwinContext GetOwinContext(this IDotvvmRequestContext context)
        {
            var concreteContext = context.HttpContext as DotvvmHttpContext;

            if (concreteContext == null)
            {
                throw new NotSupportedException("This app must run on AspNetCore hosting.");
            }

            return concreteContext.OriginalContext;
        }

        /// <summary>
        /// Gets the Authentication functionality available on the current request.
        /// </summary>
        /// <param name="context">The request context.</param>
        public static IAuthenticationManager GetAuthentication(this IDotvvmRequestContext context)
            => context.GetOwinContext().Authentication;
    }
}