using DotVVM.Framework.Configuration;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.DefaultCulture = "en-US";
            config.Markup.DefaultDirectives.Add(Constants.ResourceTypeDirective, "DotVVM.Samples.BasicSamples.Resources.Resource, DotVVM.Samples.BasicSamples");

            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
        }
    }
}