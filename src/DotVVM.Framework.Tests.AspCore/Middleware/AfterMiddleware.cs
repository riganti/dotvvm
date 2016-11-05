using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;

namespace DotVVM.Framework.Tests.AspCore.Middleware
{
    public class AfterMiddleware
    {
        public async Task Handle(IDotvvmRequestContext request, Func<IDotvvmRequestContext, Task> next)
        {
            await next(request);
            await request.HttpContext.Response.WriteAsync(MiddlewareTest.AfterFunction);
        }
    }
}