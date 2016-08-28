using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyModel;
using System.Linq;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmEmbeddedResourceMiddleware
    {
        private readonly RequestDelegate next;

        public DotvvmEmbeddedResourceMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);

            // embedded resource handler URL
            if (url.StartsWith(HostingConstants.ResourceHandlerMatchUrl, StringComparison.Ordinal))
            {
                return RenderEmbeddedResource(context);
            }
            else
            {
                return next(context);
            }
        }

        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private async Task RenderEmbeddedResource(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var resourceName = context.Request.Query["name"].ToString();
            var assemblyName = DependencyContext.Default.GetDefaultAssemblyNames().FirstOrDefault(a => a.Name == context.Request.Query["assembly"]);
            var assembly = Assembly.Load(assemblyName);
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