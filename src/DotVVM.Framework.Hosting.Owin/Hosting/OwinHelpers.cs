using DotVVM.Framework.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.AspNetCore.Hosting
{
    public static class AspNetCoreHelpers
    {
		public static IOwinContext GetOwinContext(this IDotvvmRequestContext context)
		{
			var concreteContext = context.HttpContext as DotvvmHttpContext;
			if (concreteContext == null) throw new NotSupportedException("This app must run on AspNetCore hosting.");
			return concreteContext.OriginalContext;
		}

		public static IAuthenticationManager GetOwinAuthenticationManager(this IDotvvmRequestContext context) => context.GetOwinContext().Authentication;
    }
}
