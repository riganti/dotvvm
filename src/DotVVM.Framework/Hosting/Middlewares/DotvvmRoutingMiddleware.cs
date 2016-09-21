using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmRoutingMiddleware: IMiddleware
    {
        private const string GooglebotHashbangEscapedFragment = "_escaped_fragment_=";
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
        private bool TryParseGooglebotHashbangEscapedFragment(string queryString, out string url)
        {
            if (queryString?.StartsWith(GooglebotHashbangEscapedFragment, StringComparison.Ordinal) == true)
            {
                url = queryString.Substring(GooglebotHashbangEscapedFragment.Length);
                return true;
            }

            url = null;
            return false;
        }


        public async Task<bool> Handle(IDotvvmRequestContext dotvvmContext)
        {
            var context = (DotvvmRequestContext)dotvvmContext;
            // attempt to translate Googlebot hashbang espaced fragment URL to a plain URL string.
            string url;
            if (!TryParseGooglebotHashbangEscapedFragment(dotvvmContext.HttpContext.Request.QueryString, out url))
            {
                url = dotvvmContext.HttpContext.Request.Path.Value;
            }
            url = url.Trim('/');

            // find the route
            IDictionary<string, object> parameters = null;
            var route = dotvvmContext.Configuration.RouteTable.FirstOrDefault(r => r.IsMatch(url, out parameters));

            //check if route exists
            if (route == null) return false;

            context.Route = route;
            context.Parameters = parameters;

            try
            {
                await route.ProcessRequest(context);
            }
            catch (DotvvmInterruptRequestExecutionException)
            {
                // the response has already been generated, do nothing
            }
            return true;
        }
    }
}
