using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class JQueryGlobalizeCultureMiddleware : IMiddleware
    {
        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            if (url.StartsWith(HostingConstants.GlobalizeCultureUrlPath, StringComparison.Ordinal))
            {
                await RenderResponse(request.HttpContext);
                return true;
            }
            else
            {
                return false;
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
