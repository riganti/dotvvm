using Microsoft.Owin;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Hosting
{
    public class JQueryGlobalizeCultureMiddleware : OwinMiddleware
    {
        public JQueryGlobalizeCultureMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            var url = DotvvmMiddleware.GetCleanRequestUrl(DotvvmMiddleware.ConvertHttpContext(context));

            if (url.StartsWith(HostingConstants.GlobalizeCultureUrlPath, StringComparison.Ordinal))
            {
                return RenderResponse(context);
            }
            else
            {
                return Next.Invoke(context);
            }
        }



        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private Task RenderResponse(IOwinContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/javascript";

            var id = context.Request.Query[HostingConstants.GlobalizeCultureUrlIdParameter];

            var js = JQueryGlobalizeScriptCreator.BuildCultureInfoScript(CultureInfo.GetCultureInfo(id));
            return context.Response.WriteAsync(js);
        }
    }
}

