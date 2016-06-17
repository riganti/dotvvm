using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect;

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
			config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/ComplexSamples/ServerRendering/"));
            config.Markup.AddMarkupControl("sample", "TextEditorControl", "Views/FeatureSamples/MarkupControl/TextEditorControl.dotcontrol");
            config.Markup.AddMarkupControl("sample", "PasswordStrengthControl", "Views/FeatureSamples/ClientExtenders/PasswordStrengthControl.dotcontrol");

            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.Add("ComplexSamples_SPARedirect_home", "ComplexSamples/SPARedirect", "Views/ComplexSamples/SPARedirect/home.dothtml");
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PageA", "ControlSamples/SpaContentPlaceHolder/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder/PageA.dothtml");
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA", "ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLinkUrlSuffix-PageDetail", "ControlSamples/Repeater/RouteLinkUrlSuffix/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectFromPresenter", "FeatureSamples/Redirect/RedirectFromPresenter", null, null, () => new RedirectingPresenter());


            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_testCss", new StylesheetResource() { Url = "~/Content/testResource.css" });
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_testJs", new ScriptResource() { Url = "~/Scripts/testResource.js" });
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_MasterPageResource", new ScriptResource() { Url = "~/Scripts/testResource2.js" });

        }

    }
}
 