using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting.ErrorPages;
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
        private readonly bool useErrorPage;
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMiddleware" /> class.
        /// </summary>
        public DotvvmMiddleware(RequestDelegate next, DotvvmConfiguration configuration, IList<IDotvvmMiddleware> middlewares, bool useErrorPage)
        {
            this.next = next;
            Configuration = configuration;
            this.middlewares = middlewares;
            this.useErrorPage = useErrorPage;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            // create the context
            var dotvvmContext = CreateDotvvmContext(context);
            context.RequestServices.GetRequiredService<DotvvmRequestContextStorage>().Context = dotvvmContext;
            context.Items[HostingConstants.DotvvmRequestContextKey] = dotvvmContext;

            var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();

            if (requestCultureFeature == null)
            {
#pragma warning disable CS0618
                dotvvmContext.ChangeCurrentCulture(Configuration.DefaultCulture);
#pragma warning restore CS0618
            }

            try
            {
                foreach (var middleware in middlewares)
                {
                    if (await middleware.Handle(dotvvmContext)) return;
                }
            }
            catch (DotvvmInterruptRequestExecutionException)
            {
                return;
            }
            catch (Exception ex) when (useErrorPage)
            {
                if (context.Response.HasStarted)
                    throw; // the response has already started, don't do anything, we can't write anyway

                context.Response.StatusCode = 500;
                var dotvvmErrorPageRenderer = context.RequestServices.GetRequiredService<DotvvmErrorPageRenderer>();
                await dotvvmErrorPageRenderer.RenderErrorResponse(dotvvmContext.HttpContext, ex);
                return;
            }

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
            return new DotvvmRequestContext(
                ConvertHttpContext(context),
                Configuration,
                context.RequestServices
            );
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
