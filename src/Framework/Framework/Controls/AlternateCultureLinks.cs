using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a <c>&lt;link rel=alternate</c> element for each localized route equivalent to the current route.
    /// On non-localized routes, it renders nothing (the control is therefore safe to use in a master page).
    /// The href must be an absolute URL, so it will only work correctly if <c>Context.Request.Url</c> contains the corrent domain.
    /// </summary>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/rel#alternate" />
    /// <seealso href="https://developers.google.com/search/docs/specialty/international/localized-versions#html" />
    /// <seealso cref="DotvvmRouteTable.Add(string, string, Type, object, LocalizedRouteUrl[])"/>
    public class AlternateCultureLinks : CompositeControl 
    {
        /// <param name="routeName">The name of the route to generate alternate links for. If not set, the current route is used. </param>
        public IEnumerable<DotvvmControl> GetContents(IDotvvmRequestContext context, string? routeName = null)
        {
            var route = routeName != null ? context.Configuration.RouteTable[routeName] : context.Route;
            if (route is LocalizedDotvvmRoute localizedRoute)
            {
                var currentCultureRoute = localizedRoute.GetRouteForCulture(CultureInfo.CurrentUICulture);

                foreach (var alternateCultureRoute in localizedRoute.GetAllCultureRoutes())
                {
                    if (alternateCultureRoute.Value == currentCultureRoute) continue;

                    var languageCode = alternateCultureRoute.Key == "" ? "x-default" : alternateCultureRoute.Key.ToLowerInvariant();
                    var alternateUrl = context.TranslateVirtualPath(alternateCultureRoute.Value.BuildUrl(context.Parameters!));
                    var absoluteAlternateUrl = BuildAbsoluteAlternateUrl(context, alternateUrl);

                    yield return new HtmlGenericControl("link")
                        .SetAttribute("rel", "alternate")
                        .SetAttribute("hreflang", languageCode)
                        .SetAttribute("href", absoluteAlternateUrl);
                }

            }
        }

        protected virtual string BuildAbsoluteAlternateUrl(IDotvvmRequestContext context, string alternateUrl)
        {
            return new Uri(context.HttpContext.Request.Url, alternateUrl).AbsoluteUri;
        }
    }
}
