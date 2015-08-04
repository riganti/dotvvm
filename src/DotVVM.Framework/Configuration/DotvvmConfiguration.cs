using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Parser;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using DotVVM.Framework.Styles;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmConfiguration
    {
        public const string DotvvmControlTagPrefix = "dot";
        
        /// <summary>
        /// Gets or sets the application physical path.
        /// </summary>
        [JsonIgnore]
        public string ApplicationPhysicalPath { get; set; }

        /// <summary>
        /// Gets the settings of the markup.
        /// </summary>
        [JsonProperty("markup", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmMarkupConfiguration Markup { get; private set; }

        /// <summary>
        /// Gets the route table.
        /// </summary>
        [JsonIgnore]
        public DotvvmRouteTable RouteTable { get; private set; }

        /// <summary>
        /// Gets the configuration of resources.
        /// </summary>
        [JsonProperty("resources", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(ResourceRepositoryJsonConverter))]
        public DotvvmResourceRepository Resources { get; private set; }

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        [JsonProperty("security")]
        public DotvvmSecurityConfiguration Security { get; private set; }

        /// <summary>
        /// Gets the runtime configuration.
        /// </summary>
        [JsonProperty("runtime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmRuntimeConfiguration Runtime { get; private set; }

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        [JsonProperty("defaultCulture", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Gets or sets whether the application should run in debug mode.
        /// </summary>
        [JsonProperty("debug", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [System.ComponentModel.DefaultValue(true)]
        public bool Debug { get; set; }

        /// <summary>
        /// Gets an instance of the service locator component.
        /// </summary>
        [JsonIgnore]
        public ServiceLocator ServiceLocator { get; private set; }

        [JsonIgnore]
        public StyleRepository Styles { get; set; }




        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmConfiguration"/> class.
        /// </summary>
        internal DotvvmConfiguration()
        {
            DefaultCulture = Thread.CurrentThread.CurrentCulture.Name;
            Markup = new DotvvmMarkupConfiguration();
            RouteTable = new DotvvmRouteTable(this);
            Resources = new DotvvmResourceRepository();
            Security = new DotvvmSecurityConfiguration();
            Runtime = new DotvvmRuntimeConfiguration();
            Debug = true;
            Styles = new StyleRepository();
        }

        /// <summary>
        /// Creates the default configuration.
        /// </summary>
        public static DotvvmConfiguration CreateDefault()
        {
            var configuration = new DotvvmConfiguration();

            configuration.ServiceLocator = new ServiceLocator();
            configuration.ServiceLocator.RegisterSingleton<IViewModelProtector>(() => new DefaultViewModelProtector());
            configuration.ServiceLocator.RegisterSingleton<ICsrfProtector>(() => new DefaultCsrfProtector());
            configuration.ServiceLocator.RegisterSingleton<IDotvvmViewBuilder>(() => new DefaultDotvvmViewBuilder(configuration));
            configuration.ServiceLocator.RegisterSingleton<IViewModelLoader>(() => new DefaultViewModelLoader());
            configuration.ServiceLocator.RegisterSingleton<IViewModelSerializer>(() => new DefaultViewModelSerializer(configuration) { SendDiff = true });
            configuration.ServiceLocator.RegisterSingleton<IOutputRenderer>(() => new DefaultOutputRenderer());
            configuration.ServiceLocator.RegisterSingleton<IDotvvmPresenter>(() => new DotvvmPresenter(configuration));
            configuration.ServiceLocator.RegisterSingleton<IMarkupFileLoader>(() => new DefaultMarkupFileLoader());
            configuration.ServiceLocator.RegisterSingleton<IControlBuilderFactory>(() => new DefaultControlBuilderFactory(configuration));
            configuration.ServiceLocator.RegisterSingleton<IControlResolver>(() => new DefaultControlResolver(configuration));
            configuration.ServiceLocator.RegisterSingleton<IControlTreeResolver>(() => new ControlTreeResolver(configuration));
            configuration.ServiceLocator.RegisterTransient<IViewCompiler>(() => new DefaultViewCompiler(configuration));
            configuration.ServiceLocator.RegisterSingleton<IBindingCompiler>(() => new BindingCompiler());
            configuration.ServiceLocator.RegisterSingleton<IBindingParser>(() => new BindingParser());

            configuration.Runtime.GlobalFilters.Add(new ModelValidationFilterAttribute());
            
            configuration.Markup.Controls.AddRange(new[]
            {
                new DotvvmControlConfiguration() { TagPrefix = "dot", Namespace = "DotVVM.Framework.Controls", Assembly = "DotVVM.Framework" },
                new DotvvmControlConfiguration() { TagPrefix = "bootstrap", Namespace = "DotVVM.Framework.Controls.Bootstrap", Assembly = "DotVVM.Framework" },
            });

            configuration.Resources.Register(Constants.JQueryResourceName,
                new ScriptResource()
                {
                    CdnUrl = "https://code.jquery.com/jquery-2.1.1.min.js",
                    Url = "DotVVM.Framework.Resources.Scripts.jquery-2.1.1.min.js",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "$"
                });
            configuration.Resources.Register(Constants.KnockoutJSResourceName,
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.knockout-3.2.0.js",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "ko"
                });
            configuration.Resources.Register(Constants.DotvvmResourceName,
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.js",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "dotvvm",
                    Dependencies = new[] { Constants.KnockoutJSResourceName }
                });
            configuration.Resources.Register(Constants.DotvvmValidationResourceName,
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.Validation.js",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "dotvvm.validation",
                    Dependencies = new[] { Constants.DotvvmResourceName }
                });
            configuration.Resources.Register(Constants.DotvvmDebugResourceName,
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.Debug.js",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    Dependencies = new[] { Constants.DotvvmResourceName, Constants.JQueryResourceName }
                });
            configuration.Resources.Register(Constants.BootstrapResourceName,
                new ScriptResource()
                {
                    Url = "~/Scripts/bootstrap.min.js",
                    CdnUrl = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js",
                    GlobalObjectName = "typeof $().emulateTransitionEnd == 'function'",
                    Dependencies = new[] { Constants.BootstrapCssResourceName, Constants.JQueryResourceName }
                });
            configuration.Resources.Register(Constants.BootstrapCssResourceName,
                new StylesheetResource()
                {
                    Url = "~/Content/bootstrap.min.css"
                });

            configuration.Resources.Register(Constants.DotvvmFileUploadResourceName, 
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.FileUpload.js",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    Dependencies = new[] { Constants.DotvvmResourceName }
                });
            configuration.Resources.Register(Constants.DotvvmFileUploadCssResourceName,
                new StylesheetResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.FileUpload.css",
                    EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name,
                    Dependencies = new[] { Constants.DotvvmFileUploadResourceName }
                });

            RegisterGlobalizeResources(configuration);

            return configuration;
        }


        private static void RegisterGlobalizeResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(Constants.GlobalizeResourceName, new ScriptResource()
            {
                Url = "DotVVM.Framework.Resources.Scripts.Globalize.globalize.js",
                EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name
            });


            configuration.Resources.RegisterNamedParent("globalize", new JQueryGlobalizeResourceRepository());
        }
        
    }
}
