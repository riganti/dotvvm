using DotVVM.Framework.Routing;
using DotVVM.Framework.Runtime.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmRoutingMiddleware : IMiddleware
    {
        private const string GooglebotHashbangEscapedFragment = "_escaped_fragment_=";
        /// <summary>
        /// Attempts to recognize request made by Googlebot in its effort to crawl links for AJAX SPAs.
        /// </summary>
        /// <param name="queryString">
        /// The query string of the request to try to match the Googlebot hashbang escaped fragment on.
        /// </param>
        /// <param name="url">
        /// The plain URL string that the hashbang escaped fragment represents.
        /// </param>
        /// <returns>
        /// <code>true</code>, if the URL contains valid Googlebot hashbang escaped fragment; otherwise <code>false</code>.
        /// </returns>
        /// <see href="https://developers.google.com/webmasters/ajax-crawling/docs/getting-started"/>
        private static bool TryParseGooglebotHashbangEscapedFragment(string queryString, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string url)
        {
            if (queryString?.StartsWith(GooglebotHashbangEscapedFragment, StringComparison.Ordinal) == true)
            {
                url = queryString.Substring(GooglebotHashbangEscapedFragment.Length);
                return true;
            }

            url = null!;
            return false;
        }

        public static RouteBase? FindMatchingRoute(IEnumerable<RouteBase> routes, IDotvvmRequestContext context, out IDictionary<string, object?>? parameters)
        {
            string? url;
            if (!TryParseGooglebotHashbangEscapedFragment(context.HttpContext.Request.Url.Query, out url))
            {
                url = context.HttpContext.Request.Path.Value;
            }
            url = url?.Trim('/') ?? "";

            // remove SPA identifier from the URL
            if (url.StartsWith(HostingConstants.SpaUrlIdentifier, StringComparison.Ordinal))
            {
                url = url.Substring(HostingConstants.SpaUrlIdentifier.Length).Trim('/');
            }


            // find the route
            foreach (var r in routes)
            {
                if (r.IsMatch(url, out parameters)) return r;
            }
            parameters = null;
            return null;
        }


        public async Task<bool> Handle(IDotvvmRequestContext context)
        {
            var requestTracer = context.Services.GetRequiredService<AggregateRequestTracer>();

            await requestTracer.TraceEvent(RequestTracingConstants.BeginRequest, context);

            var route = FindMatchingRoute(context.Configuration.RouteTable, context, out var parameters);

            //check if route exists
            if (route == null) return false;

            context.Route = route;
            context.Parameters = parameters;
            var presenter = context.Presenter = route.GetPresenter(context.Services);

            WriteSecurityHeaders(context);

            var filters =
                ActionFilterHelper.GetActionFilters<IPresenterActionFilter>(presenter.GetType().GetTypeInfo())
                .Concat(context.Configuration.Runtime.GlobalFilters.OfType<IPresenterActionFilter>());
            try
            {
                foreach (var f in filters) await f.OnPresenterExecutingAsync(context);
                await presenter.ProcessRequest(context);
                foreach (var f in filters) await f.OnPresenterExecutedAsync(context);
            }
            catch (DotvvmInterruptRequestExecutionException) { } // the response has already been generated, do nothing
            catch (DotvvmHttpException) { throw; }
            catch (Exception exception)
            {
                foreach (var f in filters)
                {
                    await f.OnPresenterExceptionAsync(context, exception);
                    if (context.IsPageExceptionHandled) context.InterruptRequest();
                }
                await requestTracer.EndRequest(context, exception);
                throw;
            }

            await requestTracer.EndRequest(context);

            return true;
        }

        public static void WriteSecurityHeaders(IDotvvmRequestContext context)
        {
            var config = context.Configuration.Security;
            var route = context.Route?.RouteName;
            var headers = context.HttpContext.Response.Headers;
            if (config.ContentTypeOptionsHeader.IsEnabledForRoute(route) && !headers.ContainsKey("X-Content-Type-Options"))
                headers.Add("X-Content-Type-Options", new [] { "nosniff" });

            if (config.XssProtectionHeader.IsEnabledForRoute(route) && !headers.ContainsKey("X-XSS-Protection"))
                headers.Add("X-XSS-Protection", new [] { "1; mode=block" });

            if (config.FrameOptionsCrossOrigin.IsEnabledForRoute(route) || headers.ContainsKey("X-Frame-Options"))
            {
                // nothing
            }
            else if (config.FrameOptionsSameOrigin.IsEnabledForRoute(route))
                headers.Add("X-Frame-Options", new [] { "SAMEORIGIN" });
            else
                headers.Add("X-Frame-Options", new [] { "DENY" });


        }
    }
}
