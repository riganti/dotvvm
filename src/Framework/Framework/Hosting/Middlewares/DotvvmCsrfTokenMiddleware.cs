using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Security;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmCsrfTokenMiddleware : IMiddleware
    {
        private readonly ICsrfProtector csrfProtector;
        public DotvvmCsrfTokenMiddleware(ICsrfProtector csrfProtector)
        {
            this.csrfProtector = csrfProtector;
        }
        public async Task<bool> Handle(IDotvvmRequestContext cx)
        {
            if (DotvvmMiddlewareBase.GetCleanRequestUrl(cx.HttpContext) == HostingConstants.CsrfTokenMatchUrl)
            {
                DotvvmMetrics.LazyCsrfTokenGenerated.Add(1);

                var token = csrfProtector.GenerateToken(cx);
                await cx.HttpContext.Response.WriteAsync(token);
                return true;
            }
            return false;
        }
    }
}
