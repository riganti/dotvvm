using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
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

        public DotvvmLocalResourceMiddleware(ILocalResourceUrlManager urlManager, DotvvmConfiguration configuration)
        {
            this.urlManager = urlManager;
            this.alternateDirectories = configuration.Debug ? new ConcurrentDictionary<string, string>() : null;
        }

        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            string mimeType;
            var resource = urlManager.FindResource(request.HttpContext.Request.Url.ToString(), request, out mimeType);
            if (resource != null)
            {
                request.HttpContext.Response.ContentType = mimeType;
                request.HttpContext.Response.Headers.Add("Cache-Control", new[] { "public, max-age=31536000, immutable" });
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