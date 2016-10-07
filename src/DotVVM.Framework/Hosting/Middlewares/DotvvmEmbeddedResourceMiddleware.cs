using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmEmbeddedResourceMiddleware : IMiddleware
    {
        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            // embedded resource handler URL
            if (url.StartsWith(HostingConstants.ResourceHandlerMatchUrl, StringComparison.Ordinal))
            {
                await RenderEmbeddedResource(request.HttpContext);
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
        private async Task RenderEmbeddedResource(IHttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var resourceName = context.Request.Query["name"].ToString();
            var assembly = ReflectionUtils.GetAllAssemblies().FirstOrDefault(a => a.GetName().Name == context.Request.Query["assembly"]);
            if (resourceName.EndsWith(".js", StringComparison.Ordinal))
            {
                context.Response.ContentType = "text/javascript";
            }
            else if (resourceName.EndsWith(".css", StringComparison.Ordinal))
            {
                context.Response.ContentType = "text/css";
            }
            else
            {
                context.Response.ContentType = "application/octet-stream";
            }

            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                await resourceStream.CopyToAsync(context.Response.Body);
            }
        }
    }
}