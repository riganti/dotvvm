using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ResourceManagement;
using System.Collections.Concurrent;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmLocalResourceMiddleware : IMiddleware
    {
        private readonly ILocalResourceUrlManager urlManager;
        private readonly DotvvmConfiguration config;

        public DotvvmLocalResourceMiddleware(ILocalResourceUrlManager urlManager, DotvvmConfiguration configuration)
        {
            this.urlManager = urlManager;
            this.config = configuration;
        }

        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            var sw = ValueStopwatch.StartNew(isActive: DotvvmMetrics.ResourceServeDuration.Enabled);
            var resource = urlManager.FindResource(request.HttpContext.Request.Url.ToString(), request, out var mimeType);
            if (resource != null)
            {
                try
                {
                    request.HttpContext.Response.ContentType = mimeType;
                    if (!this.config.Debug)
                        request.HttpContext.Response.Headers.Add("Cache-Control", new[] { "public, max-age=31536000, immutable" });
                    else
                        request.HttpContext.Response.Headers.Add("Cache-Control", new[] { "no-cache, no-store, must-revalidate" });
                    using (var body = resource.LoadResource(request))
                    {
                        if (body.CanSeek)
                            request.HttpContext.Response.Headers["Content-Length"] = body.Length.ToString();

                        await body.CopyToAsync(request.HttpContext.Response.Body);
                    }
                    return true;
                }
                finally
                {
                    DotvvmMetrics.ResourceServeDuration.Record(sw.ElapsedSeconds);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
