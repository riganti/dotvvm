using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace DotVVM.Framework.Hosting
{
    public static class OwinDotvvmRequestContextExtensions
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
                throw new NotSupportedException("This method can be used only in OWIN hosting!");
            }

            return concreteContext.OriginalContext;
        }

        /// <summary>
        /// Gets the Authentication functionality available on the current request.
        /// </summary>
        /// <param name="context">The request context.</param>
        public static IAuthenticationManager GetAuthentication(this IDotvvmRequestContext context)
            => context.GetOwinContext().Authentication;

        /// <summary>
        /// Gets the <see cref="IDotvvmRequestContext"/> bound to the specified <see cref="IOwinContext"/>.
        /// </summary>
        public static IDotvvmRequestContext GetDotvvmContext(this IOwinContext owinContext)
        {
            return owinContext.Get<IDotvvmRequestContext>(HostingConstants.DotvvmRequestContextKey);
        }
    }
}
