using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.AspNetCore.Hosting
{
    public static class AspNetCoreHelpers
    {
		public static HttpContext GetAspNetCoreContext(this IDotvvmRequestContext context)
		{
			var concreteContext = context.HttpContext as DotvvmHttpContext;
			if (concreteContext == null) throw new NotSupportedException("This app must run on AspNetCore hosting.");
			return concreteContext.OriginalContext;
		}

		public static AuthenticationManager GetAuthenticationManager(this IDotvvmRequestContext context) => context.GetAspNetCoreContext().Authentication;
    }
}
