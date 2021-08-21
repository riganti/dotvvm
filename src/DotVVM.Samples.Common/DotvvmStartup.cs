using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
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
using DotVVM.Framework.Controls;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DotVVM.Samples.Common.Api.AspNetCore;
using DotVVM.Samples.Common.Api.Owin;
using DotVVM.Samples.Common.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation;
using DotVVM.Samples.Common.Views.FeatureSamples.PostbackAbortSignal;
using DotVVM.Samples.Common.ViewModels.FeatureSamples.BindingVariables;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        public const string GitHubTokenEnvName = "GITHUB_TOKEN";
        public const string GitHubTokenConfigName = "githubApiToken";

        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.DefaultCulture = "en-US";
            config.UseHistoryApiSpaNavigation = true;

            AddControls(config);
            AddStyles(config);

            AddRedirections(config);
            AddRoutes(config);

            // configure serializer
            config.GetSerializationMapper()
                .Map(typeof(SerializationViewModel), map => {
                    map.Property(nameof(SerializationViewModel.Value)).Bind(Direction.ServerToClient);
                    map.Property(nameof(SerializationViewModel.Value2)).Bind(Direction.ClientToServer);
                    map.Property(nameof(SerializationViewModel.IgnoredProperty)).Ignore();
                });
            // new GithubApiClient.GithubApiClient().Repos.GetIssues()

            config.RegisterApiGroup(typeof(Common.Api.Owin.TestWebApiClientOwin), "http://localhost:61453/", "Scripts/TestWebApiClientOwin.js", "_apiOwin");
            config.RegisterApiClient(typeof(Common.Api.AspNetCore.TestWebApiClientAspNetCore), "http://localhost:5001/", "Scripts/TestWebApiClientAspNetCore.js", "_apiCore");

            config.RegisterApiGroup(typeof(GithubApiClient.GithubApiClient), "https://api.github.com/", "Scripts/GithubApiClient.js", "_github", customFetchFunction: "githubAuthenticatedFetch");
            config.RegisterApiClient(typeof(AzureFunctionsApi.Client), "https://dotvvmazurefunctionstest.azurewebsites.net/", "Scripts/AzureFunctionsApiClient.js", "_azureFuncApi");

            LoadSampleConfiguration(config, applicationPath);

            config.Markup.JavascriptTranslator.MethodCollection.AddMethodTranslator(typeof(JavascriptTranslationTestMethods),
                    nameof(JavascriptTranslationTestMethods.Unwrap),
                         new GenericMethodCompiler((a) =>
                            new JsIdentifierExpression("unwrap")
                                            .Invoke(a[1])
                                    ), allowGeneric: true, allowMultipleMethods: true);

        }

        private void LoadSampleConfiguration(DotvvmConfiguration config, string applicationPath)
        {
            var jsonText = File.ReadAllText(Path.Combine(applicationPath, "sampleConfig.json"));
            var json = JObject.Parse(jsonText);

            // find active profile
            var activeProfile = json.Value<string>("activeProfile");

            var profiles = json.Value<JArray>("profiles");
            var profile = profiles.Single(p => p.Value<string>("name") == activeProfile);

            JsonConvert.PopulateObject(profile.Value<JObject>("config").ToString(), config);

            var githubTokenEnv = Environment.GetEnvironmentVariable(GitHubTokenEnvName);
            if (githubTokenEnv is object)
            {
                json.Value<JObject>("appSettings")[GitHubTokenConfigName] = githubTokenEnv;
            }

            SampleConfiguration.Initialize(
                json.Value<JObject>("appSettings").Properties().ToDictionary(p => p.Name, p => p.Value.Value<string>())
            );
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

            config.Styles.Register<Button>(c => c.HasHtmlAttribute("server-side-style-attribute"))
               .SetControlProperty(PostBack.HandlersProperty, new ConfirmPostBackHandler("ConfirmPostBackHandler Content"));
        }

        private static void AddRedirections(DotvvmConfiguration config)
        {
            config.RouteTable.AddUrlRedirection("Redirection1", "FeatureSamples/Redirect/RedirectionHelpers_PageA", (_) => "https://www.dotvvm.com");
            config.RouteTable.AddRouteRedirection("Redirection2", "FeatureSamples/Redirect/RedirectionHelpers_PageB/{Id}", (_) => "FeatureSamples_Redirect_RedirectionHelpers_PageC-PageDetail", new { Id = 66 }, urlSuffixProvider: c => "?test=aaa");
            config.RouteTable.AddRouteRedirection("Redirection3", "FeatureSamples/Redirect/RedirectionHelpers_PageD/{Id}", (_) => "FeatureSamples_Redirect_RedirectionHelpers_PageE-PageDetail", new { Id = 77 },
                parametersProvider: (context) => {
                    var newDict = new Dictionary<string, object>(context.Parameters);
                    newDict["Id"] = 1221;
                    return newDict;
                });
        }

        private static void AddRoutes(DotvvmConfiguration config)
        {
            config.RouteTable.Add("Default", "", "Views/Default.dothtml");

            config.RouteTable.Add("ComplexSamples_SPARedirect_home", "ComplexSamples/SPARedirect", "Views/ComplexSamples/SPARedirect/home.dothtml");

            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PageA", "ControlSamples/SpaContentPlaceHolder/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA", "ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder_PrefixRouteName/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_HistoryApi_PageA", "ControlSamples/SpaContentPlaceHolder_HistoryApi/PageA/{Id}", "Views/ControlSamples/SpaContentPlaceHolder_HistoryApi/PageA.dothtml", new { Id = 0 });
            config.RouteTable.Add("ControlSamples_SpaContentPlaceHolder_HistoryApi", "ControlSamples/SpaContentPlaceHolder_HistoryApi", "Views/ControlSamples/SpaContentPlaceHolder_HistoryApi/SpaMaster.dotmaster");
            config.RouteTable.Add("ControlSamples_TextBox_TextBox_Format", "ControlSamples/TextBox/TextBox_Format", "Views/ControlSamples/TextBox/TextBox_Format.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("ControlSamples_TextBox_TextBox_Format_Binding", "ControlSamples/TextBox/TextBox_Format_Binding", "Views/ControlSamples/TextBox/TextBox_Format_Binding.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("FeatureSamples_ParameterBinding_ParameterBinding", "FeatureSamples/ParameterBinding/ParameterBinding/{A}", "Views/FeatureSamples/ParameterBinding/ParameterBinding.dothtml", new { A = 123 });
            config.RouteTable.Add("FeatureSamples_Localization_Localization", "FeatureSamples/Localization/Localization", "Views/FeatureSamples/Localization/Localization.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("FeatureSamples_Localization_Localization_NestedPage_Type", "FeatureSamples/Localization/Localization_NestedPage_Type", "Views/FeatureSamples/Localization/Localization_NestedPage_Type.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("FeatureSamples_ParameterBinding_OptionalParameterBinding", "FeatureSamples/ParameterBinding/OptionalParameterBinding/{Id?}", "Views/FeatureSamples/ParameterBinding/OptionalParameterBinding.dothtml");
            config.RouteTable.Add("FeatureSamples_ParameterBinding_OptionalParameterBinding2", "FeatureSamples/ParameterBinding/OptionalParameterBinding2/{Id?}", "Views/FeatureSamples/ParameterBinding/OptionalParameterBinding.dothtml", new { Id = 300 });
            config.RouteTable.Add("FeatureSamples_Validation_Localization", "FeatureSamples/Validation/Localization", "Views/FeatureSamples/Validation/Localization.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));
            config.RouteTable.Add("FeatureSamples_Localization_Globalize", "FeatureSamples/Localization/Globalize", "Views/FeatureSamples/Localization/Globalize.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));

            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));

            config.RouteTable.Add("ControlSamples_Repeater_RouteLinkQuery-PageDetail", "ControlSamples/Repeater/RouteLinkQuery/{Id}", "Views/ControlSamples/Repeater/RouteLinkQuery.dothtml", new { Id = 0 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectionHelpers_PageB-PageDetail", "FeatureSamples/Redirect/RedirectionHelpers_PageB/{Id}", "Views/FeatureSamples/Redirect/RedirectionHelpers_PageB.dothtml", new { Id = 22 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectionHelpers_PageC-PageDetail", "FeatureSamples/Redirect/RedirectionHelpers_PageC/{Id}", "Views/FeatureSamples/Redirect/RedirectionHelpers_PageC.dothtml", new { Id = 33 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectionHelpers_PageD-PageDetail", "FeatureSamples/Redirect/RedirectionHelpers_PageD/{Id}", "Views/FeatureSamples/Redirect/RedirectionHelpers_PageD.dothtml", new { Id = 44 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectionHelpers_PageE-PageDetail", "FeatureSamples/Redirect/RedirectionHelpers_PageE/{Id}", "Views/FeatureSamples/Redirect/RedirectionHelpers_PageE.dothtml", new { Id = 55 });
            config.RouteTable.Add("RepeaterRouteLink-PageDetail", "ControlSamples/Repeater/RouteLink/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml", new { Id = 0 });
            config.RouteTable.Add("RepeaterRouteLink-PageDetail_IdOptional", "ControlSamples/Repeater/RouteLink/{Id?}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLink-PageDetail_IdOptionalPrefixed", "ControlSamples/Repeater/RouteLink/id-{Id?}", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLink-PageDetail_IdOptionalAtStart", "id-{Id?}/ControlSamples/Repeater/RouteLink", "Views/ControlSamples/Repeater/RouteLink.dothtml");
            config.RouteTable.Add("RepeaterRouteLinkUrlSuffix-PageDetail", "ControlSamples/Repeater/RouteLinkUrlSuffix/{Id}", "Views/ControlSamples/Repeater/RouteLink.dothtml", new { Id = 0 });
            config.RouteTable.Add("FeatureSamples_Redirect_RedirectFromPresenter", "FeatureSamples/Redirect/RedirectFromPresenter", provider => new ViewModels.FeatureSamples.Redirect.RedirectingPresenter());
            config.RouteTable.Add("FeatureSamples_Validation_ClientSideValidationDisabling2", "FeatureSamples/Validation/ClientSideValidationDisabling/{ClientSideValidationEnabled}", "Views/FeatureSamples/Validation/ClientSideValidationDisabling.dothtml", new { ClientSideValidationEnabled = false });
            config.RouteTable.Add("FeatureSamples_EmbeddedResourceControls_EmbeddedResourceView", "FeatureSamples/EmbeddedResourceControls/EmbeddedResourceView", "embedded://EmbeddedResourceControls/EmbeddedResourceView.dothtml");
            config.RouteTable.Add("FeatureSamples_PostBack_PostBackHandlers_Localized", "FeatureSamples/PostBack/PostBackHandlers_Localized", "Views/FeatureSamples/PostBack/ConfirmPostBackHandler.dothtml", presenterFactory: LocalizablePresenter.BasedOnQuery("lang"));

            config.RouteTable.Add("Errors_UndefinedRouteLinkParameters-PageDetail", "Erros/UndefinedRouteLinkParameters/{Id}", "Views/UndefinedRouteLinkParameters.dothtml", new { Id = 0 });
            config.RouteTable.Add("Errors_Routing_NonExistingView", "Errors/Routing/NonExistingView", "Views/Errors/Routing/NonExistingView.dothml");
        }

        private static void AddControls(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("cc", typeof(Controls.ServerSideStylesControl));
            config.Markup.AddCodeControls("cc", typeof(Controls.TextRepeater));
            config.Markup.AddCodeControls("cc", typeof(DerivedControlUsageValidationTestControl));
            config.Markup.AddCodeControls("PropertyUpdate", typeof(Controls.ServerRenderedLabel));
            config.Markup.AddMarkupControl("IdGeneration", "Control", "Views/FeatureSamples/IdGeneration/IdGeneration_control.dotcontrol");
            config.Markup.AddMarkupControl("FileUploadInRepeater", "FileUploadWrapper", "Views/ComplexSamples/FileUploadInRepeater/FileUploadWrapper.dotcontrol");
            config.Markup.AddMarkupControl("cc", "RecursiveTextRepeater", "Views/FeatureSamples/PostBack/RecursiveTextRepeater.dotcontrol");
            config.Markup.AddMarkupControl("cc", "RecursiveTextRepeater2", "Views/FeatureSamples/PostBack/RecursiveTextRepeater2.dotcontrol");
            config.Markup.AddMarkupControl("cc", "ModuleControl", "Views/FeatureSamples/ViewModules/ModuleControl.dotcontrol");
            config.Markup.AddMarkupControl("cc", "Incrementer", "Views/FeatureSamples/ViewModules/Incrementer.dotcontrol");
            config.Markup.AddMarkupControl("cc", "TemplatedListControl", "Views/ControlSamples/TemplateHost/TemplatedListControl.dotcontrol");
            config.Markup.AddMarkupControl("cc", "TemplatedMarkupControl", "Views/ControlSamples/TemplateHost/TemplatedMarkupControl.dotcontrol");
            config.Markup.AddCodeControls("cc", typeof(Loader));
            config.Markup.AddMarkupControl("sample", "EmbeddedResourceControls_Button", "embedded://EmbeddedResourceControls/Button.dotcontrol");

            config.Markup.AutoDiscoverControls(new DefaultControlRegistrationStrategy(config, "sample", "Views/"));

        }

        public void ConfigureServices(IDotvvmServiceCollection options)
        {
            CommonConfiguration.ConfigureServices(options);
        }
    }
}
