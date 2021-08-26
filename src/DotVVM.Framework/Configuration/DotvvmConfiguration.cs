#nullable enable
using System;
using System.Collections.Generic;
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
using System.Globalization;
using System.Reflection;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Compilation.Javascript;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Framework.Configuration
{
    public sealed class DotvvmConfiguration
    {
        private bool isFrozen;
        public const string DotvvmControlTagPrefix = "dot";

        /// <summary>
        /// Gets or sets the application physical path.
        /// </summary>
        [JsonProperty("applicationPhysicalPath")]
        [DefaultValue(".")]
        public string ApplicationPhysicalPath
        {
            get { return _applicationPhysicalPath; }
            set { ThrowIfFrozen(); _applicationPhysicalPath = value; }
        }
        private string _applicationPhysicalPath = "."; // defaults to current working directory. In practice this gets overridden by app initializer

        /// <summary>
        /// Gets the settings of the markup.
        /// </summary>
        [JsonProperty("markup")]
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
        [JsonProperty("resources")]
        [JsonConverter(typeof(ResourceRepositoryJsonConverter))]
        public DotvvmResourceRepository Resources { get; private set; } = new DotvvmResourceRepository();

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        [JsonProperty("security")]
        public DotvvmSecurityConfiguration Security { get; private set; } = new DotvvmSecurityConfiguration();

        /// <summary>
        /// Gets the runtime configuration.
        /// </summary>
        [JsonProperty("runtime")]
        public DotvvmRuntimeConfiguration Runtime { get; private set; } = new DotvvmRuntimeConfiguration();

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        [JsonProperty("defaultCulture")]
        public string DefaultCulture
        {
            get { return _defaultCulture; }
            set { ThrowIfFrozen(); _defaultCulture = value; }
        }
        private string _defaultCulture;

        /// <summary>
        /// Gets or sets whether the client side validation rules should be enabled.
        /// </summary>
        [JsonProperty("clientSideValidation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool ClientSideValidation
        {
            get { return _clientSideValidation; }
            set { ThrowIfFrozen(); _clientSideValidation = value; }
        }
        private bool _clientSideValidation = true;

        /// <summary>
        /// Gets or sets whether navigation in the SPA pages should use History API. Default value is <c>true</c>.
        /// </summary>
        [JsonProperty("useHistoryApiSpaNavigation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        [Obsolete("The UseHistoryApiSpaNavigation property is not supported - the classic SPA mode (URLs with #/) was removed from DotVVM, and the History API is the default and only option now. See https://www.dotvvm.com/docs/3.0/pages/concepts/layout/single-page-applications-spa#changes-to-spas-in-dotvvm-30 for more details.")]
        public bool UseHistoryApiSpaNavigation
        {
            get { return _useHistoryApiSpaNavigation; }
            set { ThrowIfFrozen(); _useHistoryApiSpaNavigation = value; }
        }
        private bool _useHistoryApiSpaNavigation = true;

        /// <summary>
        /// Gets or sets the configuration for experimental features.
        /// </summary>
        [JsonProperty("experimentalFeatures")]
        public DotvvmExperimentalFeaturesConfiguration ExperimentalFeatures
        {
            get => _experimentalFeatures;
            set { ThrowIfFrozen(); _experimentalFeatures = value; }
        }
        private DotvvmExperimentalFeaturesConfiguration _experimentalFeatures = new DotvvmExperimentalFeaturesConfiguration();

        /// <summary>
        /// Gets or sets whether the application should run in debug mode.
        /// For ASP.NET Core check out <see cref="!:https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments" >https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments</see>
        /// </summary>
        [JsonProperty("debug", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Debug
        {
            get => _debug;
            set { ThrowIfFrozen(); _debug = value; }
        }
        private bool _debug;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw new InvalidOperationException("DotvvmConfiguration cannot be modified after initialization by IDotvvmStartup.");
        }

        /// <summary>
        /// Prevent from changes.
        /// </summary>
        public void Freeze()
        {
            isFrozen = true;
            Markup.Freeze();
            RouteTable.Freeze();
            Resources.Freeze();
            Runtime.Freeze();
            Security.Freeze();
            ExperimentalFeatures.Freeze();
            _routeConstraints.Freeze();
            Styles.Freeze();
            FreezableList.Freeze(ref _compiledViewsAssemblies);
        }

        [JsonIgnore]
        public IDictionary<string, IRouteParameterConstraint> RouteConstraints => _routeConstraints;
        private readonly FreezableDictionary<string, IRouteParameterConstraint> _routeConstraints = new FreezableDictionary<string, IRouteParameterConstraint>();

        /// <summary>
        /// Whether DotVVM compiler should generate runtime debug info for bindings. It can be useful, but may also cause unexpected problems.
        /// </summary>
        [JsonIgnore]
        public bool AllowBindingDebugging
        {
            get { return _allowBindingDebugging; }
            set { ThrowIfFrozen(); _allowBindingDebugging = value; }
        }
        private bool _allowBindingDebugging;

        /// <summary>
        /// Gets an instance of the service locator component.
        /// </summary>
        [JsonIgnore]
        [Obsolete("You probably want to use ServiceProvider")]
        public ServiceLocator ServiceLocator { get; private set; }

        [JsonIgnore]
        public IServiceProvider ServiceProvider { get; private set; }

        [JsonIgnore]
        public StyleRepository Styles
        {
            get { return _styles; }
            set { ThrowIfFrozen(); _styles = value; }
        }
        private StyleRepository _styles;

        [JsonProperty("compiledViewsAssemblies")]
        public IList<string> CompiledViewsAssemblies
        {
            get { return _compiledViewsAssemblies; }
            set { ThrowIfFrozen(); _compiledViewsAssemblies = value; }
        }
        private IList<string> _compiledViewsAssemblies = new FreezableList<string>() { "CompiledViews.dll" };

        /// <summary> must be there for serialization </summary>
        internal DotvvmConfiguration(): this(new ServiceLocator(CreateDefaultServiceCollection().BuildServiceProvider()).GetServiceProvider())
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmConfiguration"/> class.
        /// </summary>
        internal DotvvmConfiguration(IServiceProvider services)
        {
            ServiceProvider = services;
#pragma warning disable
            ServiceLocator = new ServiceLocator(services);
#pragma warning restore

            _defaultCulture = CultureInfo.CurrentCulture.Name;
            Markup = new DotvvmMarkupConfiguration(new Lazy<JavascriptTranslatorConfiguration>(() => ServiceProvider.GetRequiredService<IOptions<JavascriptTranslatorConfiguration>>().Value));
            RouteTable = new DotvvmRouteTable(this);
            _styles = new StyleRepository(this);

        }

        private static ServiceCollection CreateDefaultServiceCollection()
        {
            var services = new ServiceCollection();
            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);
            return services;
        }

        /// <summary>
        /// Creates the default configuration and optionally registers additional application services.
        /// </summary>
        /// <param name="registerServices">An action to register additional services.</param>
        /// <param name="serviceProviderFactoryMethod">Register factory method to create your own instance of IServiceProvider.</param>
        public static DotvvmConfiguration CreateDefault(Action<IServiceCollection>? registerServices = null, Func<IServiceCollection, IServiceProvider>? serviceProviderFactoryMethod = null)
        {
            var services = CreateDefaultServiceCollection();
            registerServices?.Invoke(services);
            return new ServiceLocator(services, serviceProviderFactoryMethod).GetService<DotvvmConfiguration>();
        }

        /// <summary>
        /// Creates the default configuration using the given service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve services from.</param>
        public static DotvvmConfiguration CreateDefault(IServiceProvider serviceProvider)
        {
            var config = new DotvvmConfiguration(serviceProvider);

            config.Runtime.GlobalFilters.Add(new ModelValidationFilterAttribute());

            config.Markup.Controls.Add(new DotvvmControlConfiguration() { TagPrefix = "dot", Namespace = "DotVVM.Framework.Controls", Assembly = "DotVVM.Framework" });

            RegisterConstraints(config);
            RegisterResources(config);

            ConfigureOptions(config.RouteTable, serviceProvider);
            ConfigureOptions(config.Markup, serviceProvider);
            ConfigureOptions(config.Resources, serviceProvider);
            ConfigureOptions(config.Runtime, serviceProvider);
            ConfigureOptions(config.Security, serviceProvider);
            ConfigureOptions(config.Styles, serviceProvider);
            ConfigureOptions(config, serviceProvider);

            return config;
        }

        /// <summary>
        /// Creates a configuration with fake services in place of hosting-specific components.
        /// </summary>
        internal static DotvvmConfiguration CreateInternal(Action<IServiceCollection> registerServices)
        {
            return CreateDefault(services =>
            {
                services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                services.TryAddSingleton<ICsrfProtector, FakeCsrfProtector>();
                registerServices(services);
            });
        }

        private static void ConfigureOptions<T>(T obj, IServiceProvider serviceProvider)
            where T : class
        {
            foreach (var conf in serviceProvider.GetServices<IConfigureOptions<T>>())
            {
                conf.Configure(obj);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        static void RegisterConstraints(DotvvmConfiguration configuration)
        {
            configuration.RouteConstraints.Add("alpha", GenericRouteParameterType.Create("[a-zA-Z]*?"));
            configuration.RouteConstraints.Add("bool", GenericRouteParameterType.Create<bool>("true|false", bool.TryParse));
            configuration.RouteConstraints.Add("decimal", GenericRouteParameterType.Create<decimal>("-?[0-9.e]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("double", GenericRouteParameterType.Create<double>("-?[0-9.e]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("float", GenericRouteParameterType.Create<float>("-?[0-9.e]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("guid", GenericRouteParameterType.Create<Guid>("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", Guid.TryParse));
            configuration.RouteConstraints.Add("int", GenericRouteParameterType.Create<int>("-?[0-9]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("posint", GenericRouteParameterType.Create<int>("[0-9]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("length", new GenericRouteParameterType(p => "[^/]{" + p + "}"));
            configuration.RouteConstraints.Add("long", GenericRouteParameterType.Create<long>("-?[0-9]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("max", new GenericRouteParameterType(p => "-?[0-9.e]*?", (valueString, parameter) => {
                if (parameter is null) throw new Exception("The `max` route constraint must have a numeric parameter.");
                double value;
                if (!Invariant.TryParse(valueString, out value)) return ParameterParseResult.Failed;
                if (double.Parse(parameter, CultureInfo.InvariantCulture) < value) return ParameterParseResult.Failed;
                return ParameterParseResult.Create(value);
            }));
            configuration.RouteConstraints.Add("min", new GenericRouteParameterType(p => "-?[0-9.e]*?", (valueString, parameter) => {
                if (parameter is null) throw new Exception("The `min` route constraint must have a numeric parameter.");
                double value;
                if (!Invariant.TryParse(valueString, out value)) return ParameterParseResult.Failed;
                if (double.Parse(parameter, CultureInfo.InvariantCulture) > value) return ParameterParseResult.Failed;
                return ParameterParseResult.Create(value);
            }));
            configuration.RouteConstraints.Add("range", new GenericRouteParameterType(p => "-?[0-9.e]*?", (valueString, parameter) => {
                if (parameter is null) throw new Exception("The `range` route constraint must have two numeric parameters.");
                double value;
                if (!Invariant.TryParse(valueString, out value)) return ParameterParseResult.Failed;
                var split = parameter.Split(',');
                if (double.Parse(split[0], CultureInfo.InvariantCulture) > value || double.Parse(split[1], CultureInfo.InvariantCulture) < value) return ParameterParseResult.Failed;
                return ParameterParseResult.Create(value);
            }));
            configuration.RouteConstraints.Add("maxLength", new GenericRouteParameterType(p => "[^/]{0," + p + "}"));
            configuration.RouteConstraints.Add("minLength", new GenericRouteParameterType(p => "[^/]{" + p + ",}"));
            configuration.RouteConstraints.Add("regex", new GenericRouteParameterType(p => {
                if (p is null) throw new Exception("The `regex` route constraint must have a parameter.");
                if (p.StartsWith("^")) throw new ArgumentException("Regex in route constraint should not start with `^`, it's always looking for full-match.");
                if (p.EndsWith("$")) throw new ArgumentException("Regex in route constraint should not end with `$`, it's always looking for full-match.");
                return p;
            }));
        }

        private static void RegisterResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(ResourceConstants.PolyfillBundleResourceName,
                new ScriptModuleResource(location: null,
                    nomoduleLocation: new EmbeddedResourceLocation(
                        typeof(DotvvmConfiguration).Assembly,
                            "DotVVM.Framework.obj.javascript.polyfill.bundle.js")
                    ));

            configuration.Resources.Register(ResourceConstants.KnockoutJSResourceName,
                new ScriptResource(location: new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).Assembly,
                    "DotVVM.Framework.Resources.Scripts.knockout-latest.js",
                    debugName: "DotVVM.Framework.Resources.Scripts.knockout-latest.debug.js")));

            configuration.Resources.Register(ResourceConstants.DotvvmResourceName + ".internal",
                new ScriptModuleResource(
                    location: new EmbeddedResourceLocation(
                        typeof(DotvvmConfiguration).Assembly,
                        "DotVVM.Framework.obj.javascript.root_only.dotvvm-root.js",
                        debugName: "DotVVM.Framework.obj.javascript.root_only_debug.dotvvm-root.js"),
                    nomoduleLocation: new EmbeddedResourceLocation(
                        typeof(DotvvmConfiguration).Assembly,
                        "DotVVM.Framework.obj.javascript.root_only_system.dotvvm-root.js",
                        debugName: "DotVVM.Framework.obj.javascript.root_only_system_debug.dotvvm-root.js")
                    ) {
                    Dependencies = new[] { ResourceConstants.KnockoutJSResourceName, ResourceConstants.PolyfillBundleResourceName }
                });
            configuration.Resources.Register(ResourceConstants.DotvvmResourceName + ".internal-spa",
                new ScriptModuleResource(
                    location: new EmbeddedResourceLocation(
                        typeof(DotvvmConfiguration).Assembly,
                        "DotVVM.Framework.obj.javascript.root_spa.dotvvm-root.js",
                        debugName: "DotVVM.Framework.obj.javascript.root_spa_debug.dotvvm-root.js"),
                    nomoduleLocation: new EmbeddedResourceLocation(
                        typeof(DotvvmConfiguration).Assembly,
                        "DotVVM.Framework.obj.javascript.root_spa_system.dotvvm-root.js",
                        debugName: "DotVVM.Framework.obj.javascript.root_spa_system_debug.dotvvm-root.js")
                    ) {
                    Dependencies = new[] { ResourceConstants.KnockoutJSResourceName, ResourceConstants.PolyfillBundleResourceName }
                });
            configuration.Resources.Register(ResourceConstants.DotvvmResourceName,
                new InlineScriptResource(@"", ResourceRenderPosition.Anywhere, defer: true) {
                    Dependencies = new[] { ResourceConstants.DotvvmResourceName + ".internal" }
                });

            configuration.Resources.Register(ResourceConstants.DotvvmDebugResourceName,
                new ScriptResource(location: new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).Assembly,
                    "DotVVM.Framework.Resources.Scripts.DotVVM.Debug.js")) {
                    Dependencies = new[] { ResourceConstants.DotvvmResourceName }
                });

            configuration.Resources.Register(ResourceConstants.DotvvmFileUploadCssResourceName,
                new StylesheetResource(new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).Assembly,
                    "DotVVM.Framework.Resources.Styles.DotVVM.FileUpload.css")));

            RegisterGlobalizeResources(configuration);
        }

        private static void RegisterGlobalizeResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(ResourceConstants.GlobalizeResourceName,
                new ScriptResource(location: new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).Assembly,
                    "DotVVM.Framework.Resources.Scripts.Globalize.globalize.min.js")));

            configuration.Resources.RegisterNamedParent("globalize", new JQueryGlobalizeResourceRepository());
        }
    }
}
