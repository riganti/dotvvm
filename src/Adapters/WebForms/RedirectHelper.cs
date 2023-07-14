using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using DotVVM.Framework.Routing;

#if NETFRAMEWORK
using System.Web.Routing;
#endif

namespace DotVVM.Framework.Hosting
{
    public static class DotvvmRequestContextExtensions
    {
        public static void RedirectToRouteHybrid(this IDotvvmRequestContext context, string routeName, object routeValues = null, string urlSuffix = null, object query = null)
        {
            if (context.Configuration.RouteTable.Contains(routeName))
            {
                // we have DotVVM route - use it
                var url = context.Configuration.RouteTable[routeName].BuildUrl(routeValues);
                url += UrlHelper.BuildUrlSuffix(urlSuffix, query);
                context.RedirectToUrl(url);
            }
#if NETFRAMEWORK
            else if (RouteTable.Routes[routeName] is Route webFormsRoute)
            {
                // fall back to the Web Forms route
                var url = webFormsRoute.GetVirtualPath(HttpContext.Current.Request.RequestContext, new RouteValueDictionary(routeValues))!.VirtualPath;
                url += UrlHelper.BuildUrlSuffix(urlSuffix, query);
                context.RedirectToUrl(url);
            }
#endif
            else
            {
                throw new ArgumentException($"The route {routeName} doesn't exist!");
            }
        }
    }
}
