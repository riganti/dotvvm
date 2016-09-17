using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using System.Collections.Concurrent;
using System.Threading;
using DotVVM.Framework.Hosting.ErrorPages;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.AspNetCore.Http;
using DotVVM.Framework.Pipeline;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// A middleware that handles DotVVM HTTP requests.
    /// </summary>
    public class DotvvmMiddleware : DotvvmMiddlewareBase
    {
        public readonly DotvvmConfiguration Configuration;

        private const string GooglebotHashbangEscapedFragment = "_escaped_fragment_=";


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMiddleware"/> class.
        /// </summary>
        public DotvvmMiddleware(RequestDelegate next, DotvvmConfiguration configuration)
        {
            this.next = next;
            Configuration = configuration;
        }

        private int configurationSaved = 0;
        private readonly RequestDelegate next;

        /// <summary>
        /// Process an individual request.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (Interlocked.Exchange(ref configurationSaved, 1) == 0)
                {
                    VisualStudioHelper.DumpConfiguration(Configuration, Configuration.ApplicationPhysicalPath);
                }
                // create the context
                var dotvvmContext = CreateDotvvmContext(context);
                context.Items.Add(HostingConstants.DotvvmRequestContextOwinKey, dotvvmContext);

                await new Pipeline<IDotvvmRequestContext>()
                    .Send(dotvvmContext)
                    .Through(dotvvmContext.Configuration.RequestMiddlewares)
                    .Then(p => ProcessRouting((DotvvmRequestContext)p, context));
            }
            catch (NoRouteException)
            {
                //no route, so continue to the next middleware whatever it is
                await next(context);
            }
        }

        /// <summary>
        /// Handle routing process.
        /// </summary>
        /// <param name="dotvvmContext"></param>
        /// <param name="originalContext"></param>
        /// <returns></returns>
        public async Task ProcessRouting(DotvvmRequestContext dotvvmContext, HttpContext originalContext)
        {
            // attempt to translate Googlebot hashbang espaced fragment URL to a plain URL string.
            string url;
            if (!TryParseGooglebotHashbangEscapedFragment(originalContext.Request.QueryString, out url))
            {
                url = originalContext.Request.Path.Value;
            }
            url = url.Trim('/');

            // find the route
            IDictionary<string, object> parameters = null;
            var route = Configuration.RouteTable.FirstOrDefault(r => r.IsMatch(url, out parameters));

            //check if route exists
            if (route == null) throw new NoRouteException();

            dotvvmContext.Route = route;
            dotvvmContext.Parameters = parameters;
            dotvvmContext.Query = originalContext.Request.Query
                .ToDictionary(d => d.Key, d => d.Value.Count == 1 ? (object) d.Value[0] : d.Value.ToArray());

            try
            {
                await route.ProcessRequest(dotvvmContext);
            }
            catch (DotvvmInterruptRequestExecutionException)
            {
                // the response has already been generated, do nothing
                return;
            }
        }


        public static IHttpContext ConvertHttpContext(HttpContext context)
        {
            var httpContext = context.Features.Get<IHttpContext>();
            if (httpContext == null)
            {
                httpContext = new DotvvmHttpContext(
                    context,
                    new DotvvmHttpAuthentication(context.Authentication))
                {
                    Response = new DotvvmHttpResponse(
                        context.Response,
                        httpContext,
                        new DotvvmHeaderCollection(context.Response.Headers)
                        ),
                    Request = new DotvvmHttpRequest(
                        context.Request,
                        httpContext,
                        new DotvvmHttpPathString(context.Request.Path),
                        new DotvvmHttpPathString(context.Request.PathBase),
                        new DotvvmQueryCollection(context.Request.Query),
                        new DotvvmHeaderCollection(context.Request.Headers),
                        new DotvvmCookieCollection(context.Request.Cookies)
                        )
                };

                context.Features.Set(httpContext);
            }
            return httpContext;
        }

        protected DotvvmRequestContext CreateDotvvmContext(HttpContext context)
        {
            return new DotvvmRequestContext()
            {
                HttpContext = ConvertHttpContext(context),
                Configuration = Configuration,
                ResourceManager = new ResourceManager(Configuration),
                ViewModelSerializer = Configuration.ServiceLocator.GetService<IViewModelSerializer>()
            };
        }

        /// <summary>
        /// Attempts to recognize request made by Googlebot in its effort to crawl links for AJAX SPAs.
        /// </summary>
        /// <param name="queryString">
        /// The query string of the request to try to match the Googlebot hashbang escaped fragment on.
        /// </param>
        /// <param name="url">
        /// The plain URL string that the hasbang escaped fragment represents.
        /// </param>
        /// <returns>
        /// <code>true</code>, if the URL contains valid Googlebot hashbang escaped fragment; otherwise <code>false</code>.
        /// </returns>
        /// <seealso cref="https://developers.google.com/webmasters/ajax-crawling/docs/getting-started"/>
        private bool TryParseGooglebotHashbangEscapedFragment(QueryString queryString, out string url)
        {
            if (queryString.Value.StartsWith(GooglebotHashbangEscapedFragment, StringComparison.Ordinal))
            {
                url = queryString.Value.Substring(GooglebotHashbangEscapedFragment.Length);
                return true;
            }

            url = null;
            return false;
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