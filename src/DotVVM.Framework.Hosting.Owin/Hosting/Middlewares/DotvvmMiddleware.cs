using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// A middleware that handles DotVVM HTTP requests.
    /// </summary>
    public class DotvvmMiddleware : OwinMiddleware
    {
        public readonly DotvvmConfiguration Configuration;
        private readonly IList<IMiddleware> middlewares;
        private int configurationSaved;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMiddleware" /> class.
        /// </summary>
        public DotvvmMiddleware(OwinMiddleware next, DotvvmConfiguration configuration, IList<IMiddleware> middlewares)
            : base(next)
        {
            Configuration = configuration;
            this.middlewares = middlewares;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        public override async Task Invoke(IOwinContext context)
        {
            if (Interlocked.Exchange(ref configurationSaved, 1) == 0)
            {
                VisualStudioHelper.DumpConfiguration(Configuration, Configuration.ApplicationPhysicalPath);
            }

            using (var scope = Configuration.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // create the context
                var dotvvmContext = CreateDotvvmContext(context, scope);
                dotvvmContext.Services.GetRequiredService<DotvvmRequestContextStorage>().Context = dotvvmContext;
                context.Set(HostingConstants.DotvvmRequestContextOwinKey, dotvvmContext);
                dotvvmContext.ChangeCurrentCulture(Configuration.DefaultCulture);

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

                // we cannot handle the request, pass it to another component
                await Next.Invoke(context);
            }
        }

        public static IHttpContext ConvertHttpContext(IOwinContext context)
        {
            if (context.Environment.ContainsKey(typeof(IHttpContext).FullName))
            {
                return (IHttpContext)context.Environment[typeof(IHttpContext).FullName];
            }
            var httpContext = new DotvvmHttpContext(context);

            httpContext.Response = new DotvvmHttpResponse(
                context.Response,
                httpContext,
                new DotvvmHeaderCollection(context.Response.Headers)
            );

            httpContext.Request = new DotvvmHttpRequest(
                context.Request,
                httpContext,
                new DotvvmHttpPathString(context.Request.Path),
                new DotvvmHttpPathString(context.Request.PathBase),
                new DotvvmQueryCollection(context.Request.Query),
                new DotvvmHeaderCollection(context.Request.Headers),
                new DotvvmCookieCollection(context.Request.Cookies)
            );
            context.Environment[typeof(IHttpContext).FullName] = httpContext;
            return httpContext;
        }

        protected DotvvmRequestContext CreateDotvvmContext(IOwinContext context, IServiceScope scope)
        {
            return new DotvvmRequestContext {
                Services = scope.ServiceProvider,
                HttpContext = ConvertHttpContext(context),
                Configuration = Configuration,
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
        /// Determines the current virtual directory.
        /// </summary>
        public static string GetVirtualDirectory(IHttpContext context)
        {
            return context.Request.PathBase.Value.Trim('/');
        }

        /// <summary>
        /// Get clean request url without slashes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetCleanRequestUrl(IHttpContext context)
        {
            return context.Request.Path.Value.TrimStart('/').TrimEnd('/');
        }
    }
}
