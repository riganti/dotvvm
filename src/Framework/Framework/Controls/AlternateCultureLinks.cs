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
    public class AlternateCultureLinks : CompositeControl 
    {
        private readonly IDotvvmRequestContext context;

        public AlternateCultureLinks(IDotvvmRequestContext context)
        {
            this.context = context;
        }

        public IEnumerable<DotvvmControl> GetContents(string? routeName = null)
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
                    var absoluteAlternateUrl = BuildAbsoluteAlternateUrl(alternateUrl);

                    yield return new HtmlGenericControl("link")
                        .SetAttribute("rel", "alternate")
                        .SetAttribute("hreflang", languageCode)
                        .SetAttribute("href", absoluteAlternateUrl);
                }

            }
        }

        protected virtual string BuildAbsoluteAlternateUrl(string alternateUrl)
        {
            return new Uri(context.HttpContext.Request.Url, alternateUrl).AbsoluteUri;
        }
    }
}
