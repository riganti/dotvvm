using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class JQueryGlobalizeCultureMiddleware
    {
		private readonly RequestDelegate next;

		public JQueryGlobalizeCultureMiddleware(RequestDelegate next)
        {
			this.next = next;
        }

        public Task Invoke(IHttpContext context)
        {
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);

            if (url.StartsWith(HostingConstants.GlobalizeCultureUrlPath, StringComparison.Ordinal))
            {
                return RenderResponse(context);
            }
            else
            {
                return next(context);
            }
        }



        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private Task RenderResponse(IHttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/javascript";

            var id = context.Request.Query[HostingConstants.GlobalizeCultureUrlIdParameter];

            var js = JQueryGlobalizeScriptCreator.BuildCultureInfoScript(new CultureInfo(id));
            return context.Response.WriteAsync(js);
        }
    }
}
