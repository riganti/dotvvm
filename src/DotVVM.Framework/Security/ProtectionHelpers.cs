using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security
{
    public static class ProtectionHelpers
    {
        public static string GetRequestIdentity(DotvvmRequestContext context)
        {
            return context.OwinContext.Request.Uri.ToString();
        }

        public static string GetUserIdentity(DotvvmRequestContext context)
        {
            var user = context.OwinContext.Request.User;
            var userIdentity = user != null && user.Identity.IsAuthenticated ? user.Identity.Name : null;
            return userIdentity;
        }
    }
}