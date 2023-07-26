using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

#if NETFRAMEWORK
using System.Web.Routing;
#endif

namespace DotVVM.Adapters.WebForms.Controls
{
    /// <summary>
    /// Renders a hyperlink pointing to the specified DotVVM route if such route exists; otherwise it falls back to a Web Forms route with the specified name.
    /// </summary>
#if !NETFRAMEWORK
    [Obsolete("This control is used only during the Web Forms migration and is not needed in .NET Core. Use the standard RouteLink control.")]
#endif
    public class HybridRouteLink : CompositeControl
    {
        private readonly IDotvvmRequestContext context;

        public HybridRouteLink(IDotvvmRequestContext context)
        {
            this.context = context;
        }

        public DotvvmControl GetContents(
            HtmlCapability htmlCapability,
            TextOrContentCapability textOrContent,
            RouteLinkCapability routeLinkCapability
        )
        {
            if (context.Configuration.RouteTable.Contains(routeLinkCapability.RouteName))
            {
                return GenerateDotvvmRouteLink(htmlCapability, textOrContent, routeLinkCapability);
            }
#if NETFRAMEWORK
            else if (RouteTable.Routes[routeLinkCapability.RouteName] is Route webFormsRoute)
            {
                return WebFormsLinkUtils.BuildWebFormsRouteLink(this, context, htmlCapability, textOrContent, routeLinkCapability, webFormsRoute);
            }
#endif
            else
            {
                throw new DotvvmControlException($"Route '{routeLinkCapability.RouteName}' does not exist.");
            }
        }

        private static DotvvmControl GenerateDotvvmRouteLink(HtmlCapability htmlCapability, TextOrContentCapability textOrContent, RouteLinkCapability routeLinkCapability)
        {
            var link = new RouteLink()
                .SetCapability(htmlCapability)
                .SetCapability(textOrContent)
                .SetProperty(r => r.RouteName, routeLinkCapability.RouteName)
                .SetProperty(l => l.UrlSuffix, routeLinkCapability.UrlSuffix);
            link.QueryParameters.CopyFrom(routeLinkCapability.QueryParameters);
            link.Params.CopyFrom(routeLinkCapability.Params);
            return link;
        }
        
    }
}
