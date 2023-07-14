using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

#if NETFRAMEWORK
using System.Web.Routing;
#endif

namespace DotVVM.Adapters.WebForms.Controls
{
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
