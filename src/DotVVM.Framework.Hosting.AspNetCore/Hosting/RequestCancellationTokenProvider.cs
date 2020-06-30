using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DotVVM.Framework.Hosting.AspNetCore.Hosting
{
    internal class RequestCancellationTokenProvider : IRequestCancellationTokenProvider
    {
        public CancellationToken GetCancellationToken(IDotvvmRequestContext context)
        {
            var httpContext = (DotvvmHttpContext)context.HttpContext;
            return httpContext.OriginalContext.RequestAborted;
        }
    }
}
