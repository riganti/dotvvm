using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Serialization;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.DefaultCulture = "en-US";

            AddControls(config);

            AddRoutes(config);

            RegisterResources(config);

            // import namespaces
            config.Markup.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace1", "TestNamespaceAlias"));
            config.Markup.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace2"));


            // configure serializer
            config.GetSerializationMapper()
                .Map(typeof(SerializationViewModel), map =>
                {
                    map.Property(nameof(SerializationViewModel.Value)).Bind(Direction.ServerToClient);
                    map.Property(nameof(SerializationViewModel.Value2)).Bind(Direction.ClientToServer);
                    map.Property(nameof(SerializationViewModel.IgnoredProperty)).Ignore();
                });
        }

        private static void RegisterResources(DotvvmConfiguration config)
        {
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_testCss", new StylesheetResource(new FileResourceLocation("Content/testResource.css")));
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_testJs", new ScriptResource(new FileResourceLocation("Scripts/testResource.js")));
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_MasterPageResource", new ScriptResource(new FileResourceLocation("Scripts/testResource2.js")));

            config.Resources.Register("FeatureSamples_Resources_CdnUnavailableResourceLoad", new ScriptResource()
            {
                Location = new UrlResourceLocation("http://unavailable.local/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            config.Resources.Register("FeatureSamples_Resources_CdnScriptPriority", new ScriptResource
            {
                Location = new FileResourceLocation("~/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            // dev files
            config.Resources.SetEmbeddedResourceDebugFile("dotvvm.internal", "../DotVVM.Framework/Resources/Scripts/DotVVM.js");
            config.Resources.SetEmbeddedResourceDebugFile("dotvvm.debug", "../DotVVM.Framework/Resources/Scripts/DotVVM.Debug.js");
            config.Resources.SetEmbeddedResourceDebugFile("dotvvm.fileupload-css", "../DotVVM.Framework/Resources/Scripts/DotVVM.FileUploads.css");

        }

        private static void AddRoutes(DotvvmConfiguration config)
        {
            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.Add("ComplexSamples_SPARedirect_home", "ComplexSamples/SPARedirect", "Views/ComplexSamples/SPARedirect/home.dothtml");
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PageA", "ControlSamples/SpaContentPlaceHolder/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder/PageA.dothtml");
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA", "ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLinkUrlSuffix-PageDetail", "ControlSamples/Repeater/RouteLinkUrlSuffix/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectFromPresenter", "FeatureSamples/Redirect/RedirectFromPresenter", null, null, () => new RedirectingPresenter());
            config.RouteTable.Add("FeatureSamples_Validation_ClientSideValidationDisabling2", "FeatureSamples/Validation/ClientSideValidationDisabling/{ClientSideValidationEnabled}", "Views/FeatureSamples/Validation/ClientSideValidationDisabling.dothtml");
        }

        private static void AddControls(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("PropertyUpdate", typeof(Controls.ServerRenderedLabel));
            config.Markup.AddMarkupControl("IdGeneration", "Control", "Views/FeatureSamples/IdGeneration/IdGeneration_control.dotcontrol");
            config.Markup.AddMarkupControl("FileUploadInRepeater", "FileUploadWrapper", "Views/ComplexSamples/FileUploadInRepeater/FileUploadWrapper.dotcontrol");
            config.Markup.AddMarkupControl("sample", "PasswordStrengthControl", "Views/FeatureSamples/ClientExtenders/PasswordStrengthControl.dotcontrol");
            config.Markup.AddMarkupControl("sample", "Localization_Control", "Views/FeatureSamples/Localization/Localization_Control.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlCommandBinding", "Views/FeatureSamples/MarkupControl/ControlCommandBinding.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlValueBindingWithCommand", "Views/FeatureSamples/MarkupControl/ControlValueBindingWithCommand.dotcontrol");
            
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/ComplexSamples/ServerRendering/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/FeatureSamples/MarkupControl/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/Errors/"));
        }
    }
}
