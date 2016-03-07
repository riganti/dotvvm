using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.DefaultCulture = "en-US";

            config.Markup.DefaultDirectives.Add(ParserConstants.ResourceTypeDirective, "DotVVM.Samples.BasicSamples.Resources.Resource, DotVVM.Samples.BasicSamples");
            config.Markup.Controls.Add(new DotvvmControlConfiguration()
            {
                Assembly = "DotVVM.Samples.BasicSamples",
                Namespace = "DotVVM.Samples.BasicSamples.Controls",
                TagPrefix = "PropertyUpdate"
            });
            config.Markup.Controls.Add(new DotvvmControlConfiguration()
            {
                TagPrefix = "IdGeneration",
                TagName = "Control",
                Src = "Views/FeatureSamples/IdGeneration/IdGeneration_control.dotcontrol"
            });
            config.Markup.Controls.Add(new DotvvmControlConfiguration()
            {
                TagPrefix = "FileUploadInRepeater",
                TagName = "FileUploadWrapper",
                Src = "Views/ComplexSamples/FileUploadInRepeater/FileUploadWrapper.dotcontrol"
            });
            config.Markup.Controls.Add(new DotvvmControlConfiguration()
            {
                TagPrefix = "sample",
                TagName = "ControlPropertyUpdating",
                Src = "Views/FeatureSamples/MarkupControl/ControlPropertyUpdating.dotcontrol"
            });

            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
        }
        
    }
}
 