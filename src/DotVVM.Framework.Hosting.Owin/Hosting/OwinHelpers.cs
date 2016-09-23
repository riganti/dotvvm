using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace DotVVM.Framework.Hosting
{
    public static class OwinHelpers
    {
        public static IOwinContext GetOwinContext(this IDotvvmRequestContext context)
        {
            var concreteContext = context.HttpContext as DotvvmHttpContext;

            if (concreteContext == null)
            {
                throw new NotSupportedException("This app must run on AspNetCore hosting.");
            }

            return concreteContext.OriginalContext;
        }

        public static IAuthenticationManager GetAuthentication(this IDotvvmRequestContext context)
            => context.GetOwinContext().Authentication;
    }
}