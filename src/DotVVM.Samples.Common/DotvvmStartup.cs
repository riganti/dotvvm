using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Samples.BasicSamples.Controls;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Redirect;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Serialization;
using DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand;
using DotVVM.Samples.Common;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.DependencyInjection;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.ServerSideStyles;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.DefaultCulture = "en-US";

            AddControls(config);
            AddStyles(config);

            AddRoutes(config);

            // configure serializer
            config.GetSerializationMapper()
                .Map(typeof(SerializationViewModel), map => {
                    map.Property(nameof(SerializationViewModel.Value)).Bind(Direction.ServerToClient);
                    map.Property(nameof(SerializationViewModel.Value2)).Bind(Direction.ClientToServer);
                    map.Property(nameof(SerializationViewModel.IgnoredProperty)).Ignore();
                });
            // new GithubApiClient.GithubApiClient().Repos.GetIssues()

            config.RegisterApiGroup(typeof(Common.Api.Owin.TestWebApiClientOwin), "http://localhost:61453/", "Scripts/TestWebApiClientOwin.js", "_api");
            config.RegisterApiClient(typeof(Common.Api.AspNetCore.Client), "http://localhost:5001/", "Scripts/TestWebApiClientAspNetCore.js", "_api2");

            config.RegisterApiGroup(typeof(GithubApiClient.GithubApiClient), "https://api.github.com/", "Scripts/GithubApiClient.js", "_github", customFetchFunction: "basicAuthenticatedFetch");
            config.RegisterApiClient(typeof(AzureFunctionsApi.Client), "https://dotvvmazurefunctionstest.azurewebsites.net/", "Scripts/AzureFunctionsApiClient.js", "_azureFuncApi");
        }

        public static void AddStyles(DotvvmConfiguration config)
        {
            // HasViewInDirectory samples
            config.Styles.Register<ServerSideStylesControl>(c => c.HasViewInDirectory("Views/FeatureSamples/ServerSideStyles/DirectoryStyle/"))
                .SetAttribute("directory", "matching");
            config.Styles.Register("customTagName", c => c.HasViewInDirectory("Views/FeatureSamples/ServerSideStyles/DirectoryStyle/"))
                .SetAttribute("directory", "matching");

            // HasDataContext and HasRootDataContext samples
            config.Styles.Register("customDataContextTag", c => c.HasRootDataContext<ServerSideStylesMatchingViewModel>()).
                SetAttribute("rootDataContextCheck", "matching");
            config.Styles.Register("customDataContextTag", c => c.HasDataContext<ServerSideStylesMatchingViewModel.TestingObject>()).
                SetAttribute("dataContextCheck", "matching");

            // All style samples
            config.Styles.Register<ServerSideStylesControl>()
                .SetAttribute("value", "Text changed")
                .SetDotvvmProperty(ServerSideStylesControl.CustomProperty, "Custom property changed", StyleOverrideOptions.Ignore)
                .SetAttribute("class", "Class changed", StyleOverrideOptions.Overwrite);
            config.Styles.Register("customTagName")
                .SetAttribute("ignore", "Attribute ignored", StyleOverrideOptions.Ignore)
                .SetAttribute("overwrite", "Attribute changed", StyleOverrideOptions.Overwrite)
                .SetAttribute("append", "Attribute appended", StyleOverrideOptions.Append)
                .SetAttribute("class", "new-class", StyleOverrideOptions.Append);
            config.Styles.Register<ServerSideStylesControl>(c => c.HasProperty(ServerSideStylesControl.CustomProperty), false)
                .SetAttribute("derivedAttr", "Derived attribute");
            config.Styles.Register<ServerSideStylesControl>(c => c.HasProperty(ServerSideStylesControl.AddedProperty))
                .SetAttribute("addedAttr", "Added attribute");
        }

        private static void AddRoutes(DotvvmConfiguration config)
        {
            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.Add("ComplexSamples_SPARedirect_home", "ComplexSamples/SPARedirect", "Views/ComplexSamples/SPARedirect/home.dothtml");
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PageA", "ControlSamples/SpaContentPlaceHolder/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA", "ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("FeatureSamples_ParameterBinding_ParameterBinding", "FeatureSamples/ParameterBinding/ParameterBinding/{A}", "Views/FeatureSamples/ParameterBinding/ParameterBinding.dothtml", new { A = 123 });
            config.RouteTable.Add("FeatureSamples-Localization", "FeatureSamples/Localization", "Views/FeatureSamples/Localization/Localization.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("FeatureSamples-Localization-Localization_NestedPage_Type", "FeatureSamples/Localization/Localization_NestedPage_Type", "Views/FeatureSamples/Localization/Localization_NestedPage_Type.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("FeatureSamples_ParameterBinding_OptionalParameterBinding", "FeatureSamples/ParameterBinding/OptionalParameterBinding/{Id?}", "Views/FeatureSamples/ParameterBinding/OptionalParameterBinding.dothtml");
            config.RouteTable.Add("FeatureSamples_ParameterBinding_OptionalParameterBinding2", "FeatureSamples/ParameterBinding/OptionalParameterBinding2/{Id?}", "Views/FeatureSamples/ParameterBinding/OptionalParameterBinding.dothtml", new { Id = 300 });

            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));

            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml", new { Id = 0 });
            config.RouteTable.Add("RepeaterRouteLink-PageDetail_IdOptional", "ControlSamples/Repeater/RouteLink/{Id?}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLink-PageDetail_IdOptionalPrefixed", "ControlSamples/Repeater/RouteLink/id-{Id?}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLink-PageDetail_IdOptionalAtStart", "id-{Id?}/ControlSamples/Repeater/RouteLink", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLinkUrlSuffix-PageDetail", "ControlSamples/Repeater/RouteLinkUrlSuffix/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml", new { Id = 0 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectFromPresenter", "FeatureSamples/Redirect/RedirectFromPresenter", provider => new RedirectingPresenter());
            config.RouteTable.Add("FeatureSamples_Validation_ClientSideValidationDisabling2", "FeatureSamples/Validation/ClientSideValidationDisabling/{ClientSideValidationEnabled}", "Views/FeatureSamples/Validation/ClientSideValidationDisabling.dothtml", new { ClientSideValidationEnabled = false });
            config.RouteTable.Add("FeatureSamples_EmbeddedResourceControls_EmbeddedResourceView", "FeatureSamples/EmbeddedResourceControls/EmbeddedResourceView", "embedded://EmbeddedResourceControls/EmbeddedResourceView.dothtml");

            
            config.RouteTable.Add("Errors_Routing_NonExistingView", "Errors/Routing/NonExistingView", "Views/Errors/Routing/NonExistingView.dothml");
        }

        private static void AddControls(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("cc", typeof(Controls.ServerSideStylesControl));
            config.Markup.AddCodeControls("cc", typeof(Controls.DerivedControl));
            config.Markup.AddCodeControls("PropertyUpdate", typeof(Controls.ServerRenderedLabel));
            config.Markup.AddCodeControls("cc", typeof(Controls.PromptButton));
            config.Markup.AddMarkupControl("IdGeneration", "Control", "Views/FeatureSamples/IdGeneration/IdGeneration_control.dotcontrol");
            config.Markup.AddMarkupControl("FileUploadInRepeater", "FileUploadWrapper", "Views/ComplexSamples/FileUploadInRepeater/FileUploadWrapper.dotcontrol");
            config.Markup.AddMarkupControl("sample", "Localization_Control", "Views/FeatureSamples/Localization/Localization_Control.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlCommandBinding", "Views/FeatureSamples/MarkupControl/ControlCommandBinding.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlValueBindingWithCommand", "Views/FeatureSamples/MarkupControl/ControlValueBindingWithCommand.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlWithButton", "Views/ControlSamples/Repeater/SampleControl/ControlWithButton.dotcontrol");
            config.Markup.AddMarkupControl("sample", "ControlControlCommandInvokeAction", "Views/FeatureSamples/MarkupControl/ControlControlCommandInvokeAction.dotcontrol");
            

            config.Markup.AddMarkupControl("sample", "EmbeddedResourceControls_Button", "embedded://EmbeddedResourceControls/Button.dotcontrol");

            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/ComplexSamples/ServerRendering/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/FeatureSamples/MarkupControl/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/FeatureSamples/StaticCommand/"));
            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/Errors/"));
        }

        public void ConfigureServices(IDotvvmServiceCollection services)
        {
            CommonConfiguration.ConfigureServices(services);
            services.AddDefaultTempStorages("Temp");
            services.AddScoped<ViewModelScopedDependency>();
            services.AddSingleton<IGreetingComputationService, HelloGreetingComputationService>();
        }
    }
}
