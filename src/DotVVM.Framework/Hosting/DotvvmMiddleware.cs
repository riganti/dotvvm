using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using System.Collections.Concurrent;
using System.Threading;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// A middleware that handles DotVVM HTTP requests.
    /// </summary>
    public class DotvvmMiddleware : OwinMiddleware
    {
        public readonly DotvvmConfiguration Configuration;

        private const string GooglebotHashbangEscapedFragment = "_escaped_fragment_=";


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmMiddleware"/> class.
        /// </summary>
        public DotvvmMiddleware(OwinMiddleware next, DotvvmConfiguration configuration) : base(next)
        {
            Configuration = configuration;
        }
        private int configurationSaved = 0;
        /// <summary>
        /// Process an individual request.
        /// </summary>
        public override Task Invoke(IOwinContext context)
        {
            if (Interlocked.Exchange(ref configurationSaved, 1) == 0) {
                VisualStudioHelper.DumpConfiguration(Configuration, Configuration.ApplicationPhysicalPath);
            }
            // create the context
            var dotvvmContext = new DotvvmRequestContext()
            {
                OwinContext = context,
                Configuration = Configuration,
                ResourceManager = new ResourceManager(Configuration),
                ViewModelSerializer = Configuration.ServiceLocator.GetService<IViewModelSerializer>()
            };

            // attempt to translate Googlebot hashbang espaced fragment URL to a plain URL string.
            string url;
            if (!TryParseGooglebotHashbangEscapedFragment(context.Request.QueryString, out url))
            {
                url = context.Request.Path.Value;
            }
            url = url.Trim('/');

            // find the route
            IDictionary<string, object> parameters = null;
            var route = Configuration.RouteTable.FirstOrDefault(r => r.IsMatch(url, out parameters));

            if (route != null)
            {
                // handle the request
                dotvvmContext.Route = route;
                dotvvmContext.Parameters = parameters;
                dotvvmContext.Query = context.Request.Query
                    .ToDictionary(d => d.Key, d => d.Value.Length == 1 ? (object) d.Value[0] : d.Value);

                return route.ProcessRequest(dotvvmContext);
            }
            else
            {
                // we cannot handle the request, pass it to another component
                return Next.Invoke(context);
            }
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

        public static bool IsInCurrentVirtualDirectory(IOwinContext context, ref string url)
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
        /// Determines the current OWIN virtual directory.
        /// </summary>
        public static string GetVirtualDirectory(IOwinContext owinContext)
        {
            return ((string)owinContext.Request.Environment["owin.RequestPathBase"]).Trim('/');
        }

        public static string GetCleanRequestUrl(IOwinContext context)
        {
            return context.Request.Path.Value.TrimStart('/').TrimEnd('/');
        }
    }
}