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
        public Task Handle(IDotvvmRequestContext request, Func<IDotvvmRequestContext, Task> next)
        {
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            if (url.StartsWith(HostingConstants.GlobalizeCultureUrlPath, StringComparison.Ordinal))
            {
                return RenderResponse(request.HttpContext);
            }
            else
            {
                return next(request);
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
