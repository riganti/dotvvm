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
        private readonly ConcurrentDictionary<string, string> alternateDirectories;
        private readonly bool allowCache;

        public DotvvmLocalResourceMiddleware(ILocalResourceUrlManager urlManager, DotvvmConfiguration configuration)
        {
            this.urlManager = urlManager;
            this.alternateDirectories = configuration.Debug ? new ConcurrentDictionary<string, string>() : null;
            this.allowCache = configuration.Debug;
        }

        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            var resource = urlManager.FindResource(request.HttpContext.Request.Url.ToString(), request, out string mimeType);
            if (resource != null)
            {
                request.HttpContext.Response.ContentType = mimeType;
                if (allowCache)
                    request.HttpContext.Response.Headers.Add("Cache-Control", new[] { "public, max-age=31536000, immutable" });
                else
                    request.HttpContext.Response.Headers.Add("Cache-Control", new[] { "no-store, must-revalidate" });
                using (var body = resource.LoadResource(request))
                {
                    await body.CopyToAsync(request.HttpContext.Response.Body);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}