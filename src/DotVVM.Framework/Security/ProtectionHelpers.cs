#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security
{
    public static class ProtectionHelpers
    {
        public static string GetRequestIdentity(IDotvvmRequestContext context)
        {
            return context.HttpContext.Request.Url.ToString();
        }

        public static string GetUserIdentity(IDotvvmRequestContext context)
        {
            var user = context.HttpContext.User;
            
            if (user != null && user.Identity.IsAuthenticated)
            {
                return "user" + user.Identity.Name ?? "";
            }
            else
            {
                return "anonymous";
            }
        }
    }
}
