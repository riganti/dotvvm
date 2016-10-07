using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;

namespace DotVVM.Framework.Tests.AspCore.Middleware
{
    public class BeforeMiddleware
    {
        public async Task Handle(IDotvvmRequestContext request, Func<IDotvvmRequestContext, Task> next)
        {
            request.HttpContext.Response.Write(MiddlewareTest.BeforeFunction);
            next(request);
        }
    }
}