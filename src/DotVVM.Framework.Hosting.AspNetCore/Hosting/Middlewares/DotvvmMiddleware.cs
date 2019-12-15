using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using IDotvvmMiddleware = DotVVM.Framework.Hosting.Middlewares.IMiddleware;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// A middleware that handles DotVVM HTTP requests.
    /// </summary>
    public class DotvvmMiddleware : DotvvmMiddlewareBase
    {
        public readonly DotvvmConfiguration Configuration;
        private readonly IList<IDotvvmMiddleware> middlewares;
        private readonly RequestDelegate next;

        private int configurationSaved;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMiddleware" /> class.
        /// </summary>
        public DotvvmMiddleware(RequestDelegate next, DotvvmConfiguration configuration, IList<IDotvvmMiddleware> middlewares)
        {
            this.next = next;
            Configuration = configuration;
            this.middlewares = middlewares;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            if (Interlocked.Exchange(ref configurationSaved, 1) == 0)
            {
                VisualStudioHelper.DumpConfiguration(Configuration, Configuration.ApplicationPhysicalPath);
            }
            // create the context
            var dotvvmContext = CreateDotvvmContext(context);
            context.RequestServices.GetRequiredService<DotvvmRequestContextStorage>().Context = dotvvmContext;
            context.Items[HostingConstants.DotvvmRequestContextOwinKey] = dotvvmContext;

            var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();

            if (requestCultureFeature == null)
            {
                dotvvmContext.ChangeCurrentCulture(Configuration.DefaultCulture);
            }

            try
            {
                foreach (var middleware in middlewares)
                {
                    if (await middleware.Handle(dotvvmContext)) return;
                }
            }
            catch (DotvvmInterruptRequestExecutionException) { return; }
            await next(context);
        }

        public static IHttpContext ConvertHttpContext(HttpContext context)
        {
            var httpContext = context.Features.Get<IHttpContext>();
            if (httpContext == null)
            {
                httpContext = new DotvvmHttpContext(context) {
                    Response = new DotvvmHttpResponse(
                        context.Response,
                        httpContext,
                        new DotvvmHeaderCollection(context.Response.Headers)
                    ),
                    Request = new DotvvmHttpRequest(
                        context.Request,
                        httpContext
                    )
                };

                context.Features.Set(httpContext);
            }
            return httpContext;
        }

        protected DotvvmRequestContext CreateDotvvmContext(HttpContext context)
        {
            return new DotvvmRequestContext {
                HttpContext = ConvertHttpContext(context),
                Configuration = Configuration,
                Services = context.RequestServices
            };
        }

        public static bool IsInCurrentVirtualDirectory(IHttpContext context, ref string url)
        {
            var virtualDirectory = GetVirtualDirectory(context);
            if (url.StartsWith(virtualDirectory, StringComparison.Ordinal))
            {
                url = url.Substring(virtualDirectory.Length).TrimStart('/');
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get clean request url without slashes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetCleanRequestUrl(HttpContext context)
        {
            return context.Request.Path.Value.TrimStart('/').TrimEnd('/');
        }
    }
}
