using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyModel;
#if Owin
using Context = Microsoft.Owin.HttpContext;
#else
using Context = Microsoft.AspNetCore.Http.HttpContext;
#endif

namespace DotVVM.Framework.Hosting.Middlewares
{
    /// <summary>
    /// Provides access to embedded resources in the DotVVM.Framework assembly.
    /// </summary>
    public abstract class ADotvvmEmbeddedResourceMiddleware
    {
        /// <summary>
        /// Renders the embedded resource.
        /// </summary>
        private async Task RenderEmbeddedResource(IHttpContext context)
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
