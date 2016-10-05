using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;

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
        [JsonProperty("routes")]
        [JsonConverter(typeof(RouteTableJsonConverter))]
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
        public bool Debug { get; set; }

        /// <summary>
        /// Whether DotVVM compiler should generate runtime debug info for bindings. It can be useful, but may also cause unexpected problems.
        /// </summary>
        public bool AllowBindingDebugging { get; set; }

        /// <summary>
        /// Gets an instance of the service locator component.
        /// </summary>
        [JsonIgnore]
        public ServiceLocator ServiceLocator { get; private set; }

        [JsonIgnore]
        public StyleRepository Styles { get; set; }

        [JsonProperty("compiledViewsAssemblies")]
        public List<string> CompiledViewsAssemblies { get; set; } = new List<string>() { "CompiledViews.dll" };

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
            Styles = new StyleRepository();
        }

        /// <summary>
        /// Creates the default configuration.
        /// </summary>
        public static DotvvmConfiguration CreateDefault()
        {
            var configuration = new DotvvmConfiguration();

            InitDefaultServices(configuration);

            configuration.Runtime.GlobalFilters.Add(new ModelValidationFilterAttribute());
            configuration.Markup.Controls.AddRange(new[]
            {
                new DotvvmControlConfiguration() { TagPrefix = "dot", Namespace = "DotVVM.Framework.Controls", Assembly = "DotVVM.Framework" }
            });

            RegisterResources(configuration);

            return configuration;
        }

        private static void RegisterResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(ResourceConstants.JQueryResourceName,
                new ScriptResource()
                {
                    CdnUrl = "https://code.jquery.com/jquery-2.1.1.min.js",
                    Url = "DotVVM.Framework.Resources.Scripts.jquery-2.1.1.min.js",
                    EmbeddedResourceAssembly = typeof (DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "$"
                });
            configuration.Resources.Register(ResourceConstants.KnockoutJSResourceName,
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.knockout-latest.js",
                    EmbeddedResourceAssembly = typeof (DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "ko"
                });

            configuration.Resources.Register(ResourceConstants.DotvvmResourceName + ".internal",
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.js",
                    EmbeddedResourceAssembly = typeof (DotvvmConfiguration).Assembly.GetName().Name,
                    GlobalObjectName = "dotvvm",
                    Dependencies = new[] { ResourceConstants.KnockoutJSResourceName }
                });
            configuration.Resources.Register(ResourceConstants.DotvvmResourceName,
                new InlineScriptResource()
                {
                    Code = @"if (window.dotvvm) { throw 'DotVVM is already loaded!'; } window.dotvvm = new DotVVM();",
                    Dependencies = new[] { ResourceConstants.DotvvmResourceName + ".internal" }
                });

            configuration.Resources.Register(ResourceConstants.DotvvmDebugResourceName,
                new ScriptResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.Debug.js",
                    EmbeddedResourceAssembly = typeof (DotvvmConfiguration).Assembly.GetName().Name,
                    Dependencies = new[] { ResourceConstants.DotvvmResourceName, ResourceConstants.JQueryResourceName }
                });

            configuration.Resources.Register(ResourceConstants.DotvvmFileUploadCssResourceName,
                new StylesheetResource()
                {
                    Url = "DotVVM.Framework.Resources.Scripts.DotVVM.FileUpload.css",
                    EmbeddedResourceAssembly = typeof (DotvvmConfiguration).Assembly.GetName().Name
                });

            RegisterGlobalizeResources(configuration);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void InitDefaultServices(DotvvmConfiguration configuration)
        {
            configuration.ServiceLocator = new ServiceLocator();
            configuration.ServiceLocator.RegisterSingleton<IViewModelProtector>(() => new DefaultViewModelProtector(configuration));
            configuration.ServiceLocator.RegisterSingleton<ICsrfProtector>(() => new DefaultCsrfProtector(configuration));
            configuration.ServiceLocator.RegisterSingleton<IDotvvmViewBuilder>(() => new DefaultDotvvmViewBuilder(configuration));
            configuration.ServiceLocator.RegisterSingleton<IViewModelLoader>(() => new DefaultViewModelLoader());
            configuration.ServiceLocator.RegisterSingleton<IViewModelValidationMetadataProvider>(() => new AttributeViewModelValidationMetadataProvider());
            configuration.ServiceLocator.RegisterSingleton<IValidationRuleTranslator>(() => new ViewModelValidationRuleTranslator());
            configuration.ServiceLocator.RegisterSingleton<IViewModelValidator>(() => new ViewModelValidator(configuration.ServiceLocator.GetService<IViewModelSerializationMapper>()));
            configuration.ServiceLocator.RegisterSingleton<IViewModelSerializationMapper>(() => new ViewModelSerializationMapper(configuration));
            configuration.ServiceLocator.RegisterSingleton<IViewModelSerializer>(() => new DefaultViewModelSerializer(configuration) { SendDiff = true });
            configuration.ServiceLocator.RegisterSingleton<IOutputRenderer>(() => new DefaultOutputRenderer());
            configuration.ServiceLocator.RegisterSingleton<IDotvvmPresenter>(() => new DotvvmPresenter(configuration));
            configuration.ServiceLocator.RegisterSingleton<IMarkupFileLoader>(() => new DefaultMarkupFileLoader());
            configuration.ServiceLocator.RegisterSingleton<IControlBuilderFactory>(() => new DefaultControlBuilderFactory(configuration));
            configuration.ServiceLocator.RegisterSingleton<IControlResolver>(() => new DefaultControlResolver(configuration));
            configuration.ServiceLocator.RegisterSingleton<IControlTreeResolver>(() => new DefaultControlTreeResolver(configuration));
            configuration.ServiceLocator.RegisterSingleton<IAbstractTreeBuilder>(() => new ResolvedTreeBuilder());
            configuration.ServiceLocator.RegisterTransient<IViewCompiler>(() => new DefaultViewCompiler(configuration));
            configuration.ServiceLocator.RegisterSingleton<IBindingCompiler>(() => new BindingCompiler(configuration));
            configuration.ServiceLocator.RegisterSingleton<IBindingExpressionBuilder>(() => new BindingExpressionBuilder());
            configuration.ServiceLocator.RegisterSingleton<IBindingIdGenerator>(() => new OriginalStringBindingIdGenerator());
            configuration.ServiceLocator.RegisterSingleton<IControlUsageValidator>(() => new DefaultControlUsageValidator());
            configuration.ServiceLocator.RegisterSingleton<IDictionary<string, IRouteParameterType>>(() => new DefaultRouteParameterTypes());
        }


        private static void RegisterGlobalizeResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(ResourceConstants.GlobalizeResourceName, new ScriptResource()
            {
                Url = "DotVVM.Framework.Resources.Scripts.Globalize.globalize.js",
                EmbeddedResourceAssembly = typeof(DotvvmConfiguration).Assembly.GetName().Name
            });

            configuration.Resources.RegisterNamedParent("globalize", new JQueryGlobalizeResourceRepository());
        }
        
    }
}
