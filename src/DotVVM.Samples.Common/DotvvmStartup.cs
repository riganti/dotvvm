using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Samples.BasicSamples.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Serialization;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.ServerSideStyles;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.DefaultCulture = "en-US";

            AddControls(config);
            AddStyles(config);

            AddRoutes(config);

            RegisterResources(config);

            // import namespaces
            config.Markup.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace1", "TestNamespaceAlias"));
            config.Markup.ImportedNamespaces.Add(new Framework.Compilation.NamespaceImport("DotVVM.Samples.BasicSamples.TestNamespace2"));


            // configure serializer
            config.GetSerializationMapper()
                .Map(typeof(SerializationViewModel), map => {
                    map.Property(nameof(SerializationViewModel.Value)).Bind(Direction.ServerToClient);
                    map.Property(nameof(SerializationViewModel.Value2)).Bind(Direction.ClientToServer);
                    map.Property(nameof(SerializationViewModel.IgnoredProperty)).Ignore();
                });
            // new GithubApiClient.GithubApiClient().Repos.GetIssues()
            config.RegisterApiGroup(typeof(TestWebApiClient.TestWebApiClient), "http://localhost:5001/", "Scripts/TestWebApiClient.js");
            config.RegisterApiGroup(typeof(GithubApiClient.GithubApiClient), "https://api.github.com/", "Scripts/GithubApiClient.js", "_github", customFetchFunction: "basicAuthenticatedFetch");
            config.RegisterApiClient(typeof(AzureFunctionsApi.Client), "https://dotvvmazurefunctionstest.azurewebsites.net/", "Scripts/AzureFunctionsApiClient.js", "_azureFuncApi");
        }

        private static void RegisterResources(DotvvmConfiguration config)
        {
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_testCss", new StylesheetResource(new FileResourceLocation("Content/testResource.css")));
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_testJs", new ScriptResource(new FileResourceLocation("Scripts/testResource.js")));
            config.Resources.Register("ControlSamples_SpaContentPlaceHolder_MasterPageResource", new ScriptResource(new FileResourceLocation("Scripts/testResource2.js")));

            config.Resources.Register("FeatureSamples_Resources_CdnUnavailableResourceLoad", new ScriptResource() {
                Location = new UrlResourceLocation("http://unavailable.local/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource.js"))
            });

            config.Resources.Register("FeatureSamples_Resources_CdnScriptPriority", new ScriptResource {
                Location = new FileResourceLocation("~/Scripts/testResource.js"),
                LocationFallback = new ResourceLocationFallback("window.dotvvmTestResource", new FileResourceLocation("~/Scripts/testResource2.js"))
            });

            config.Resources.Register("extenders", new ScriptResource
            {
                Location = new FileResourceLocation("Scripts/ClientExtenders.js")
            });

            // dev files
            config.Resources.SetEmbeddedResourceDebugFile("dotvvm.internal", "../DotVVM.Framework/Resources/Scripts/DotVVM.js");
            config.Resources.SetEmbeddedResourceDebugFile("dotvvm.debug", "../DotVVM.Framework/Resources/Scripts/DotVVM.Debug.js");
            config.Resources.SetEmbeddedResourceDebugFile("dotvvm.fileupload-css", "../DotVVM.Framework/Resources/Scripts/DotVVM.FileUploads.css");

            // test debug version of knockout
            //((ScriptResource)config.Resources.FindResource("knockout"))
            //    .Location = new FileResourceLocation("..\\DotVVM.Framework\\Resources\\Scripts\\knockout-latest.debug.js");

        }

        public static void AddStyles(DotvvmConfiguration config)
        {
            // HasViewInDirectory samples
            config.Styles.Register<Controls.ServerSideStylesControl>(c => c.HasViewInDirectory("Views/FeatureSamples/ServerSideStyles/DirectoryStyle/"))
                .SetAttribute("directory", "matching");
            config.Styles.Register("customTagName", c => c.HasViewInDirectory("Views/FeatureSamples/ServerSideStyles/DirectoryStyle/"))
                .SetAttribute("directory", "matching");

            // HasDataContext and HasRootDataContext samples
            config.Styles.Register("customDataContextTag", c => c.HasRootDataContext<ServerSideStylesMatchingViewModel>()).
                SetAttribute("rootDataContextCheck", "matching");
            config.Styles.Register("customDataContextTag", c => c.HasDataContext<ServerSideStylesMatchingViewModel.TestingObject>()).
                SetAttribute("dataContextCheck", "matching");

            // All style samples
            config.Styles.Register<Controls.ServerSideStylesControl>()
                .SetAttribute("value", "Text changed")
                .SetDotvvmProperty(Controls.ServerSideStylesControl.CustomProperty, "Custom property changed", false)
                .SetAttribute("class", "Class changed", true);
            config.Styles.Register("customTagName")
                .SetAttribute("noAppend", "Attribute changed")
                .SetAttribute("append", "Attribute changed", true);
            config.Styles.Register<Controls.ServerSideStylesControl>(c => c.HasProperty(Controls.ServerSideStylesControl.CustomProperty), false)
                .SetAttribute("derivedAttr", "Derived attribute");
            config.Styles.Register<Controls.ServerSideStylesControl>(c => c.HasProperty(Controls.ServerSideStylesControl.AddedProperty))
                .SetAttribute("addedAttr", "Added attribute");
        }

        private static void AddRoutes(DotvvmConfiguration config)
        {
            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.Add("ComplexSamples_SPARedirect_home", "ComplexSamples/SPARedirect", "Views/ComplexSamples/SPARedirect/home.dothtml");
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PageA", "ControlSamples/SpaContentPlaceHolder/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA", "ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("FeatureSamples_ParameterBinding_ParameterBinding", "FeatureSamples/ParameterBinding/ParameterBinding/{A}", "Views/FeatureSamples/ParameterBinding/ParameterBinding.dothtml", new { A = 123 });
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml", new { Id = 0 });
            config.RouteTable.Add("RepeaterRouteLinkUrlSuffix-PageDetail", "ControlSamples/Repeater/RouteLinkUrlSuffix/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml", new { Id = 0 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectFromPresenter", "FeatureSamples/Redirect/RedirectFromPresenter", null, null, () => new RedirectingPresenter());
            config.RouteTable.Add("FeatureSamples_Validation_ClientSideValidationDisabling2", "FeatureSamples/Validation/ClientSideValidationDisabling/{ClientSideValidationEnabled}", "Views/FeatureSamples/Validation/ClientSideValidationDisabling.dothtml", new { ClientSideValidationEnabled = false });
            config.RouteTable.Add("FeatureSamples_EmbeddedResourceControls_EmbeddedResourceView", "FeatureSamples/EmbeddedResourceControls/EmbeddedResourceView", "embedded://EmbeddedResourceControls/EmbeddedResourceView.dothtml");
        }

        private static void AddControls(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("cc", typeof(Controls.ServerSideStylesControl));
            config.Markup.AddCodeControls("cc", typeof(Controls.DerivedControl));
            config.Markup.AddCodeControls("PropertyUpdate", typeof(Controls.ServerRenderedLabel));
            config.Markup.AddCodeControls("cc", typeof(Controls.PromptButton));
            config.Markup.AddMarkupControl("IdGeneration", "Control", "Views/FeatureSamples/IdGeneration/IdGeneration_control.dotcontrol");
            config.Markup.AddMarkupControl("FileUploadInRepeater", "FileUploadWrapper", "Views/ComplexSamples/FileUploadInRepeater/FileUploadWrapper.dotcontrol");
            config.Markup.AddMarkupControl("sample", "PasswordStrengthControl", "Views/FeatureSamples/ClientExtenders/PasswordStrengthControl.dotcontrol");
            config.Markup.AddMarkupControl("sample", "Localization_Control", "Views/FeatureSamples/Localization/Localization_Control.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlCommandBinding", "Views/FeatureSamples/MarkupControl/ControlCommandBinding.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlValueBindingWithCommand", "Views/FeatureSamples/MarkupControl/ControlValueBindingWithCommand.dotcontrol");
            config.Markup.AddMarkupControl("sample", "EmbeddedResourceControls_Button", "embedded://EmbeddedResourceControls/Button.dotcontrol");

            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/ComplexSamples/ServerRendering/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/FeatureSamples/MarkupControl/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/Errors/"));
        }
    }
}
